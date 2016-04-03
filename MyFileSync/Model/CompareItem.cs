using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSyncLib
{
    public class CompareItem
    {
        private ISyncItem _itemLeft;
        private ISyncItem _itemRight;
        private DifferenceStatus differenceStatus;

        public DifferenceStatus getDifferenceStatus { get { return this.differenceStatus; } }
        public ISyncItem ItemLeft { get { return _itemLeft; } }
        public ISyncItem ItemRight { get { return _itemRight; } }

        public CompareItem(ISyncItem itemLeft, ISyncItem itemRight)
        {
            this._itemLeft = itemLeft;
            this._itemRight = itemRight;

            if (itemLeft == null && itemRight == null)
                throw new Exception("Cannot be null both left item and right item");

            if (itemLeft == null)
                this.differenceStatus = DifferenceStatus.ExistsOnlyInRight;
            else if (itemRight == null)
                this.differenceStatus = DifferenceStatus.ExistsOnlyInLeft;
            else if (!itemLeft.IsFolder() && itemLeft.GetLastUpdateDate().TrimMilliseconds() > itemRight.GetLastUpdateDate().TrimMilliseconds())
                this.differenceStatus = DifferenceStatus.UpdatedInLeft;
            else if (!itemLeft.IsFolder() && itemRight.GetLastUpdateDate().TrimMilliseconds() > itemLeft.GetLastUpdateDate().TrimMilliseconds())
                this.differenceStatus = DifferenceStatus.UpdatedInRight;
            else
                this.differenceStatus = DifferenceStatus.NoDifference;
        }
        
        // stato: esiste solo su drive, esiste solo in locale, esiste in entrambi ma uno ha data più recente, esiste in entrambi con data uguale
        public enum DifferenceStatus { ExistsOnlyInLeft, ExistsOnlyInRight, NoDifference, UpdatedInLeft, UpdatedInRight }

        protected internal void SyncLeftToRight(IFileManager fileManagerLeft, IFileManager fileManagerRight)
        {
            switch (differenceStatus)
            {
                case DifferenceStatus.ExistsOnlyInLeft:
                    // create in right
                    fileManagerRight.CreateFileOrFolder(_itemLeft, fileManagerLeft.GetFileStream(_itemLeft));
                    break;
                case DifferenceStatus.ExistsOnlyInRight:
                    // remove in right
                    break;
                case DifferenceStatus.UpdatedInLeft:
                    // update in right
                    fileManagerRight.UpdateFile(_itemRight, _itemLeft, fileManagerLeft.GetFileStream(_itemLeft));
                    break;
                case DifferenceStatus.UpdatedInRight:
                    // update in left
                    fileManagerLeft.UpdateFile(_itemLeft, _itemRight, fileManagerRight.GetFileStream(_itemRight));
                    break;
                case DifferenceStatus.NoDifference:
                    break;
            }
        }

        protected internal void SyncRightToLeft(IFileManager fileManagerLeft, IFileManager fileManagerRight)
        {
            switch (differenceStatus)
            {
                case DifferenceStatus.ExistsOnlyInLeft:
                    // remove in right
                    break;
                case DifferenceStatus.ExistsOnlyInRight:
                    // create in left
                    fileManagerLeft.CreateFileOrFolder(_itemRight, fileManagerRight.GetFileStream(_itemRight));
                    break;
                case DifferenceStatus.UpdatedInLeft:
                    // update in right
                    fileManagerRight.UpdateFile(_itemRight, _itemLeft, fileManagerLeft.GetFileStream(_itemLeft));
                    break;
                case DifferenceStatus.UpdatedInRight:
                    // update in left
                    fileManagerLeft.UpdateFile(_itemLeft, _itemRight, fileManagerRight.GetFileStream(_itemRight));
                    break;
                case DifferenceStatus.NoDifference:
                    break;
            }
        }



        public override string ToString()
        {
            switch (differenceStatus)
            {
                case DifferenceStatus.ExistsOnlyInLeft:
                    return "ExistsOnlyInLeft\tLEFT: " + _itemLeft.GetItemFullName() + "\t" + _itemLeft.GetLastUpdateDate() + "\tRIGHT:";
                case DifferenceStatus.ExistsOnlyInRight:
                    return "ExistsOnlyInRight\tLEFT: \tRIGHT:" + _itemRight.GetItemFullName() + "\t" + _itemRight.GetLastUpdateDate();
                case DifferenceStatus.UpdatedInLeft:
                    return "UpdatedInLeft\tLEFT: " + _itemLeft.GetItemFullName() + "\t" + _itemLeft.GetLastUpdateDate() + "\tRIGHT:" + _itemRight.GetItemFullName() + "\t" + _itemRight.GetLastUpdateDate();
                case DifferenceStatus.UpdatedInRight:
                    return "UpdatedInRight\tLEFT: " + _itemLeft.GetItemFullName() + "\t" + _itemLeft.GetLastUpdateDate() + "\tRIGHT:" + _itemRight.GetItemFullName() + "\t" + _itemRight.GetLastUpdateDate();
                case DifferenceStatus.NoDifference:
                    return "NoDifference\tLEFT: " + _itemLeft.GetItemFullName() + "\t" + _itemLeft.GetLastUpdateDate() + "\tRIGHT:" + _itemRight.GetItemFullName() + "\t" + _itemRight.GetLastUpdateDate();
            }
            return base.ToString();
        }
    }
}