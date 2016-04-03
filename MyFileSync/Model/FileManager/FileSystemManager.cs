using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSyncLib
{
    public class FileSystemManager:IFileManager
    {
        private string path;

        private void CreateParentDirectoriesIfNotExists(ISyncItem syncItem)
        {
            if (!Directory.Exists(path + syncItem.GetItemPath()))
            {
                string[] folders = syncItem.GetItemPath().Split('\\');
                string tmpPath = path;
                for (int i = 0; i < folders.Length; i++)
                {
                    if (!Directory.Exists(tmpPath + folders[i]))
                        Directory.CreateDirectory(tmpPath + "\\" + folders[i]);
                    tmpPath += "\\" + folders[i];
                }
            }
        }

        public IList<ISyncItem> getFilesAndFoldersInPath(string path)
        {
            this.path = path;
            string[] allFiles = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
            string[] allDirectories = System.IO.Directory.GetDirectories(path, "*.*", System.IO.SearchOption.AllDirectories);

            string parentPath = path;//.Substring(0,path.LastIndexOf("\\"));

            List<FileSystemItem> resultList = new List<FileSystemItem>();
            foreach (string file in allFiles)
                resultList.Add(new FileSystemItem(file, parentPath, false));

            foreach (string directory in allDirectories)
                resultList.Add(new FileSystemItem(directory, parentPath, true));

            return resultList.Cast<ISyncItem>().ToList();
        }
        
        public void UpdateFile(ISyncItem syncItemFrom, ISyncItem syncItemTo, System.IO.Stream stream)
        {
            if (!Directory.Exists(path + syncItemFrom.GetItemPath()))
                CreateParentDirectoriesIfNotExists(syncItemFrom);

            if (syncItemFrom.IsFolder())
                Directory.CreateDirectory(path + syncItemFrom.GetItemFullName());
            else
            {
                File.Delete(path + syncItemFrom.GetItemFullName());
                using (var fileStream = new FileStream(path + syncItemFrom.GetItemFullName(), FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
                File.SetLastWriteTime(path + syncItemFrom.GetItemFullName(), syncItemTo.GetLastUpdateDate());
            }
        }
        public void CreateFileOrFolder(ISyncItem syncItem, Stream stream)
        {
            // se la directory non esiste la creo
            if (!Directory.Exists(path + syncItem.GetItemPath()))
                CreateParentDirectoriesIfNotExists(syncItem);

            if (syncItem.IsFolder())
                Directory.CreateDirectory(path + syncItem.GetItemFullName());
            else
            {
                using (var fileStream = new FileStream(path + syncItem.GetItemFullName(), FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
                File.SetLastWriteTime(path + syncItem.GetItemFullName(), syncItem.GetLastUpdateDate());
            }
        }
        public void RemoveFileOrFolder(ISyncItem syncItem)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream GetFileStream(ISyncItem syncItem)
        {
            if (syncItem.IsFolder())
                return null;

            try
            {
                if (!(syncItem is FileSystemItem))
                    throw new Exception("Unknown file type");

                FileSystemItem gdriveItem = (FileSystemItem)syncItem;
                System.IO.Stream stream = new MemoryStream(File.ReadAllBytes(path + syncItem.GetItemFullName()));//File.OpenRead(path + syncItem.GetItemFullName());

                return stream;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }
    }
}
