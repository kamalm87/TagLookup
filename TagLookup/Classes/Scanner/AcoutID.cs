using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific
using System.Net;
using System.IO;

namespace TagLookup.Classes.Scanner
{

    // required: no
    // description: response format
    enum format
    {
        json = 1,
        jsonp,
        xml
    }

    // required: no
    // description:JSONP callback, only applicable if you select the jsonp format
    enum jsoncallback
    {
        jsonAcoustidApi
    }


    // required: no
    // description: returned metadata
    enum meta
    {
        recordings,
        recordingids,
        releases,
        releaseids,
        releasegroups,
        releasegroupids,
        tracks,
        compress,
        usermeta,
        sources
    }


    class AcoustID
    {
        static string baseAcoustID_API_URL = @"http://api.acoustid.org/v2/lookup";

        public FPCalc fingerPrintScanner;

        public AcoustID( string clientID, string FPCalcFilePath )
        {
            ClientID = clientID;
            MetaEnumChoices = SetDefaultMetaDataTags( );

            fingerPrintScanner = new FPCalc( FPCalcFilePath );

            DEFAULT_METADATA_QUERY = createQueryURL_Base( MetaEnumChoices, 1 );
        }

        public string QueryAPI( string tagBaseQuery, Tuple<string, string, string> fingerPrintInformation )
        {
            if ( tagBaseQuery == "" || fingerPrintInformation == null ) return null;

            string queryURL = tagBaseQuery + "&duration=" + fingerPrintInformation.Item2
                            + "&fingerprint=" + fingerPrintInformation.Item3;

            var request = WebRequest.Create( queryURL );
            
            // NOTE THIS -- may need a timer somehwere because Acoustid API limit: 3 requests per second 
            // Thread.Sleep( 334);
            var response = (HttpWebResponse)request.GetResponse( );
            
            if ( response.StatusCode == HttpStatusCode.OK )
            {
                string jsonText;

                using ( var sr = new StreamReader( response.GetResponseStream( ) ) )
                {
                    jsonText = sr.ReadToEnd( );
                }

                return jsonText;
            }
            else
            {
                return null;
            }

        }

        string createQueryURL_Base( bool [] enumValuesToInclude, int formatType )
        {
            string baseQueryURL = baseAcoustID_API_URL + "?client=" + ClientID +
                /* format=[format enum choice] */   chooseQueryReturnFormat( formatType ) +
                /* meta=[meta enum tag choices]*/   createMetaTagsToQuery( enumValuesToInclude );

            return baseQueryURL;
        }

        string chooseQueryReturnFormat( int enumChoice )
        {
            string formatString = "&format=";

            if ( 0 < enumChoice && enumChoice < 4 )
            {
                formatString = formatString + ( (format)enumChoice ).ToString( );
                return formatString;
            }

            else return "";
        }

        string createMetaTagsToQuery( bool [] enumValuesToInclude )
        {
            string metaTag = "&meta=";

            for ( int i = 0; i < enumValuesToInclude.Length; i++ )
            {
                if ( enumValuesToInclude [ i ] == true )
                    metaTag += ( (meta)i ).ToString( ) + "+";
            }

            if ( metaTag [ metaTag.Length - 1 ] == '+' )
                metaTag = metaTag.Substring( 0, metaTag.Length - 2 );

            return metaTag;
        }


        bool [] SetDefaultMetaDataTags()
        {
            bool [] allTags = new bool [ 10 ];

            for ( int i = 0; i < allTags.Length; i++ )
                allTags [ i ] = true;


            /* meta=releases+releasegroups+tracks+compress+usermeta+source */
            /* enums
                        recordings,         [0]
                        recordingids,       [1]
                        releases,           [2]
                        releaseids,         [3]
                        releasegroups,      [4]    
                        releasegroupids,    [5]
                        tracks,             [6]
                        compress,           [7]
                        usermeta,           [8]
                        sources             [9]
             */

            // do not use these tags--cause incomplete metadata results
            allTags [ 0 ] = false;
            allTags [ 1 ] = false;
            allTags [ 3 ] = false;
            allTags [ 5 ] = false;
            return allTags;
        }

        // required for the API -- using the default one
        public string ClientID { get; set; }

        // required for all individual queries
        public string FileName { get; set; }
        public string Duration { get; set; }
        public string FingerPrint { get; set; }
        
        public string DEFAULT_METADATA_QUERY { get; set; }

        public Tuple<string, string, string> IndividualFingerPrintItem { get; set; }
        
        public bool [] MetaEnumChoices { get; set; }
    }
}
