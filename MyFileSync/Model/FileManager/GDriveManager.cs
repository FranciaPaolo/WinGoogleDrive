using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileSyncLib
{
    public class GDriveManager: IFileManager
    {
        private static string[] Scopes = { DriveService.Scope.Drive };//, DriveService.Scope.DriveFile, DriveService.Scope.DriveMetadata };        
        private UserCredential credential;
        private DriveService service;
        private int? maxRequestResultItems = 10000;
        private List<GDriveItem> cachedFileList;
        private File baseFolder;

        private GDriveManager()
        { }

        private void authenticate()
        {
            using (var stream =new System.IO.FileStream("client_secret.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = System.IO.Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
        }
        
        public static GDriveManager create(string appName)
        {
            GDriveManager gutil = new GDriveManager();
            gutil.authenticate();

            // Create Drive API service.
            gutil.service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = gutil.credential,
                ApplicationName = appName,
            });

            return gutil; 
        }


        public System.IO.Stream GetFileStream(ISyncItem syncItem)
        {
            if (syncItem.IsFolder())
                return null;

            try
            {
                if (!(syncItem is GDriveItem))
                    throw new Exception("Unknown file type");

                GDriveItem gdriveItem = (GDriveItem)syncItem;
                //System.IO.Stream stream= new System.IO.MemoryStream();

                //var request = service.Files.Get(gdriveItem.GetBaseItem().Id);
                //request.Download(stream);

                //return stream;


                var request = service.HttpClient.GetStreamAsync(gdriveItem.GetBaseItem().DownloadUrl);
                var stream = request.Result;
                return stream;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }

        
        private IList<Google.Apis.Drive.v2.Data.File> getFoldersByTitle(string title)
        {
            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = "title = '"+title+"' and mimeType = 'application/vnd.google-apps.folder'";            
            listRequest.MaxResults = 60;

            // List files.
            IList<Google.Apis.Drive.v2.Data.File> files = listRequest.Execute().Items;
            return files;
        }

        private File getCachedFolder(string folderRelativePath)
        {
            if (folderRelativePath == "" || folderRelativePath == "\\")
                return baseFolder;
            return cachedFileList.Single(w => w.IsFolder() && w.GetItemFullName() == folderRelativePath).GetBaseItem();
        }
        
        public IList<ISyncItem> getFilesAndFoldersInPath(string path)
        {
            string title = path.Substring(path.LastIndexOf("\\")+1);
            baseFolder = getFoldersByTitle(title).Single();
            
            string folderId = baseFolder.Id;//TODO object reference not set...
            //TODO gestire la possibilità che ci siano più folder con lo stesso nome

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = "'" + folderId + "' in parents and trashed=false";
            listRequest.MaxResults = maxRequestResultItems;

            // List files and direcories
            List<GDriveItem> resultList = listRequest.Execute().Items.Select(s => new GDriveItem(s, "")).ToList();

            // scan subfolders
            List<GDriveItem> subFolders = resultList.Where(w => w.IsFolder()).ToList();

            while (subFolders.Count > 0)
            {
                GDriveItem currentSubFolder = subFolders.FirstOrDefault();
                subFolders.Remove(currentSubFolder);

                FilesResource.ListRequest listSubFolderRequest = service.Files.List();
                listSubFolderRequest.Q = "'" + currentSubFolder.GetBaseItem().Id + "' in parents and trashed=false";
                listSubFolderRequest.MaxResults = maxRequestResultItems;
                IList<GDriveItem> subItems = listSubFolderRequest.Execute().Items.Select(s => new GDriveItem(s, currentSubFolder.GetItemPath() + "\\" + currentSubFolder.GetItemName())).ToList();

                resultList.AddRange(subItems);

                // ricorsione
                subFolders.AddRange(subItems.Where(w => w.IsFolder()).ToList());
            }

            cachedFileList = resultList;
            return resultList.Cast<ISyncItem>().ToList();
        }

        public void UpdateFile(ISyncItem syncItemFrom, ISyncItem syncItemTo, System.IO.Stream stream)
        {
            File folder = getCachedFolder(syncItemTo.GetItemPath());

            if (!syncItemTo.IsFolder())
            {
                GDriveItem gdriveItem = (GDriveItem)syncItemFrom;

                service.Files.Delete(gdriveItem.GetBaseId()).Execute();

                File body = new File();
                body.Title = syncItemTo.GetItemName();
                body.MimeType = syncItemTo.GetMimeType();
                body.ModifiedDate = syncItemTo.GetLastUpdateDate();
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = folder.Id } };

                var request = service.Files.Insert(body, stream, syncItemTo.GetMimeType());
                request.Upload();
                File uploadedFile = request.ResponseBody;
                GDriveItem uploadedGdriveItem = new GDriveItem(uploadedFile, gdriveItem.GetItemPath());

                cachedFileList.Remove(gdriveItem);
                cachedFileList.Add(uploadedGdriveItem);                
            }
        }
        public void CreateFileOrFolder(ISyncItem syncItem, System.IO.Stream stream)
        {
            // se la directory non esiste la creo
            // ...

            File folder = getCachedFolder(syncItem.GetItemPath());

            if (syncItem.IsFolder())
            {
                Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
                body.Title = syncItem.GetItemName();
                body.Description = "description";
                body.MimeType = "application/vnd.google-apps.folder";                
                body.Parents = new List<ParentReference>() { new ParentReference() { Id = folder.Id } };

                File updloadedFolder=service.Files.Insert(body).Execute();
                
                cachedFileList.Add(new GDriveItem(updloadedFolder, syncItem.GetItemPath()));
            }
            else
            {
                File body = new File();
                body.Title = syncItem.GetItemName();                
                body.MimeType = syncItem.GetMimeType();
                body.ModifiedDate = syncItem.GetLastUpdateDate();
                body.Parents = new List<ParentReference>(){ new ParentReference() {Id = folder.Id} };
                
                var request = service.Files.Insert(body, stream, syncItem.GetMimeType());                
                request.Upload();
                File uploadedFile = request.ResponseBody;
                cachedFileList.Add(new GDriveItem(uploadedFile, syncItem.GetItemPath()));
            }
        }

        public void RemoveFileOrFolder(ISyncItem syncItem)
        {
            throw new NotImplementedException();
        }
    }
}
