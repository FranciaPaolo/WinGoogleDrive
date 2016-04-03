# WinGoogleDrive
The synchronization make use of two file manager (one for left space and one for right space) to get the list of files and folders.

Initialize:
=======
First we need to create the SyncManager:
```
 SyncManager syncManager = new SyncManager(
    leftFileManager: GDriveManager.create(gDriveApplicationName),
    rightFileManager: new FileSystemManager()
 );
 ```
 Note in the example above we used:
 * the Google Drive manager, in left space, to get the list of files from google drive
 * File System manager, in right space, to get the list of file from a local folder

Compare folder: 
=======
To get the list of changes between left spce and right space:
```
List<CompareItem> syncResult = syncManager.CompareFolder("GDriveFolder1\SubFolderToSync", "C:\folderToSync");
```
The compare method use the modification date to determine which is more recent (sure in a future release it will be improved).
The list will contain all compared files ex

| Change           | Google Drive                | Local File System           |
| ---------------- |:---------------------------:| ---------------------------:|
| UpdatedInRight   | \file1.txt 01/01/2016 14:24 | \file1.txt 01/04/2016 10:05 |
| ExistsOnlyInLeft | \file2.txt 10/02/2016 16:25 |                             |



Sync :
=======
SyncManager has two sync methods:
* SyncItemToLeft to apply in the Left space the changes
* SyncItemToRight to apply in the Right space the changes
