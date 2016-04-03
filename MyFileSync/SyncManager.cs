using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSyncLib
{
    public class SyncManager
    {
        private IFileManager leftFileManager;
        private IFileManager rightFileManager;

        public SyncManager(IFileManager leftFileManager, IFileManager rightFileManager)
        {
            if (leftFileManager == null || rightFileManager == null)
                throw new Exception("SyncManager argument cannot be null");
            this.leftFileManager = leftFileManager;
            this.rightFileManager = rightFileManager;
        }


        public List<CompareItem> CompareFolder(string leftPath, string rightPath)
        {
            // Get items
            List<ISyncItem> listLeft = leftFileManager.getFilesAndFoldersInPath(leftPath).OrderBy(o => o.GetItemFullName()).ToList();
            List<ISyncItem> listRight = rightFileManager.getFilesAndFoldersInPath(rightPath).OrderBy(o => o.GetItemFullName()).ToList();

            // Do compare
            List<ISyncItem> _listRight = new List<ISyncItem>(listRight);
            List<CompareItem> resultList = new List<CompareItem>();
            foreach (ISyncItem fileLeft in listLeft)
            {
                ISyncItem fileRight = _listRight.FirstOrDefault(f => f.GetItemFullName() == fileLeft.GetItemFullName());
                CompareItem syncItem = new CompareItem(fileLeft, fileRight);

                if (fileRight != null)
                    _listRight.Remove(fileRight);

                resultList.Add(syncItem);
            }

            // items remaining in _listRight are surely not in left
            foreach (ISyncItem fileRight in _listRight)
            {
                CompareItem syncItem = new CompareItem(null, fileRight);
                resultList.Add(syncItem);
            }
            return resultList;
        }

        public void SyncItemToLeft(CompareItem item)
        {
            item.SyncRightToLeft(leftFileManager, rightFileManager);
        }

        public void SyncItemToRight(CompareItem item)
        {
            item.SyncLeftToRight(leftFileManager, rightFileManager);
        }
    }
}
