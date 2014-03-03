using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific
using System.IO;
using System.ComponentModel;

namespace TagLookup.Classes.MediaFiles
{
    public class MediaFile : INotifyPropertyChanged
    {
        // Reads a Mediafile using the TagLibSharp File type, extracting desired 
        // tags into a more lightweight object with fewer details
        // ( A collection of ~2,000 TagLib.File objects consumes over 2GB of memory,
        // overfilling the list container )

        // Assigns the class's properties with the corresponding
        // TagLib.File property
        // TODO (possible): add/modify properties/the process for 
        // current projects
        public MediaFile( TagLib.File mediaFile )
        {


            if(mediaFile.Tag.Composers.Length != 0)
            {
                Producers = new List<string>();
                foreach ( string composer in mediaFile.Tag.Composers )
                    Producers.Add( composer );
            }
            else
            {
                Producers = new List<string>( );
            }

            FileName = mediaFile.Name;
            Album = mediaFile.Tag.Album;
            Comments = mediaFile.Tag.Comment;
            Lyrics = mediaFile.Tag.Lyrics;
            Title = mediaFile.Tag.Title;
            BitRate = mediaFile.Properties.AudioBitrate;
            Duration = mediaFile.Properties.Duration;
            DiscCount = Convert.ToInt16( mediaFile.Tag.DiscCount );
            Track = Convert.ToInt16( mediaFile.Tag.Track );
            TrackCount = Convert.ToInt16( mediaFile.Tag.TrackCount );
            Year = Convert.ToInt16( mediaFile.Tag.Year );
            FileInfo file = new FileInfo( FileName );
            ContainingFolder = file.DirectoryName;
            PartialFileName = file.Name;

            Disc = Convert.ToInt16(mediaFile.Tag.Disc);

            FileSize = file.Length / (long)( 1024.0 * 1000.0 );
            FileType = file.Extension;

            if ( mediaFile.Tag.Performers.Length > 0 )
                Artist = mediaFile.Tag.Performers [ 0 ];
            else
                Artist = "";

            if(mediaFile.Tag.Genres.Length == 0)
            {
                Genre = "";
            }
            else
            {
                Genre = mediaFile.Tag.Genres [ 0 ];
            }

            var picture = mediaFile.Tag.Pictures;

            if(mediaFile.Tag.Pictures.Length == 0)
            {
                HasArtwork = false;
            }
            else
            {
                HasArtwork = true;
            }

            SaveCanvasImageSource = false;
            Write = false;
        }

        // when the 
        public bool SaveCanvasImageSource { get; set; }
  

        public List<byte[]> GetArtwork()
        {
            List<byte []> pictures = new List<byte []>( );


            if ( File.Exists( FileName ) )
            {
                var mediaFileStream = TagLib.File.Create( FileName );
                foreach ( var picture in mediaFileStream.Tag.Pictures )
                {
                    byte [] imageData = picture.Data.Data;
                    pictures.Add( imageData );
                }
                return pictures;
            }
            else
            {
                return null;
            }
        }

        public void SetArtwork( byte[] artworkData )
        {
            if(File.Exists(FileName))
            {
                try
                {
                    var mediaFileStream = TagLib.File.Create( FileName );

                    if ( mediaFileStream.Tag.Pictures != null )
                    {

                        TagLib.Picture pic = new TagLib.Picture( );
                        pic.Data = artworkData;
                        // mediaFileStream.Tag.Pictures [ 0 ] = pic;

                        TagLib.IPicture newArt = new TagLib.Picture( );
                        newArt.Data = artworkData;
                        mediaFileStream.Tag.Pictures = new TagLib.IPicture [ 1 ] { newArt };
                    } 
                    else
                    {
                        mediaFileStream.Tag.Pictures = new TagLib.IPicture[1];
                        mediaFileStream.Tag.Pictures [ 0 ].Data = artworkData;
                    }
                    mediaFileStream.Save( );
                }
                catch(Exception ex)
                {
                    System.Windows.MessageBox.Show( ex.Message + System.Environment.NewLine + ex.StackTrace );
                }
                
            }
        }
        

        public void SaveMediaFile( MediaFile mf, bool saveArtwork = false, byte[] artworkData = null )
        {
            if(!File.Exists(FileName)) 
            {
                System.Windows.MessageBox.Show( FileName + " does not exist" );
                return;
            }

            TagLib.File file = TagLib.File.Create( mf.FileName );
            file.Tag.Album = mf.Album;
            
            file.Tag.Performers = new string [] { mf.Artist };
            file.Tag.Comment = mf.Comments;

            file.Tag.Genres = new string [] { mf.Genre };
            file.Tag.Title = mf.Title;
            file.Tag.Lyrics = mf.Lyrics;
            
            file.Tag.Year       = Convert.ToUInt16( mf.Year );
            file.Tag.Track      = Convert.ToUInt16( mf.Track );
            file.Tag.TrackCount = Convert.ToUInt16( mf.TrackCount );
            file.Tag.Disc       = Convert.ToUInt16( mf.Disc );
            file.Tag.DiscCount  = Convert.ToUInt16( mf.DiscCount );

            if ( saveArtwork == true )
                SetArtwork( artworkData );
            else
                file.Save( );
        }

        


        // Value based properties
        // The non-auto Properties are intended to be tag values
        // that should be capable of being edited by the program
        // and eventually saved, if the save function is invoked.
        public string Album
        {
            get
            {
                return album;
            }
            set
            {
                if ( value != this.Album )
                {
                    this.album = value;
                    NotifyPropertyChanged( "Album" );
                }
            }
        }
        public string Artist
        {
            get
            {
                return artist;
            }
            set
            {
                if ( value != this.Artist )
                {
                    this.artist = value;
                    NotifyPropertyChanged( "Artist" );
                }
            }
        }
        public string Comments
        {
            get
            {
                return comments;
            }
            set
            {
                if ( value != this.Comments )
                {
                    this.comments = value;
                    NotifyPropertyChanged( "Comments" );
                }
            }
        }
        public int? Disc
        {
            get
            {
                return disc;
            }
            set
            {
                if ( value != this.Disc )
                {
                    this.disc = value;
                    NotifyPropertyChanged( "Disc" );
                }
            }
        }
        public int? DiscCount
        {
            get
            {
                return discCount;
            }
            set
            {
                if ( value != this.DiscCount )
                {
                    this.discCount = value;
                    NotifyPropertyChanged( "DiscCount" );
                }
            }
        }
        public string Genre
        {
            get
            {
                return genre;
            }
            set
            {
                if ( value != this.Genre )
                {
                    this.genre = value;
                    NotifyPropertyChanged( "Genre" );
                }
            }
        }
        public string Lyrics
        {
            get
            {
                return lyrics;
            }
            set
            {
                if ( value != this.Lyrics )
                {
                    this.lyrics = value;
                    NotifyPropertyChanged( "Lyrics" );
                }
            }
        }
        public bool HasArtwork { get; set; }
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if ( value != this.Title )
                {
                    this.title = value;
                    NotifyPropertyChanged( "Title" );
                }
            }
        }
        public int? Track
        {
            get
            {
                return track;
            }
            set
            {
                if ( value != this.Track )
                {
                    this.track = value;
                    NotifyPropertyChanged( "Track" );
                }
            }
        }
        public int? TrackCount
        {
            get
            {
                return trackCount;
            }
            set
            {
                if ( value != this.TrackCount )
                {
                    this.trackCount = value;
                    NotifyPropertyChanged( "TrackCount" );
                }
            }
        }
        public int? Year
        {
            get
            {
                return year;
            }
            set
            {
                if ( value != this.Year )
                {
                    this.year = value;
                    NotifyPropertyChanged( "Year" );
                }
            }
        }
        public int? BitRate { get; set; }
        public TimeSpan Duration
        {
            get
            {
                return duration;
            }
            set
            {
                if ( value != this.duration )
                {
                    this.duration = value;
                    NotifyPropertyChanged( "Duration" );
                }
            }
        }
        public long FileSize
        {
            get;
            set;
        }
        public string FileName { get; set; }
        public string FileType { get; set; }

        // Non-updating properties ( UNDER CONSTRUCTION )
        public List<string> Producers { get; set; }
        public string ContainingFolder  { get; set;}
        public string PartialFileName { get; set; }
        // Flag property: 
        // All mediafiles in the DataGridView with this value marked as true
        // will be passed as a parameter to the save function
        public bool Write
        {
            get
            {
                return write;
            }
            set
            {
                if ( value != this.write )
                {
                    this.write = value;
                }
            }
        }

        public MediaFile()
        { }

        // Flag setter:
        // Will be activated for a given MediaFile in the DataGridView collection if 
        // a given column in the MediaFile's item is edited
        private void UpdateProperties()
        {
            Write = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( String info )
        {
            if ( PropertyChanged != null )
            {
                if ( Write == false )
                    Write = true;
                PropertyChanged( this, new PropertyChangedEventArgs( info ) );
            }
        }

        // private variables for properties
        string album;
        string artist;
        string comments;
        string genre;
        string lyrics;
        string title;
        int? disc;
        int? discCount;
        int? track;
        int? trackCount;
        int? year;
        bool write;


        TimeSpan duration;
    }
}
