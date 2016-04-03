using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSyncLib
{
    public interface ISyncItem
    {
        DateTime GetLastUpdateDate();
        string GetItemName();
        string GetItemFullName();
        string GetMimeType();
        bool IsFolder();
        string GetItemPath();


    }
}
