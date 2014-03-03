using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using TagLookup.Classes.MediaFiles.Processing;
using TagLookup.Classes.UI;

namespace TagLookup.Classes.MediaFiles
{

    public delegate ListBox ListBoxFunction( int type );
    public delegate int IntTypeFunction( ListBox lB );
    public delegate Predicate<object> PredicateTypeSetter( int type );

    public class MediaFileCollection
    {
        public MediaFileCollection()
        {
            ocMediaFiles = new ObservableCollection<MediaFile>( );
            toWriteMediaFiles = new List<MediaFile>( );
            fileLoader = new FileLoader( );
            exceptionList = new List<Tuple<string, string>>( );
            filteredMediaFiles = new ObservableCollection<MediaFile>( );
            partialFilePathToMediaFile = new Dictionary<string, MediaFile>( );
            fullFilePathToMediaFile = new Dictionary<string, MediaFile>( );
        }

        public ObservableCollection<MediaFile> CreateViewable( string folderPath )
        {
            if ( !Directory.Exists( folderPath ) ) return null;

            fileLoader.ReadFolder( folderPath );

            foreach ( string file in fileLoader.Files )
            {
                try
                {
                    var mf = new MediaFile( TagLib.File.Create( file ) );
                    fullFilePathToMediaFile.Add( file, mf );
                    int lastSlashOffset = file.LastIndexOf( "\\" ) + 1;
                    string partialFile = file.Substring( lastSlashOffset );
                    partialFilePathToMediaFile.Add( partialFile, mf );
                    ocMediaFiles.Add( mf );
                }
                catch ( Exception ex )
                {
                    exceptionList.Add( new Tuple<string, string>( ex.Message + System.Environment.NewLine + ex.StackTrace, file ) );
                }
            }

            return ocMediaFiles;
        }

        public int Save()
        {
            int mediaFileSavedCount = 0;

            foreach ( MediaFile mf in ocMediaFiles )
            {
                try
                {
                    if ( mf.Write == true )
                    {
                        TagLib.File file = TagLib.File.Create( mf.FileName );
                        file.Tag.Album = mf.Album;
                        file.Tag.Performers = new string [] { mf.Artist };
                        file.Tag.Comment = mf.Comments;
                        file.Tag.Genres = new string [] { mf.Genre };
                        file.Tag.Title = mf.Title;
                        file.Tag.Lyrics = mf.Lyrics;
                        file.Tag.Year = Convert.ToUInt16( mf.Year );
                        file.Tag.Track = Convert.ToUInt16( mf.Track );
                        file.Tag.TrackCount = Convert.ToUInt16( mf.TrackCount );
                        file.Tag.Disc = Convert.ToUInt16( mf.Disc );
                        file.Tag.DiscCount = Convert.ToUInt16( mf.DiscCount );
                        file.Save( );
                        mf.Write = false;
                        mediaFileSavedCount++;
                    }
                }
                catch ( Exception ex )
                {
                    exceptionList.Add( new Tuple<string, string>( ex.Message + System.Environment.NewLine + ex.StackTrace, mf.FileName ) );
                }
            }

            return mediaFileSavedCount;
        }

        public ICollectionView FilteredViewableCollection( ListBox listBox, int type, PredicateTypeSetter predicateTypeSetter )
        {
            if ( ocMediaFiles == null )
                return null;

            var itemSourceList = new CollectionViewSource( ) { Source = ocMediaFiles };
            ICollectionView itemsList = itemSourceList.View;
            List<string> filterValues = new List<string>( );

            foreach ( CheckBoxItem cbi in listBox.Items )
            {
                filterValues.Add( cbi.Name );
            }

            var complexFilter = predicateTypeSetter( type );

            return itemsList;
        }

        public ObservableCollection<MediaFile> ocMediaFiles;
        public ObservableCollection<MediaFile> filteredMediaFiles;
        List<MediaFile> toWriteMediaFiles;
        public Dictionary<string, MediaFile> partialFilePathToMediaFile;
        public Dictionary<string, MediaFile> fullFilePathToMediaFile;
        FileLoader fileLoader;
        FileMover fileMover;
        List<string> listOfFilterValues;
        List<Tuple<string, string>> exceptionList;
    }
}
