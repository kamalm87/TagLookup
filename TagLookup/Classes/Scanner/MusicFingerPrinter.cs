using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific
using Newtonsoft.Json;
using TagLookup.Classes.Scanner;
using TagLookup.Classes.Scanner.JSON;
using Microsoft.WindowsAPICodePack;
using System.IO;

namespace TagLookup.Classes.Scanner
{
    public class MusicFingerPrinter
    {

        string ClientID;
        string FPCalcPath;

        AcoustID AcoustID;

        public MusicFingerPrinter()
        {
            ClientID = "8XaBELgH";

            var dir = System.Reflection.Assembly.GetExecutingAssembly( ).GetName( ).CodeBase;

            FPCalcPath = getExpectedFPCalcPath( )  + "fpcalc.exe";
            
            //  bool isThisPathValid = File.Exists( FPCalcPath );
            // hardcode the path of FPCalc if desired
            //  FPCalcPath = @"C:\Users\KNM\Documents\GitHub\MusicManager\MusicManager\Utilities\fpcalc.exe";
            
            AcoustID = new AcoustID( ClientID, FPCalcPath );
            ExceptionList = new List<Tuple<string, string>>( );
        }

        string getExpectedFPCalcPath()
        {
            string programPath = System.Reflection.Assembly.GetExecutingAssembly( ).GetName( ).CodeBase.ToString();
            int indexOfExecutableName = programPath.LastIndexOf("/");
            var workingDir = programPath.Substring( 8, indexOfExecutableName - 8);
            workingDir += "/Utilities/";
            return workingDir;
        }


        public MusicFingerPrinter( string clientID, string fpCalcPath )
        {
            ClientID = clientID;
            FPCalcPath = fpCalcPath;
            AcoustID = new AcoustID( ClientID, FPCalcPath );
            ExceptionList = new List<Tuple<string, string>>( );
        }


        public Tuple<string, string, WriteableResult, bool> ScanSingleMediaFile( string fileName, bool queryArtwork = true)
        {
            try
            {
                var getFileAudioFingerPrint = AcoustID.fingerPrintScanner.FingerPrint( fileName );
                var queryMetaDataFromFingerPrint = AcoustID.QueryAPI( AcoustID.DEFAULT_METADATA_QUERY, getFileAudioFingerPrint );

                JSON_Result jsonSerializedObject = JsonConvert.DeserializeObject<JSON_Result>( queryMetaDataFromFingerPrint );
                WriteableResult writeableResult = new WriteableResult( jsonSerializedObject, fileName, queryArtwork, 4 );

                int indexOfFinalDirectory = fileName.LastIndexOf( "\\" ) + 1;
                string partialFileName = fileName.Substring( indexOfFinalDirectory );
                Tuple<string, string, WriteableResult, bool> ScanItem = new Tuple<string, string, WriteableResult, bool>( fileName, partialFileName, writeableResult, false );
                return ScanItem;
            }
            catch ( Exception ex )
            {
                ExceptionList.Add( new Tuple<string, string>( fileName, ex.Message + System.Environment.NewLine + ex.StackTrace ) );
                return null;
            }
        }

        public List<Tuple<string, string>> ExceptionList { get; set; }
        public List<Tuple<string, string, WriteableResult, bool>> ListOfScannedItems { get; set; }
    }
}
