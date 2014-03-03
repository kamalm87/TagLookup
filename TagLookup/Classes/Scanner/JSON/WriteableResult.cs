using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific
using System.IO;
using System.Net;
using System.Windows.Media;
using TagLookup.Classes.MediaFiles;

namespace TagLookup.Classes.Scanner.JSON
{
    public class WriteableResult
    {

        public WriteableResult(MediaFile mf)
        {
            WriteableResultOptions = new List<WriteableResultOption>( );
            SetValues( mf.FileName, false );
        }
        
        public WriteableResult( JSON_Result json_result, string fileName, bool queryArtwork, int maxResults = 0 )
        {
            try
            {
                SetValues( fileName, queryArtwork );
                AddResults( json_result, maxResults );
                WriteableResultOptions.Sort( );

                string years = "";

                foreach(var item in WriteableResultOptions)
                {
                    years += item.Year + ", ";
                }

              //  System.Windows.MessageBox.Show( years );

                var waka = "HARD IN THE DATA PAINT";
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show( ex.Message );
            }
            
        }

        void AddResults( JSON_Result json_result, int maxResults)
        {
            // give an arbitrarily high assignment if there's no limit placed on the max number of results
            if(maxResults == 0)
            {
                maxResults = 50;
            }

            // Checks for results before iterating through results, and invoking AddResult for each result 
            // in the JSON result up to assigned maxResults
            if (json_result.results != null)
            {
                int currentResult = 0;
                foreach( Result result in json_result.results)
                {
                    if(currentResult < maxResults)
                    {
                        AddResult( result );
                        currentResult++;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        void AddResult(Result result)
        {
            if ( result.releasegroups != null )
            {
                foreach(Releasegroup releasegroup in result.releasegroups)
                {
                    if(releasegroup.releases != null)
                    {
                        foreach(Release release in releasegroup.releases)
                        {
                            WriteableResultOptions.Add( new WriteableResultOption( release, releasegroup.artists, releasegroup.title, FileName, QueryArtwork));
                        }
                    }
                }
            }
        }

        void SetValues(string fileName, bool queryArtwork)
        {
            FileName = fileName;
            QueryArtwork = queryArtwork;
            WriteableResultOptions = new List<WriteableResultOption>( );
        }

        public List<WriteableResultOption> WriteableResultOptions { get; set; }
        bool QueryArtwork { get; set; }
        int ResultNumber { get; set; }
        string FileName { get; set; }
    }


    public class WriteableResultOption : IComparable
    {
        public WriteableResultOption(string fileName)
        {
            Year = 0;
            Album = "";
            TrackNumber = 0;
            TrackCount = 0;
            DiscNumber = 0;
            DiscCount = 0;
            FileName = fileName;
            Artists = new List<string>(  );
            Artists.Add( "" );
            Genre = "";
            Title = "";
            IsANullResult = true;
            
        }

        public WriteableResultOption( Release release, List<Artist> artists, string albumTitle, string fileName, bool queryArtwork )
        {
            IsANullResult = false;
            // hardcoded to only look at US (United States) releases
             Year = getCountryRelease( release.releaseevents, "US" );

            Album = albumTitle;
            TrackCount = release.track_count;
            DiscCount = release.medium_count;
            FileName = fileName;


           
            if ( release.mediums.Count > 0 )
            {
                var media = release.mediums [ 0 ];
                Format = media.format;
                DiscNumber = media.position;
                var track = media.tracks [ 0 ];
                TrackNumber = track.position;
                Title = track.title;
                if ( track.artists != null )
                    Artists = ProcessTrackArtists( track.artists );
                else if ( artists != null )
                    Artists = ProcessAlbumArtists( artists );
                else if ( Artists == null )
                {
                    Artists = new List<string>( );
                    Artists.Add( "None" );
                }
            }
         

            ReleaseID = release.id;

            if( queryArtwork == true)
            {
                try
                {
                    ImageSourceURL = QueryAlbumArtURL( release.id );
                }
                catch(Exception ex)
                {
                    Exceptions.Add( new Tuple<string,string,string>( fileName, "QueryAlbumArtURL -> release.id" ,ex.Message + System.Environment.NewLine + ex.StackTrace ));
                }

                
            }
                
           
        }



        int IComparable.CompareTo( object obj )
        {
            WriteableResultOption that = (WriteableResultOption)obj;


            if ( this.Year == 0 && that.Year == 0)
            {
                return 0;
            }

            if ( this.Year != 0 && that.Year == 0 )
            {
                return -1;
            }

            if ( this.Year == 0 && that.Year != 0 )
            {
                return 1;
            }

            if ( this.Year != 0 && that.Year != 0 )
            {
                if ( this.Year < that.Year )
                    return -1;
                if ( this.Year > that.Year )
                    return 1;
                if ( this.Year == that.Year )
                    return 0;

                else
                {
                    System.Windows.MessageBox.Show( "What the fuck" );
                    return 0;
                }
                    
                    
            }



            else
            {
                var a = this;
                var b = that;
                System.Windows.MessageBox.Show( "What the fuck" );
                return 0;
            }
            
        }


        public static bool operator <( WriteableResultOption left, WriteableResultOption right )
        {

            if ( left.Year < right.Year )
                return true;
            else
                return false;
        }

        public static bool operator>(WriteableResultOption left, WriteableResultOption right)
        {
           if ( right.Year > left.Year )
                return true;
            else
                return false;
        }


        public bool IsANullResult { get; set; }


        string QueryAlbumArtURL( string musicBrainzReleaseID )
        {
            string queryText = "http://coverartarchive.org/release/" + musicBrainzReleaseID + "/";
            HttpWebResponse response;

            var request = WebRequest.Create( queryText );
            
            try
            {
                response = (HttpWebResponse)request.GetResponse( );
            }
            catch(WebException ex)
            {
                response = ex.Response as HttpWebResponse;
            }

            if ( response.StatusCode == HttpStatusCode.OK)
            {
                string jsonText;
                using (var sr = new StreamReader( response.GetResponseStream()))
                {
                    jsonText = sr.ReadToEnd( );
                    if (jsonText.Contains("\"image\":"))
                    {
                        string url = ExtractImageURL( jsonText );
                        return url;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else if ( response.StatusCode == HttpStatusCode.NotFound)
            {
                if ( Exceptions == null ) Exceptions = new List<Tuple<string, string,string>>();
                Exceptions.Add( new Tuple<string, string, string>( FileName, "http://www.coverartarchive.org/release" + ReleaseID, response.StatusCode.ToString() ) );
                return null;
            }
            else
            {
                return null;
            }
        }

        string ExtractImageURL(string jsonText)
        {
            string imageTag = "\"image\":";
            int imageIndex = jsonText.IndexOf( imageTag );
            int urlEndIndex = jsonText.IndexOf( "\"", imageIndex + imageTag.Length + 3 );

            if( imageIndex != -1 && urlEndIndex != -1 )
            {
                string url = jsonText.Substring( imageIndex + imageTag.Length + 1, urlEndIndex - ( imageIndex + imageTag.Length + 1 ) );
                return url;
            }
            else
            {
                return null;
            }
        }

        public string ReleaseID { get; set; }
        public string ImageSourceURL { get; set; }

        public List<string> Artists { get; set; }

        public int? DiscNumber { get; set; }
        public int? DiscCount { get; set; }

        public int? TrackNumber { get; set; }
        public int? TrackCount { get; set; }

        public int? Year { get; set; }

        public string Genre { get; set; }

        public string Album { get; set; }
        public string FileName { get; set; }
        public string Format { get; set; }
        public string Title { get; set; }

        public List<Tuple<string, string, string>> Exceptions { get; set; }


        int getCountryRelease( List<Releaseevent> releaseEvents, string countryCode )
        {
           
            if(releaseEvents != null)
            {
                foreach ( Releaseevent re in releaseEvents )
                {
                    if ( re.country == countryCode )
                    {
                        if ( re.date != null )
                            return re.date.year;
                        else
                            return 0;
                    }
                }
                return 0;
            }
            else
            {
                return 0;
            }
            
        }

        List<string> ProcessTrackArtists( List<Artist2> artists )
        {
            List<string> artistList = new List<string>( );

            foreach ( Artist2 artist in artists )
            {
                artistList.Add( artist.name );
            }
            if ( artistList.Count > 0 )
                return artistList;
            else
                return null;
        }

        List<string> ProcessAlbumArtists( List<Artist> artists )
        {
            List<string> artistList = new List<string>( );

            foreach ( Artist artist in artists )
            {
                artistList.Add( artist.name );
            }
            if ( artistList.Count > 0 )
                return artistList;
            else
                return null;
        }
    }
}
