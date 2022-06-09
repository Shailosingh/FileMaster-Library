/*
------------------------------------------------------------------------------------------
File:    FileMaster.cs
Purpose: This acts as an example for how to use the FileMaster Library. Keep in mind that
while this is a console application, this library can be used in a GUI program.
==========================================================================================
Program Description:
FileMaster Library is offers fast and efficient methods to split up files into smaller 
chunks and join them back up in C#. This is useful for a variety of use cases, such as:

1. Circumventing the upload limit of various websites/webapps (see the Discord Filemaster
on my GitHub)

2. Putting a large file on multiple smaller storage mediums (Floppy Disks, CD's, DVD's, etc)

Good luck with your coding 😊!
------------------------------------------------------------------------------------------
Author:  Shailendra Singh
Version  2022-02-24
------------------------------------------------------------------------------------------
*/

using FileMasterLibrary;
using System.Diagnostics;

namespace FileMasterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //Test out File Splitter
            string inputFile = @"C:\Users\compu\Videos\The Heathers.mp4";
            string outputFolder = @"C:\Users\compu\Videos\Piece Folder";
            int pieceSize = 8 * (int)Math.Pow(1000, 2); //8 MB
            StartSplit(inputFile, outputFolder, pieceSize);
            
            
            //Test out File Joiner
            string inputFolderPath = @"C:\Users\compu\Videos\Piece Folder";
            string outputFolderPath = inputFolderPath; //Send joined file to same directory as where pieces are
            string outputFileName = "The Heathers Musical";
            string outputExtension = "MP4";
            StartJoin(inputFolderPath, outputFolderPath, outputFileName, outputExtension);
            
        }

        public static void StartSplit(string inputFile, string outputFolder, int pieceSize)
        {
            Console.WriteLine("Starting Split");
            Console.WriteLine("______________");
            Console.WriteLine($"Input File: {inputFile}");
            Console.WriteLine($"Output Folder: {outputFolder}\n");
            FileMaster fileMasterInstance = new FileMaster();

            Console.WriteLine("Starting up Split Thread");
            Console.WriteLine("________________________");
            Thread splitThread = new Thread(() =>fileMasterInstance.FileSplit(inputFile, outputFolder, pieceSize));
            Stopwatch timer = new Stopwatch();
            timer.Start();
            splitThread.Start();

            Console.WriteLine("Validating...");
            while (!fileMasterInstance.IsFinished && !fileMasterInstance.IsValidated) ;

            //Check for errors and exit
            if(fileMasterInstance.ExitError)
            {
                Console.WriteLine($"ERROR: {fileMasterInstance.StatusMessage}");
                return;
            }

            Console.WriteLine("Validation Successful!");
            Console.WriteLine($"Total Number Of Pieces: {fileMasterInstance.TotalFiles}");

            long currentNumFilesProcessed = 0;
            while(!fileMasterInstance.IsFinished)
            {
                //Check if the number of files processed has changed
                if(currentNumFilesProcessed != fileMasterInstance.NumFilesProcessed)
                {
                    currentNumFilesProcessed = fileMasterInstance.NumFilesProcessed;
                    Console.WriteLine($"Pieces Created: {currentNumFilesProcessed}/{fileMasterInstance.TotalFiles}");
                }
            }

            //Check for errors and exit
            if(fileMasterInstance.ExitError)
            {
                Console.WriteLine($"\nERROR: {fileMasterInstance.StatusMessage}");
                return;
            }

            //Split must be over
            timer.Stop();
            Console.WriteLine("\nSPLIT COMPLETE!");
            Console.WriteLine($"Took {timer.ElapsedMilliseconds} milliseconds.");
        }

        public static void StartJoin(string inputFolderPath, string outputFolderPath, string outputFileName, string outputExtension)
        {
            Console.WriteLine("Starting Join");
            Console.WriteLine("_____________");
            Console.WriteLine($"Input Folder: {inputFolderPath}");
            Console.WriteLine($"Output Folder: {outputFolderPath}\n");
            Console.WriteLine($"Desired new file: {outputFileName}.{outputExtension}");
            FileMaster fileMasterInstance = new FileMaster();

            Console.WriteLine("Starting up Join Thread");
            Console.WriteLine("________________________");
            Thread joinThread = new Thread(() => fileMasterInstance.FileJoin(inputFolderPath, outputFolderPath, outputFileName, outputExtension));
            Stopwatch timer = new Stopwatch();
            timer.Start();
            joinThread.Start();

            Console.WriteLine("Validating...");
            while (!fileMasterInstance.IsFinished && !fileMasterInstance.IsValidated) ;

            //Check for errors and exit
            if (fileMasterInstance.ExitError)
            {
                Console.WriteLine($"ERROR: {fileMasterInstance.StatusMessage}");
                return;
            }

            Console.WriteLine("Validation Successful!");
            Console.WriteLine($"Total Number Of Pieces: {fileMasterInstance.TotalFiles}");

            long currentNumFilesProcessed = 0;
            while (!fileMasterInstance.IsFinished)
            {
                //Check if the number of files processed has changed
                if (currentNumFilesProcessed != fileMasterInstance.NumFilesProcessed)
                {
                    currentNumFilesProcessed = fileMasterInstance.NumFilesProcessed;
                    Console.WriteLine($"Pieces Joined: {currentNumFilesProcessed}/{fileMasterInstance.TotalFiles}");
                }
            }

            //Check for errors and exit
            if (fileMasterInstance.ExitError)
            {
                Console.WriteLine($"\nERROR: {fileMasterInstance.StatusMessage}");
                return;
            }

            //Split must be over
            timer.Stop();
            Console.WriteLine("\nJOIN COMPLETE!");
            Console.WriteLine($"Took {timer.ElapsedMilliseconds} milliseconds.");
        }
    }
}