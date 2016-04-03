using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSyncLib
{
    public interface IFileManager
    {
        IList<ISyncItem> getFilesAndFoldersInPath(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncItemFrom">File da aggiornare</param>
        /// <param name="syncItemTo">File aggiornato</param>
        /// <param name="stream"></param>
        void UpdateFile(ISyncItem syncItemFrom, ISyncItem syncItemTo, System.IO.Stream stream);
        void CreateFileOrFolder(ISyncItem syncItem, System.IO.Stream file);
        void RemoveFileOrFolder(ISyncItem syncItem);

        System.IO.Stream GetFileStream(ISyncItem syncItem);
    }
}
