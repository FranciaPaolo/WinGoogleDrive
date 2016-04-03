using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSyncLib
{
    public class FileSystemItem : ISyncItem
    {
        private FileInfo fileInfo;
        private DirectoryInfo directoryInfo;
        private string path;
        private bool isFolder;
        private string itemName;

        public FileSystemItem(string fullFileItemPath, string baseItemPath, bool isFolder)
        {
            this.isFolder = isFolder;
            path = fullFileItemPath.Replace(baseItemPath, "");// costruisco una sorta di path relativo
            path = path.Substring(0, path.LastIndexOf("\\"));// rimuovo il nome del file/cartella
            if (isFolder)
            {
                directoryInfo = new DirectoryInfo(fullFileItemPath);
                itemName = directoryInfo.Name;
                isFolder = true;
            }
            else {
                fileInfo = new FileInfo(fullFileItemPath);
                itemName = fileInfo.Name;
            }

        }

        public string GetItemFullName()
        {
            return path + "\\" + itemName;
        }

        public string GetItemName()
        {
            return itemName;
        }

        public string GetItemPath()
        {
            return path;
        }

        public DateTime GetLastUpdateDate()
        {
            if (isFolder)
                return directoryInfo.LastWriteTime;
            else
                return fileInfo.LastWriteTime;
        }

        public bool IsFolder()
        {
            return isFolder;
        }

        public string GetMimeType()
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileInfo.FullName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }


        public override string ToString()
        {
            return GetItemFullName();
        }
    }
}
