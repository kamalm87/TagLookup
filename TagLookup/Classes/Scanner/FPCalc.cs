using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific namespace
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Threading;

namespace TagLookup.Classes.Scanner
{
    class FPCalc
    {
        public FPCalc( string executablePath )
        {
            ExecutablePath = executablePath;
        }

        // Input: Runs the FPCalc fingerprint scanner application on the inputed filename 
        // Output: If there is no output from the application, return null, otherwise return below three items:
        //         * Item 1: inputed filename
        //         * Item 2: track duration for the AcoustID API
        //         * Item 3: track's fingerprint for the AcoustID API
        public Tuple<string, string, string> FingerPrint( string fileName )
        {
            ProcessStartInfo fpCalcProcessSettings = new ProcessStartInfo( );
            fpCalcProcessSettings.CreateNoWindow = true;
            fpCalcProcessSettings.FileName = ExecutablePath;
            fpCalcProcessSettings.Arguments = addQuotationMarksIfMissing( fileName );
            fpCalcProcessSettings.UseShellExecute = false;
            fpCalcProcessSettings.RedirectStandardOutput = true;

     
            Process fpCalcProcess = new Process( );
            fpCalcProcess.StartInfo = fpCalcProcessSettings;
            fpCalcProcess.Start( );

            if ( fpCalcProcess.StandardOutput != null )
            {
                System.IO.StreamReader consoleStream = fpCalcProcess.StandardOutput;
                string completeRawOutput = consoleStream.ReadToEnd( );
                return processRawFpCalcTextToThreeStringTuple( completeRawOutput );
            }
            else
            {
                return null;
            }

        }
        
        // adds quotation marks for the filename if there are none; does nothing otherwise
        string addQuotationMarksIfMissing( string inputString )
        {
            if ( inputString [ 0 ] != '\"' )
                inputString = "\"" + inputString;
            if ( inputString [ inputString.Length - 1 ] != '\"' )
                inputString = inputString + "\"";
            return inputString;
        }

        // Parses the output a successful FPCalc scan, returning a tuple containing
        // FILE - for file reference if necessary
        // DURATION - for the AcoustID API Query
        // FINGERPINT - for the AcoustID API Query
        Tuple<string, string, string> processRawFpCalcTextToThreeStringTuple( string rawInput )
        {
            string fileKeyword = "FILE=";
            string durationKeyword = "DURATION=";
            string fingeprintKeyword = "FINGERPRINT=";

            int indexOfFile = rawInput.IndexOf( fileKeyword );
            int indexOfDuration = rawInput.IndexOf( durationKeyword );
            int indexOfFingerPrint = rawInput.IndexOf( fingeprintKeyword );

            string inputFile = rawInput.Substring( indexOfFile + fileKeyword.Length, indexOfDuration - 7 );
            string inputDuration = rawInput.Substring( indexOfDuration + durationKeyword.Length, indexOfFingerPrint - indexOfDuration - durationKeyword.Length - 2 );
            string inputFingerPrint = rawInput.Substring( indexOfFingerPrint + fingeprintKeyword.Length );

            return new Tuple<string, string, string>( inputFile, inputDuration, inputFingerPrint );
        }

        string ExecutablePath { get; set; }
    }
}

