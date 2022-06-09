/*
------------------------------------------------------------------------------------------
File:    FileMaster.cs
Purpose: This class splits files into pieces using the "FileSplit" method and joins them
back together using the "FileJoin" method. 

Both methods may be started as new threads so the task can be done in the background and the 
UI does not become totally frozen while the splits or joins occurs.

The properties of this class can be accessed by the thread calling the method, to detect 
when the operation had completed validation, completed operation the number of files done 
processing, if there are any errorsand what the error message is.
==========================================================================================
Program Description:
The FileMaster Library offers fast and efficient methods to split up files into smaller 
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

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMasterLibrary
{
    public class FileMaster
    {
        //Properties
        public bool IsValidated { get; private set; }
        public bool IsFinished { get; private set; }
        public bool ExitError { get; private set; }
        public string StatusMessage { get; private set; }
        public long NumFilesProcessed { get; private set; }
        public long TotalFiles { get; private set; }

        //Constants
        long MAX_PIECE_SIZE = int.MaxValue;
        
        //Constructor
        public FileMaster()
        {
            IsValidated = false;
            IsFinished = false;
            StatusMessage = "";
            ExitError = false;
            NumFilesProcessed = 0;
            TotalFiles = 0;
        }

        //Public methods-------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Takes the file specified by the "inputFilePath", splits it up into file pieces of
        /// size "pieceSize" and writes these chunks to the "outputFolderPath"
        /// </summary>
        /// <param name="inputFilePath">Path to the file to be split up.</param>
        /// <param name="outputFolderPath">The folder where the pieces will be written to.</param>
        /// <param name="pieceSize">The size of each piece in bytes.</param>
        public void FileSplit(string inputFilePath, string outputFolderPath, int pieceSize)
        {
            //Reset class properties so it has a clean slate
            ResetProperties();

            //Ensure that the input file exists
            if(!File.Exists(inputFilePath))
            {
                SetupErrorMessage($"Input file ({inputFilePath}) does not exist.");
                return;
            }

            //Ensure that the output folder exists
            if(!Directory.Exists(outputFolderPath))
            {
                SetupErrorMessage($"Output folder path ({outputFolderPath}) does not exist.");
                return;
            }

            //Ensure that the piece size is in the correct range
            if(!(pieceSize > 0 && pieceSize <= MAX_PIECE_SIZE))
            {
                SetupErrorMessage("The file piece size must be greater than 0");
                return;
            }

            //Calculate total number of pieces, the input shall be split into
            FileInfo inputFileInfo = new FileInfo(inputFilePath);
            long fileSize = inputFileInfo.Length;
            long numberOfFiles = fileSize / pieceSize;
            if (fileSize % pieceSize != 0 || fileSize == 0)
            {
                numberOfFiles++;
            }
            TotalFiles = numberOfFiles;

            //VALIDATION IS OVER, so signal the start of splitting
            IsValidated = true;

            //Get the correct file name length (the left part will be padded with 0's depending on how many files there are)
            int fileNameLength = (int)Math.Ceiling(Math.Log10(numberOfFiles));
            if (fileNameLength == 0)
            {
                fileNameLength = 1;
            }

            //Open up input file
            using (FileStream currentInputStream = File.OpenRead(inputFilePath))
            {
                //Cycle through all writing to all pieces
                for (int fileIndex = 0; fileIndex < numberOfFiles; fileIndex++)
                {
                    //Get current piece name
                    string currentPieceName = $@"{fileIndex.ToString().PadLeft(fileNameLength, '0')}.piece";

                    //Get current piece directory
                    string currentPieceDirectory = Path.Combine(outputFolderPath, currentPieceName);

                    //If it is the last piece to be created and it isn't exactly the size of pieceSize, change it
                    if (fileIndex == numberOfFiles - 1 && fileSize % pieceSize != 0)
                    {
                        pieceSize = (int)fileSize % pieceSize;
                    }

                    //Write piece file
                    using (FileStream currentPieceStream = File.OpenWrite(currentPieceDirectory))
                    {
                        CopyBlockOfBytesTo(currentInputStream, currentPieceStream, pieceSize);
                    }

                    //Completed processing piece
                    NumFilesProcessed++;
                }
            }

            //Finished splitting
            IsFinished = true;
        }

        /// <summary>
        /// Takes the folder at "inputFolderPath," joins together all the .piece files within it into one big file with the name
        /// "outputFileName," extension "outputExtension" and writes the file to the folder pointed to at "outputFolderPath"
        /// </summary>
        /// <param name="inputFolderPath">The folder where the input .piece files are</param>
        /// <param name="outputFolderPath">The folder where the joined file will be written</param>
        /// <param name="outputFileName">The name of the new joined file</param>
        /// <param name="outputExtension">The extension of the new joined file</param>
        public void FileJoin(string inputFolderPath, string outputFolderPath, string outputFileName, string outputExtension)
        {
            //Reset class properties so it has a clean slate
            ResetProperties();

            //Ensure that the input folder exists
            if (!Directory.Exists(inputFolderPath))
            {
                SetupErrorMessage($"Input folder path ({inputFolderPath}) does not exist.");
                return;
            }

            //Ensure that the output folder exists
            if(!Directory.Exists(outputFolderPath))
            {
                SetupErrorMessage($"Output folder path ({outputFolderPath}) does not exist.");
                return;
            }

            //Ensure that there is at least one piece file in the input folder
            string[] pieceFiles = Directory.GetFiles(inputFolderPath, "*.piece");
            long numberOfPieceFiles = pieceFiles.Length;
            if (numberOfPieceFiles == 0)
            {
                SetupErrorMessage("No .piece files given");
                return;
            }
            TotalFiles = numberOfPieceFiles;

            //Sort the list of piece files by filename
            Array.Sort(pieceFiles);

            //Get the correct file name length (the left part will be padded with 0's depending on how many files there are)
            int fileNameLength = (int)Math.Ceiling(Math.Log10(numberOfPieceFiles));
            if (fileNameLength == 0)
            {
                fileNameLength = 1;
            }

            //Ensure that all piece files in input folder have the correct title/directory
            for (int fileIndex = 0; fileIndex < numberOfPieceFiles; fileIndex++)
            {
                //Setup the correct name for this 
                string correctName = $@"{fileIndex.ToString().PadLeft(fileNameLength, '0')}.piece";

                //Get correct directory
                string correctDirectory = Path.Combine(inputFolderPath, correctName);

                //Compare with the actual directory. If not equal, then "correctName" must be missing from the input folder
                if(!pieceFiles[fileIndex].Equals(correctDirectory))
                {
                    SetupErrorMessage($"Invalid file inputs. Expected ({correctDirectory}), got ({pieceFiles[fileIndex]})");
                    return;
                }

            }

            //Validation Complete
            IsValidated = true;

            //Try to get create the joined file
            try
            {
                //Get directory of joined file to be made
                string joinedFileName = $@"{outputFileName}.{outputExtension}";
                string joinedFilePath = Path.Combine(outputFolderPath, joinedFileName);

                //Create joined file
                using(FileStream joinedFileStream = File.OpenWrite(joinedFilePath))
                {
                    //Cycle through all the pieces and join em together
                    for (int fileIndex = 0; fileIndex < numberOfPieceFiles; fileIndex++)
                    {
                        //Open current piece file and attach it to joined file
                        using (FileStream pieceFileStream = File.OpenRead(pieceFiles[fileIndex]))
                        {
                            //Get the piece size, ensure it is small enough and set it as the transfer buffer size
                            long pieceSize = pieceFileStream.Length;
                            if(!(pieceSize > 0 && pieceSize <= MAX_PIECE_SIZE))
                            {
                                joinedFileStream.Dispose();
                                File.Delete(joinedFilePath);
                                SetupErrorMessage($"Joining failed! {pieceFileStream.Name} is of bad size");
                                return;
                            }
                            pieceFileStream.CopyTo(joinedFileStream, (int)pieceSize);
                        }

                        //Current piece file done processing
                        NumFilesProcessed++;
                    }
                }
            }
            catch (Exception e)
            {
                SetupErrorMessage($"{e.Message}");
                return;
            }

            //Joining complete :)
            IsFinished = true;
        }

        //Private Methods------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Signals that the FileMaster is exiting its operation (split or join) with an error
        /// and giving an error message for the "StatusMessage"
        /// </summary>
        /// <param name="errorMessage">The error message to be passed out of the FileMaster</param>
        private void SetupErrorMessage(string errorMessage)
        {
            ExitError = true;
            IsFinished = true;
            StatusMessage = errorMessage;
        }

        /// <summary>
        /// Copies a specified number of bytes from the source stream (must be readable) into the destination stream 
        /// (must be writable). The positions for both streams will be advanced by the number of bytes copied
        /// </summary>
        /// <param name="sourceStream">The stream that will be copied from (Must be open to reading)</param>
        /// <param name="destinationStream">The stream that will have bytes copied into it (Must be open to writing)</param>
        /// <param name="numberOfBytes">The number of bytes that shall be copied</param>
        private void CopyBlockOfBytesTo(FileStream sourceStream, FileStream destinationStream, int numberOfBytes)
        {
            byte[] transferBuffer = new byte[numberOfBytes];
            sourceStream.Read(transferBuffer, 0, numberOfBytes);
            destinationStream.Write(transferBuffer);
        }

        /// <summary>
        /// Gives "this" object default properties so, it can be given a clean slate before attempting a file split or join
        /// </summary>
        private void ResetProperties()
        {
            IsValidated = false;
            IsFinished = false;
            StatusMessage = "";
            ExitError = false;
            NumFilesProcessed = 0;
            TotalFiles = 0;
        }
    }
}
