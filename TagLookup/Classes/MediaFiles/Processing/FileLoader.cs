using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific
using System.IO;

namespace TagLookup.Classes.MediaFiles.Processing
{
    class FileLoader
    {

        public FileLoader()
        {
            listOfFileInfoArrays = new List<FileInfo []>( );
            listOfDirectoryInfoArrays = new List<DirectoryInfo []>( );

            Directories = new List<DirectoryInfo>( );
            Files = new List<string>( );
            InitializeTagLibExtensions( );
            ExceptionLog = new List<Tuple<string, string>>( );
        }

        // prefered constructor
        public FileLoader( string directoryPath )
        {
            listOfFileInfoArrays = new List<FileInfo []>( );
            listOfDirectoryInfoArrays = new List<DirectoryInfo []>( );

            Directories = new List<DirectoryInfo>( );
            Files = new List<string>( );
            InitializeTagLibExtensions( );
            ExceptionLog = new List<Tuple<string, string>>( );

            ReadFolder( directoryPath );
        }


        public void ReadFolder( string folderPath )
        {
            ProcessDirectoryFiles( folderPath, ProcessFile );
        }


        // hack: clears existing files for each processing
        public List<string> GetTemporaryFileNames(string baseDirectory)
        {
            List<string> fileNames = new List<string>( );
            Files.Clear( );
            ReadFolder( baseDirectory );
            fileNames = Files;
            return fileNames;
        }

        // Attempts to process every directory belonging in a root directory by applying 
        // the void ProcessFile function on each processed directory. 
        // *
        // Swallows and logs all failed attempts in ExceptionLog, which contains
        // the directorypath and the exception of each failed attempt
        void ProcessDirectoryFiles( string folderPath, Action<string> fileAction )
        {
            foreach ( string file in Directory.GetFiles( folderPath ) )
            {
                fileAction( file );
            }
            foreach ( string subDirectory in Directory.GetDirectories( folderPath ) )
            {
                try
                {
                    ProcessDirectoryFiles( subDirectory, fileAction );
                }
                catch ( Exception ex )
                {
                    ExceptionLog.Add( new Tuple<string, string>( ex.Message + System.Environment.NewLine + ex.StackTrace, subDirectory ) );
                }
            }
        }

        // Create a new FileInfo from a filepath, adding it's filepath to List<string> Files
        // if it matches a desired extension type within List<string> Extensions
        // *
        // Swallows and logs all failed attempts in ExceptionLog, which contains
        // the filepath and the exception of each failed attempt
        void ProcessFile( string path )
        {
            try
            {
                FileInfo fileInfo = new FileInfo( path );
                IterateThroughExtensions( fileInfo );
            }
            catch ( Exception ex )
            {
                ExceptionLog.Add( new Tuple<string, string>( ex.Message + System.Environment.NewLine + ex.StackTrace, path ) );
            }
        }

        void IterateThroughExtensions( FileInfo fileInfo )
        {
            foreach ( string extension in Extensions )
            {
                if ( extension == fileInfo.Extension.ToLower( ) )
                {
                    Files.Add( fileInfo.FullName );
                    return;
                }
            }
        }

        // Adds all extensions supported by TagLibSharp to List<string> Extensions,
        // which acts as a list of accepted extensions
        void InitializeTagLibExtensions()
        {
            Extensions = new List<string> { ".mp3", ".flac", ".mpc", ".mpp", ".mp+", ".wv", ".spx", ".tta", 
                                            ".aiff", ".aif", ".aifc", ".mp4", ".m4a", ".m4p", ".m4b", ".m4r",
                                            ".m4v", ".asf", ".wma", ".wmv"    
                                          };
        }

        public List<string> Files;
        public List<DirectoryInfo> Directories;
        public List<string> Extensions;
        List<FileInfo []> listOfFileInfoArrays;
        List<DirectoryInfo []> listOfDirectoryInfoArrays;
        List<Tuple<string, string>> ExceptionLog;
    }
}
