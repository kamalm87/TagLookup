using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific namespaces
using System.IO;
using System.Net;

using TagLookup.Classes.MediaFiles;
using TagLookup.Classes.Scanner;
using TagLookup.Classes.Scanner.JSON;


namespace TagLookup.Classes.Scanner
{
    class ScannedFiles
    {
        MusicFingerPrinter MusicFingerPrinter;

        public ScannedFiles()
        {
            MusicFingerPrinter = new MusicFingerPrinter( "8XaBELgH", @"C:\Users\KNM\Documents\GitHub\MusicManager\MusicManager\Utilities\fpcalc.exe" );
            WriteableResults = new List<WriteableResult>( );
            MediaFileWritableResults = new Dictionary<string, WriteableResult>( );
            ReleaseIDToImageData = new Dictionary<string, byte []>( );
            MusicFingerPrinter = new MusicFingerPrinter( );
            ExceptionList = new List<Tuple<string, string>>( );
            GetWriteableResultMediaFile = new Dictionary<WriteableResult, MediaFile>( );
        }



        /*** public void Add( MediaFile mf, bool downloadArtwork = true) ***/
        // Scans the MediaFile and attempts to query the AcoustID API given a MediaFile object
        // * If successful, it will map the file name to the results in the MediaFileWritableResults dictionary<string, WriteableResults>
        // * If the query is unsuccessful a blank WriteableResult will be mapped
        // * Will also attempt to overwrite an existing key only if the ScanFile is successful
        
        // By default, this function will also attempt to download artwork from using http://coverartarchive.org/release/ + [MusicBrain ReleaseID]
        // This option will slow scanning down, significantly increase the memory footprint, and consume a significant amount of bandwidth
        // as it will cause this function to make the query, parse the JSON result and attempt to download and render the artwork. 
        public void Add( MediaFile mf, bool downloadArtwork = true)
        {
            try 
            {
                if(!MediaFileWritableResults.ContainsKey(mf.FileName))
                {
                    var scanMediaFile = MusicFingerPrinter.ScanSingleMediaFile( mf.FileName );
                    
                    if ( !MediaFileWritableResults.ContainsKey(mf.FileName) )
                    {
                        if( scanMediaFile != null)
                        {
                            MediaFileWritableResults.Add( mf.FileName, scanMediaFile.Item3 );
                            GetWriteableResultMediaFile.Add( scanMediaFile.Item3, mf );
                        }
                        else
                        {
                            MediaFileWritableResults.Add( mf.FileName, new WriteableResult(mf) );
                        }
                            
                    }
                    else if ( MediaFileWritableResults.ContainsKey(mf.FileName))
                    {
                        if ( scanMediaFile != null )
                        {
                            MediaFileWritableResults [ mf.FileName ] = scanMediaFile.Item3;
                            GetWriteableResultMediaFile.Add( scanMediaFile.Item3, mf );
                        }
                            
                    }
                           
                    if ( downloadArtwork == true )
                    {
                        SetArtwork( mf );
                    }
                }
                else
                {
                    return;
                }
            }
            catch(Exception ex)
            {
                ExceptionList.Add( new Tuple<string, string>( "Add -> " + mf.FileName + ", downloadArtwork -> " + downloadArtwork.ToString() , ex.Message + System.Environment.NewLine + ex.StackTrace + System.Environment.NewLine + ex.StackTrace ));
            }
        }
         
        /*** public void SetArtwork(MediaFile mf) ***/
        // *
        // *
        // *
        public void SetArtwork(MediaFile mf)
        {
            if( MediaFileWritableResults.ContainsKey(mf.FileName))
            {
                var wrOptions = MediaFileWritableResults [ mf.FileName ].WriteableResultOptions;
                
                foreach(var wr in wrOptions)
                {
                    if(ReleaseIDToImageData.ContainsKey(wr.ReleaseID))
                    { 
                        // Do nothing, already mapped
                    }
                    else if ( wr.ImageSourceURL != null)
                    {
                        try
                        {
                            var request = WebRequest.Create( wr.ImageSourceURL );
                            var response = (HttpWebResponse)request.GetResponse( ); 

                            if( response.StatusCode == HttpStatusCode.OK && response.ResponseUri != null)
                            {
                                WebClient client = new WebClient( );

                                byte [] data = client.DownloadData( response.ResponseUri );
                                ReleaseIDToImageData.Add( wr.ReleaseID, data );
                            }
                            else
                            {
                                ExceptionList.Add( new Tuple<string, string>( wr.ImageSourceURL, response.StatusCode.ToString() ) );
                            }
                        }
                        catch (Exception ex)
                        {
                            ExceptionList.Add( new Tuple<string, string>( mf.FileName, ex.Message + System.Environment.NewLine + ex.StackTrace ) );
                        }
                    }
                }
            }
        }

        public byte [] GetArtwork( string releaseID )
        {
            if(ReleaseIDToImageData.ContainsKey(releaseID))
            {
                return ReleaseIDToImageData [ releaseID ];
            }
            else
            {
                return null;
            }
        }

        public Dictionary<WriteableResult, MediaFile> GetWriteableResultMediaFile { get; set; }
        public Dictionary<string, WriteableResult> MediaFileWritableResults { get; set; }

        public List<WriteableResult> WriteableResults { get; set; }

        Dictionary<string, string> FileNameToReleaseIDMapping { get; set; }
        Dictionary<string, string> ReleaseIDToImageName { get; set; }
        public Dictionary<string, byte[]> ReleaseIDToImageData { get; set; }
        
        Dictionary<string, string> placeholder1 { get; set; }
        Dictionary<string, string> placeholder2 { get; set; }

        List<Tuple<string, string>> ExceptionList;
}
}
