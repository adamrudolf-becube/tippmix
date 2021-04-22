using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace TippMix
{
    /// <summary>
    /// Thisclass contains the basic functions the program needs.
    /// </summary>
    static class Accessories
    {
        /// <summary>
        /// Acts as a global variable. Conrtols how detailed the output is. The higher the more tetailed the output is.
        /// </summary>
        /// <remarks>1 = only main status reports. Normal output for a user.\n2 = report about entering each function.\n3 = every step in every function is printed on the screen.</remarks>
        public static int DEBUG_LEVEL = 3;

        public static List<Tuple<long, int>> Indexes = new List<Tuple<long, int>>();

        //public const string defaultPath = @"";
        /// <summary>
        /// Working directory of the program.
        /// </summary>
        public const string defaultPath = @"d:\SkyDrive\Személyes\TippMix\";

        public static TextWriter logWriter = new StreamWriter(defaultPath + " log.txt");

        /// <summary>
        /// Function downloads the webpage, given in the address parameter, and converts it into string.
        /// </summary>
        /// <param name="address">A URI of a web page we would like t download.</param>
        /// <returns>Returns the source of the webpage in a string in UTF8 encoding.</returns>
        public static string downloadStringUTF8(string address)
        {
            // Create a new WebClient instance.
            WebClient myWebClient = new WebClient();
            byte[] myDataBuffer;

            // Download the Web resource and save it into a data buffer.
            try
            {
                myDataBuffer = myWebClient.DownloadData(address);
                Console.WriteLine(address + " SUCCESS");
                logWriter.WriteLine(address + " SUCCESS");
            }
            catch
            {
                Console.WriteLine(address + " FAIL");
                logWriter.WriteLine(address + " FAIL");
                return "FAIL";
            }
            // Initialize a UTF8 encoding class
            Encoding enc = new UTF8Encoding(true, true);


            // Encode the byte array to a string. 
            return enc.GetString(myDataBuffer);
        }

        /// <summary>
        /// This function seeks for a SoprtEvent with the specified ID in the given List of SportEvent objects.
        /// </summary>
        /// <remarks>The function seeks for the ID with interval halfing algorithm, so it is important to give the list to the function ordered by the ID-s.</remarks>
        /// <param name="spevList">List of SportEvent objects ordered by ID.</param>
        /// <param name="ID">The ID we are searcing for.</param>
        /// <returns> the index of the SportEvent object with the </returns>
        public static int getEventFromId(List<PastSportEvent> spevList, long ID)
        {
            // Declare and initialize boundary variables
            int to = spevList.Count;
            int from = 0;
            
            // Infinite loop, but we have return cases for sure
            while (true)
            {
                // Set actual index to the rounded mean of from and two
                int actual = (int)Math.Round((1.0 * (from + to)) / 2.0);
                // If from and to are adjecent, so they "reached each other", there is no such ID
                if (from == (to - 1) && spevList[from].ID != ID)
                    return 0;
                // If seeken ID is above the actual, we restrict the interval to the upper half
                if (spevList[actual].ID < ID)
                    from = actual;
                // If seeken ID is below the actual, we restrict the interval to the bottom half
                if (spevList[actual].ID > ID)
                    to = actual;
                // If seeken ID is equal to the actual, we return with the actual index
                if (spevList[actual].ID == ID)
                    return actual;
            }
        }
    }
}
