using FileSyncLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        // TO START YOU NEED: https://developers.google.com/drive/v2/web/quickstart/dotnet#step_1_turn_on_the_api_name
        // in Google Developers Console turn on "Drive API"
        // select OAuth consent screen an Email address and enter a Product name 
        // select Credentials tab, click the Add credentials button and select OAuth 2.0 client ID.
        // Select the application type Other, enter the name od the application es "Other client 1"
        // Download the "client_secret.json" put in this project folder and set the Copy to Output Directory field to Copy always.


        // configurations
        static string gDriveApplicationName = "Other client 1";

        static SyncFolder[] synFolders = {
            new SyncFolder() {
                gDriveFolderPath=@"MyWiki\Repository\myWikiImages",
                fileSystemFolderPath= @"C:\Personale\myWiki 8.5\root\images"
            },
        };

        static string compareOutPutFile = @"./out.csv";
        static SyncManager syncManager;

        static void Main(string[] args)
        {
            syncManager = new SyncManager(
                leftFileManager: GDriveManager.create(gDriveApplicationName),
                rightFileManager: new FileSystemManager()
            );

            while (true)
            {
                ShowHelp();
                
                string[] arguments = Console.ReadLine().Split(' ');
                if (arguments.Length > 0)
                {
                    List<string> options = arguments.ToList();
                    options.RemoveAt(0);
                    switch (arguments[0].ToLower())
                    {
                        case "compare":
                            Compare(options);
                            break;
                        case "sync":
                            Sync(options);
                            break;
                        default:
                            ShowHelp();
                            break;
                    }
                }
                else
                    ShowHelp();
            }
        }

        public static string getItemString(ISyncItem syncItem)
        {
            if (syncItem != null && syncItem is GDriveItem && syncItem.IsFolder())
                return syncItem.GetItemFullName() + ";" + syncItem.GetLastUpdateDate();
            else if (syncItem != null)
                return syncItem.GetItemFullName() + ";" + syncItem.GetLastUpdateDate();
            else
                return ";";
        }

        public static void ShowHelp()
        {
            Console.WriteLine("\nType \n\"compare\" to start compare operation");
            Console.WriteLine("\toption \"e\" to export to excel");
            Console.WriteLine("\toption \"a\" to show all compared items");

            Console.WriteLine("\"sync\" to sync files and folders");            
            Console.WriteLine("");
        }

        public static void Compare(List<string> options)
        {
            foreach (SyncFolder synFolder in synFolders)
            {
                Console.WriteLine("\nComparing...\nLEFT:" + synFolder.gDriveFolderPath + "\nRIGHT:" + synFolder.fileSystemFolderPath);
                bool showCompareDetails = options.Contains("a");

                synFolder.syncResult = syncManager.CompareFolder(synFolder.gDriveFolderPath, synFolder.fileSystemFolderPath);

                StreamWriter sw = null;
                if (options.Contains("e"))
                    sw = new StreamWriter(compareOutPutFile);

                foreach (CompareItem syncItem in synFolder.syncResult)
                {
                    if (showCompareDetails)
                        Console.WriteLine("{0}", syncItem.ToString());
                    if (sw != null)
                        sw.WriteLine(syncItem.getDifferenceStatus + ";" + getItemString(syncItem.ItemLeft) + ";" + getItemString(syncItem.ItemRight) + ";");
                }
                if (sw != null)
                    sw.Close();

                int totalLeft = synFolder.syncResult.Count(c => c.ItemLeft != null);
                int totalRight = synFolder.syncResult.Count(c => c.ItemRight != null);
                int countUpdatedInLeft = synFolder.syncResult.Count(c => c.getDifferenceStatus == CompareItem.DifferenceStatus.UpdatedInLeft);
                int countExistsOnlyInLeft = synFolder.syncResult.Count(c => c.getDifferenceStatus == CompareItem.DifferenceStatus.ExistsOnlyInLeft);
                int countUpdatedInRight = synFolder.syncResult.Count(c => c.getDifferenceStatus == CompareItem.DifferenceStatus.UpdatedInRight);
                int countExistsOnlyInRight = synFolder.syncResult.Count(c => c.getDifferenceStatus == CompareItem.DifferenceStatus.ExistsOnlyInRight);
                int countNoDifference = synFolder.syncResult.Count(c => c.getDifferenceStatus == CompareItem.DifferenceStatus.NoDifference);

                Console.WriteLine("\nCompare summary:");
                Console.WriteLine("\n- Total GDrive files:{0}", totalLeft);
                Console.WriteLine("- Total Local files:{0}", totalRight);
                Console.WriteLine("- Total changes from GDrive files: updated {0} new {1}", countUpdatedInLeft, countExistsOnlyInLeft);
                Console.WriteLine("- Total changes from Local files: updated {0} new {1}", countUpdatedInRight, countExistsOnlyInRight);
                Console.WriteLine("- Total identical files: {0}", countNoDifference);
            }
        }

        public static void Sync(List<string> options)
        {
            foreach (SyncFolder synFolder in synFolders)
            {
                Console.WriteLine("\nSync...\nLEFT:" + synFolder.gDriveFolderPath + "\nRIGHT:" + synFolder.fileSystemFolderPath);

                if (synFolder.syncResult == null)
                {
                    Console.WriteLine("Error: you must do a compare before sync");
                    return;
                }

                Console.WriteLine("\n\nSync To Right: Local file system");
                // Sync file that exists only in local
                IList<CompareItem> itemsToSyncToRight = synFolder.syncResult.
                    Where(w => w.getDifferenceStatus == CompareItem.DifferenceStatus.ExistsOnlyInLeft
                    || w.getDifferenceStatus == CompareItem.DifferenceStatus.UpdatedInLeft).ToList();
                foreach (CompareItem syncItem in itemsToSyncToRight)
                {
                    Console.WriteLine("SyncItemToRight {0}", syncItem.ItemLeft.GetItemFullName());
                    syncManager.SyncItemToRight(syncItem);
                }
                Console.WriteLine("Sync To Right Completed");

                Console.WriteLine("\n\nSync To Left: Google Drive");
                // Sync file that exists only in local
                IList<CompareItem> itemsToSyncToLeft = synFolder.syncResult.
                    Where(w => w.getDifferenceStatus == CompareItem.DifferenceStatus.ExistsOnlyInRight
                    || w.getDifferenceStatus == CompareItem.DifferenceStatus.UpdatedInRight).ToList();
                foreach (CompareItem syncItem in itemsToSyncToLeft)
                {
                    Console.WriteLine("SyncItemToLeft {0}", syncItem.ItemRight.GetItemFullName());
                    syncManager.SyncItemToLeft(syncItem);
                }
                Console.WriteLine("Sync To Left Completed");
            }
        }


        class SyncFolder
        {
            public string gDriveFolderPath;
            public string fileSystemFolderPath;
            public List<CompareItem> syncResult;
        }
    }

}