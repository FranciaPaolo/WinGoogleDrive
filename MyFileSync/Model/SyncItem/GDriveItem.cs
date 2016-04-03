using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileSyncLib
{
    public class GDriveItem : ISyncItem
    {
        private File item;
        private string path;
        private bool isFolder;

        public GDriveItem(File item, string path)
        {
            this.item = item;
            this.path = path;
            isFolder = false;
            if (item.MimeType.ToLower() == "application/vnd.google-apps.folder")
            {
                isFolder = true;
                this.path = path;
            }
            
        }

        public string GetItemPath()
        {
            return path;
        }

        public string GetItemFullName()
        {
            return path+"\\"+item.Title;
        }

        public string GetItemName()
        {
            return item.Title;
        }

        public bool IsFolder()
        {
            return isFolder;
        }

        public DateTime GetLastUpdateDate()
        {
            return item.ModifiedDate.Value;
        }

        public string GetMimeType()
        {
            return item.MimeType;
        }






        public File GetBaseItem()
        {
            return item;
        }

        public string GetBaseId()
        {
            return this.item.Id;
        }

        public override string ToString()
        {
            return GetItemFullName();
        }
    }
}
