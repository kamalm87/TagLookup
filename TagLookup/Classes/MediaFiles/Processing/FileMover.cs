using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific
using TagLookup.Classes.MediaFiles;

namespace TagLookup.Classes.MediaFiles.Processing
{
    // This class is intended to faciliate moving MediaFiles to an arbitrarily
    // assigned desired base directory and categorizing a given MediaFile
    // with a directory structure that currently supports the (optional) 
    // categorizations in any order the user desires:
    // * Album
    // * Artist
    // * Genre
    // * FileType
    class FileMover
    {
        public FileMover( List<MediaFile> mediaFiles, string [] folders )
        {
            mediaFilesToRelocate = mediaFiles;
            folderStructure = new FolderStructure( folders );
        }

        public void SetFolderStructure( string [] folderCategories )
        {
            folderStructure = new FolderStructure( folderCategories );
        }

        string getArtist( MediaFile mf )
        {
            return mf.Artist;
        }
        string getAlbum( MediaFile mf )
        {
            return mf.Album;
        }
        string getGenre( MediaFile mf )
        {
            return mf.Genre;
        }
        string getFileType( MediaFile mf )
        {
            return mf.FileType;
        }

        // Returns the value of the FilePath for a given item
        // ** Example **
        // Given: MediaFile: Artist: Artist1, Album: Album1, Genre: Genre1, FileType: MP3, FileName: SongTitle.mp3
        // Order: FileType -> Genre -> Artist -> Album
        // (External usage) Base Directory: C:/Music
        // Returns: MP3/Genre1/Artist1/Album1/
        // Helps Create: C:/Music/MP3/Genre1/Artist1/Album1/SongTitle.mp3
        public string FilePath( MediaFile mf )
        {
            string filePath = "";

            foreach ( string folder in folderStructure.FolderCategories )
            {
                if ( folder == "Artist" )
                {
                    filePath += mf.Artist + @"/";
                }
                if ( folder == "Album" )
                {
                    filePath += mf.Album + @"/";
                }
                if ( folder == "FileType" )
                {
                    filePath += mf.FileType.Substring( 1, mf.FileType.Length - 1 ).ToUpper( ) + @"/";
                }
                if ( folder == "Genre" )
                {
                    filePath += mf.Genre + @"/";
                }
            }

            return filePath;
        }

        List<MediaFile> mediaFilesToRelocate;
        public FolderStructure folderStructure;
    }

    // Intended to hold the data for the current directory structure 
    // selected within the application
    // ( As stands, the default is Artist -> Album -> FileType -> Genre )
    class FolderStructure
    {
        public FolderStructure( string [] folderCategories )
        {
            FolderCategories = folderCategories;
            NumberOfFolderCategories = folderCategories.Count( );
        }

        public int NumberOfFolderCategories { get; set; }
        public string [] FolderCategories { get; set; }
    }
}
