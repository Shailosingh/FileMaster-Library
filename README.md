# FileMaster-Library
The FileMaster Library offers fast and efficient methods to split up files into smaller chunks and join them back up in C#. This is useful for a variety of use cases, such as:

1. Circumventing the upload limit of various websites/webapps (see the Discord Filemaster on my GitHub)

2. Putting a large file on multiple smaller storage mediums (Floppy Disks, CD's, DVD's, etc)

## Getting started
This library is very simple to use. If you simply wish to split or join files on the same calling thread, you can just initialize the FileMaster object and invoke the methods ```FileSplit(string inputFilePath, string outputFolderPath, int pieceSize)``` or ```FileJoin(string inputFolderPath, string outputFolderPath, string outputFileName, string outputExtension)``` respectively. At the end of its execution, you can check if there was an error using the FileMaster object's ```ExitError``` property. If ```ExitError``` is ```true```, then there must be an error and the error code is given by the property ```StatusMessage```.

However, if the file you wish to split is very large, there is a large amount of files you wish to join or you are doing these operations in a GUI project, it is not wise to do this work on the single calling thread. If you try to do intense work in a GUI project from the GUI thread, the program will become unresponsive. Not only this but, the FileMaster object has a whole host of properties that tell the user the progress the program is making on splitting or joining your files. These are totally inaccessible if you simply do this work on a single thread. Therefore, it is recommended that you use this library using multiple threads. While this is more complicated, I have provided examples of both Console and WinForms projects on how to use this library correctly:

[Console Example](https://github.com/Shailosingh/FileMaster-Library/blob/master/FileMasterConsole/Program.cs)

[WinForms Example](https://github.com/Shailosingh/DiscordFileMaster)

[WinUI 3 Example](https://github.com/Shailosingh/DiscordFileMasterV2)

[NuGet Package Download](https://www.nuget.org/packages/FileMasterLibrary)
