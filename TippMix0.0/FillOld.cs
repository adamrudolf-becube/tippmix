using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Globalization;

namespace TippMix
{
    public class round : IComparable
    {   
        public int year;
        public int week;
        public int round_number;

        public int CompareTo(object obj)
        {
            round another = obj as round;
            int ownNumber = getNumber();
            int anoNumber = another.getNumber();
            if (ownNumber == anoNumber)
                return 0;
            else
                return ownNumber > anoNumber ? 1 : -1;
        }

        public int getNumber()
        {
            return 1000 * year + 10 * week + round_number;
        }

        public round()
        {

        }

        public round(int _year, int _week, int _round_number)
        {
            year = _year;
            week = _week;
            round_number = _round_number;
        }

        public override string ToString()
        {
            return year.ToString() + ". " + week.ToString() + ". " + round_number.ToString() + ".";
        }

    }

    /// <summary>
    /// This object contains functions to get the list of SportEvent objects from the web, or from a file in the correct format. It also contains helper methods as subrotines for these purposes, for example conversions.
    /// </summary>
    class SportEventDictionary
    {
        /// <summary>
        /// Contains a set of SportEvent objects in a dictionary.
        /// </summary>
        public SortedDictionary<long, PastSportEvent> sportsEventList = new SortedDictionary<long, PastSportEvent>();
        //public SortedDictionary<long, Offer> offerList = new SortedDictionary<long, Offer>();
        /// <summary>
        /// The object can save the list to a file, or read the file and fill the dictionary from it. This is the name of the file.
        /// </summary>
        private string storageFileName = "SportEvents.txt";

        /// <summary>
        /// Function expects a List of SportEvent objects, and writes their properties to a textfile.
        /// </summary>
        /// <param name="sportsEventList"></param>
/**/    public void writeSPListToFile()
        {
            using(TextWriter tw = new StreamWriter(Accessories.defaultPath + storageFileName))
                foreach (KeyValuePair<long, PastSportEvent> sp in sportsEventList)
                    tw.WriteLine(sp.Value.ToTabSeparatedString());
        }

/**/    public void writeSPListToFile(SortedDictionary<long, PastSportEvent> sportsEventList)
        {
            using (StreamWriter sw = File.AppendText(Accessories.defaultPath + storageFileName))
                foreach (KeyValuePair<long, PastSportEvent> sp in sportsEventList)
                    sw.WriteLine(sp.Value.ToTabSeparatedString());
        }

/**/    public string getStorageFileName()
        {
            return storageFileName;
        }

/**/    public void setStorageFileName(string newFileName)
        {
            storageFileName = newFileName;
        }

/**/    public void uploadFile()
        {
            missingFromWeb();
            writeSPListToFile();
        }

/**/    public void missingFromWeb()
        {
            fillFromFile();

            // Determining the list of read elements
            List<round> missingRounds = determineMissingList();

            // Getting the results of the events of listed rounds
            fillResultsFromWeb(missingRounds);

            // Getting the offers for the existing SportEvents
            getOfferList(missingRounds);

            // Writing list to file
            writeSPListToFile();
        }

        public List<round> determineMissingList()
        {
            // Determining the list of read elements
            List<round> allRounds = ReadWhatToRead();
            List<round> missingRounds = new List<round>();
            int latest = getLatestRound();

            foreach (round rnd in allRounds)
                if (rnd.getNumber() >= latest)
                    missingRounds.Add(rnd);
                else
                    break;

            return missingRounds;
        }

        private int getLatestRound()
        {
            PastSportEvent last = sportsEventList.Last().Value;
            return last.getRoundNumber();
        }

/**/    public void fillFromFile()
        {
            sportsEventList.Clear();
            addListFromFile();
        }

/**/    public void fillFromWeb()
        {
            sportsEventList.Clear();
            addListFromWeb();
        }

/**/    public void getOfferList(List<round> allRounds)
        {
            int size = allRounds.Count;

            // Iterating through the rounds
            for (int i = 1; i < size; i++)
                // Each old offer has 5 pages. We have to iterate through them as well                
                for (int page = 1; page <= 5; page++)
                {
                    // Downloading the webpage content
                    string URIoffer = System.String.Format("http://kozpontban.hu/blog/tippmix-{0}-{1}-het-{2}-fordulo-teljes-ajanlat-{3}-resz/", allRounds[i].year, allRounds[i].week, allRounds[i].round_number, page);
                    string webPageContent = Accessories.downloadStringUTF8(URIoffer);

                    if (webPageContent != "FAIL")
                        // Matching pattern and writing results to the event
                        fillOffersFromWebContent(allRounds[i], webPageContent);
                }
        }

        public void getCurrentOffers()
        {
            string URIoffer = System.String.Format("http://www.szerencsejatek.hu/tippmix-teljes-ajanlat");
            string webPageContent = Accessories.downloadStringUTF8(URIoffer);

            if (webPageContent != "FAIL")
                // Matching pattern and writing results to the event
                fillCurrentOffersFromWebContent(webPageContent);
        }

        private void addListFromFile()
        {
            // Getting the file
            string[] lines = System.IO.File.ReadAllLines(Accessories.defaultPath + storageFileName);
            
            // Iterating through the lines of the file
            foreach (string line in lines)
                addToSportsEventListFromFileLine(line);
        }

        private void addToSportsEventListFromFileLine(string line)
        {
            PastSportEvent spTemp = getSportEventFromString(line);
            sportsEventList.Add(spTemp.ID, spTemp);
        }

        private void addToSportsEventList(PastSportEvent spev)
        {
            sportsEventList.Add(spev.ID, spev);
        }


        private PastSportEvent getSportEventFromString(string line)
        {
            PastSportEvent TempSpEv = new PastSportEvent();
            string[] attributes;
            string[] separator = new string[] {"\t"};

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");

            attributes = line.Split(separator, StringSplitOptions.None);

            try
            { TempSpEv.ID = (long)Convert.ToInt32(attributes[0]); }
            catch
            { throw new InvalidOperationException("Pilisi nyenyec."); }

            try
            { TempSpEv.number = Convert.ToInt32(attributes[1]); }
            catch
            { throw new InvalidOperationException("Pilisi nyenyec."); }

            TempSpEv.betting_event = attributes[2];

            try
            { TempSpEv.min_tie = Convert.ToInt32(attributes[3]); }
            catch
            { throw new InvalidOperationException("Pilisi nyenyec."); }

            TempSpEv.sport       = attributes[4];
            TempSpEv.sport_event = attributes[5];
            TempSpEv.result      = attributes[6];
            TempSpEv.outcome     = attributes[7];

            try
            { TempSpEv.Hodds = (float)Convert.ToDouble(attributes[8]); }
            catch
            { throw new InvalidOperationException("Pilisi nyenyec."); }
            
            try
            { TempSpEv.Dodds = (float)Convert.ToDouble(attributes[9]); }
            catch
            { throw new InvalidOperationException("Pilisi nyenyec."); }
            
            try
            { TempSpEv.Vodds = (float)Convert.ToDouble(attributes[10]); }
            catch
            { throw new InvalidOperationException("Pilisi nyenyec."); }
            
            try
            { TempSpEv.Wodds = (float)Convert.ToDouble(attributes[11]); }
            catch
            { throw new InvalidOperationException("Pilisi nyenyec."); }

            Thread.CurrentThread.CurrentCulture = new CultureInfo("hu");

            return TempSpEv;
        }

        private void addListFromWeb()
        {
            // Determining the list of read elements
            List<round> allRounds = ReadWhatToRead();

            // Getting the results of the events of listed rounds
            fillResultsFromWeb(allRounds);

            // Getting the offers for the existing SportEvents
            getOfferList(allRounds);

            // Writing list to file
            writeSPListToFile();
        }

        private void fillResultsFromWeb(List<round> allRounds)
        {
            //List<SportEvent> sportsEventList = new List<SportEvent>();
            int size = allRounds.Count;

            // Iterating through the rounds
            for (int i = size - 1; i >= 0; i--)
            {
                string URIresult = System.String.Format("http://www.szerencsejatek.hu/tippmix-regebbi-eredmenyek?ev={0}&het={1}&fordulo={2}", allRounds[i].year, allRounds[i].week, allRounds[i].round_number);
                string webPageContent = Accessories.downloadStringUTF8(URIresult);
                Thread.CurrentThread.CurrentCulture = new CultureInfo("hu");
                fillResultsFromWebContent(allRounds, i, webPageContent);
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            }
        }

        private void fillResultsFromWebContent(List<round> allRounds, int i, string webPageContent)
        {
            // One regex rules them all!!
            string patternResults = @"<tr>\s+<td>(\d{3})</td>\s+<td>(<strong>.+</strong>.*)</td>(\s+<td>(.+)</td>)?\s+<td>([&;\s\w\.-]+)</td>\s+<td>([\s\w]+)</td>\s+<td>\s+([\d\.]+)";

            // Matching pattern and
            int number = 1;

            foreach (Match match in Regex.Matches(webPageContent, patternResults))
                if (match.Success)
                {
                    PastSportEvent temp = convertMatchToResult(match, allRounds[i], number);
                    long ID = temp.ID;
                    if (!sportsEventList.ContainsKey(ID))
                        sportsEventList.Add(ID, temp);
                    number++;

                }
        }

        private void fillOffersFromWebContent(round rnd, string webPageContent)
        {
            string patternOffers = @"<p>(\d{3})  (.+)<br />\n([&#8211;0-9]+) / (.+) / (.+)<br />\nH ([&#8211;0-9,]+) / D ([&#8211;0-9,]+) / V ([&#8211;0-9,]+) / (.+)</p>";

            Thread.CurrentThread.CurrentCulture = new CultureInfo("hu");
            foreach (Match match in Regex.Matches(webPageContent, patternOffers))
                if (match.Success)
                    convertMatchToEvent(ref sportsEventList, match, rnd);

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
        }

        private void fillCurrentOffersFromWebContent(string webPageContent)
        {
            string patternOffers = @"<tr class=.+>\s+<td class=.opener nowrap.><ins></ins>(\d{3})</td>\s+<td class=.center nowrap.><ins class=.(.+). title=.(.+).><span>(.+)</span></ins>(\d)</td>\s+<td><strong>(.+)</strong></td>\s+<td class=.center.>(\s+<span.+>)?\s+([\d.-]+)(\s+</span>)?\s+</td>\s+<td class=.center.>(\s+<span.+>)?\s+([\d.-]+)(\s+</span>)?\s+</td>\s+<td class=.center.>(\s+<span.+>)?\s+([\d.-]+)(\s+</span>)?\s+</td>\s+<td class=.nowrap.>\s+(.+)\s+</td>\s+<td class=.nowrap.>\s+(.+)\s+</td>\s+<td>\s+(<a href=.(.+). class=.(.+).>(.+)</a>\s+)?</td>\s+</tr>\s+<tr class=.+>\s+<td colspan=.3.>(.+)</td>\s+<td colspan=.6.>(.+)</td>\s+</tr>";
            round currentRound = getRoundFromContentsOfCurrentOffers(webPageContent);

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            foreach (Match match in Regex.Matches(webPageContent, patternOffers))
                if (match.Success)
                    addToSportsEventList(convertCurrentOfferMatchToOffer(match, currentRound));

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
        }

        private static round getRoundFromContentsOfCurrentOffers(string webPageContent)
        {
            string roundPattern = @"<h2>(\d{4}). (\d{1,2}). hét (\d). forduló</h2>";

            Regex date = new Regex(roundPattern);

            int year, week, round;

            string yearStr = date.Match(webPageContent).Groups[1].Value;
            string weekStr = date.Match(webPageContent).Groups[2].Value;
            string roundStr = date.Match(webPageContent).Groups[3].Value;

            try
            { year = Convert.ToInt32(yearStr); }
            catch
            { throw new InvalidOperationException(String.Format("Cannot convert yearStr ({0}) to Int32.", yearStr)); }

            try
            { week = Convert.ToInt32(weekStr); }
            catch
            { throw new InvalidOperationException(String.Format("Cannot convert weekStr ({0}) to Int32.", weekStr)); }

            try
            { round = Convert.ToInt32(roundStr); }
            catch
            { throw new InvalidOperationException(String.Format("Cannot convert roundStr ({0}) to Int32.", roundStr)); }

            return new round(year, week, round);
        }

        public List<round> ReadWhatToRead()
        {
            // Downloading the webpage
            List<round> rounds = new List<round>();
            string webPage = Accessories.downloadStringUTF8("http://www.szerencsejatek.hu/tippmix-regebbi-eredmenyek");

            // Initializing the regular expression object
            // This pattern matces each line which contain the link we are looking for, and groups the seeken year, week and round numbers.
            string pattern = @"<a href=./tippmix-regebbi-eredmenyek\?ev=\d+&amp;het=\d+&amp;fordulo=\d+.>(\d+)\. (\d+)\. hét (\d+)\. forduló</a>";
            Regex regex = new Regex(pattern);
            
            // Filling 'rounds'
            foreach (Match match in Regex.Matches(webPage, pattern))
                if (match.Success)
                    rounds.Add(convertMatchToRound(match));
                
            // Returning with 'rounds'
            return rounds;
        }

        public round convertMatchToRound(Match match)
        {
            round temp = new round();

            try
                { temp.year = Convert.ToInt32(match.Groups[1].Value); }
            catch
                { throw new InvalidOperationException("Pilisi nyenyec."); }

            try
                { temp.week = Convert.ToInt32(match.Groups[2].Value); }
            catch
                { throw new InvalidOperationException("Pilisi nyenyec."); }

            try
                { temp.round_number = Convert.ToInt32(match.Groups[3].Value); }
            catch
                { throw new InvalidOperationException("Pilisi nyenyec."); }

            return temp;

        }

        static void convertMatchToEvent(ref /*List<SportEvent>*/SortedDictionary<long, PastSportEvent> spev, Match match, round rnd)
        {
            // Determining index of SportEvent in question
            int number;
            try
            { number = Convert.ToInt32(match.Groups[1].Value); }
            catch
            { throw new InvalidOperationException("Pilisi nyenyec."); }

            long tempID = 1000000 * rnd.year + 10000 * rnd.week + 1000 * rnd.round_number + number;
            //int tempIndex = Accessories.getEventFromId(spev, tempID);

            if (spev.ContainsKey(tempID))
            {
                // Console.WriteLine(match.Groups[3].Value.ToString());
                if (match.Groups[3].Value == "&#8211;")
                {
                    spev[tempID].min_tie = 0;
                }
                else
                {
                    try
                    {
                        spev[tempID].min_tie = Convert.ToInt32(match.Groups[3].Value);
                    }
                    catch
                    {
                        throw new InvalidOperationException
                        ("Cannot convert min_tie (" + match.Groups[3].Value + ") to Int32.");
                    }
                }
                spev[tempID].sport = match.Groups[4].Value.Replace("&#8211;", "-");
                spev[tempID].sport_event = match.Groups[2].Value.Replace("&#8211;", "-");
                if (match.Groups[6].Value == "&#8211;")
                    spev[tempID].Hodds = 0;
                else
                    spev[tempID].Hodds = (float)Convert.ToDouble(match.Groups[6].Value);
                if (match.Groups[7].Value == "&#8211;")
                    spev[tempID].Dodds = 0;
                else
                    spev[tempID].Dodds = (float)Convert.ToDouble(match.Groups[7].Value);
                if (match.Groups[8].Value == "&#8211;")
                    spev[tempID].Vodds = 0;
                else
                    spev[tempID].Vodds = (float)Convert.ToDouble(match.Groups[8].Value);
            }
        }

        static PastSportEvent convertMatchToResult(Match match, round rnd, int number)
        {
            PastSportEvent spev = new PastSportEvent();

            int size = match.Groups.Count;
            //                                     1 number                   2 bettig event      3 dummy 4 sport           5 result                  6 outcome               7 winnig odds
            //string patternResults = @"<tr>\s+<td>(\d{3})</td>\s+<td><strong>(.+)</strong>.*</td>(\s+<td>(.+)</td>)?\s+<td>([&;\s\w\.-]+)</td>\s+<td>([\s\w]+)</td>\s+<td>\s+([\d\.]+)";
            
            try
            {
                spev.number = number;
            }
            catch
            {
                throw new InvalidOperationException
                ("Pilisi nyenyec.");
            }

            spev.ID = 1000000 * rnd.year + 10000 * rnd.week + 1000 * rnd.round_number + number;

            spev.betting_event = match.Groups[2].Value.Replace("&#8211;", "-").Trim();
            
            spev.sport = match.Groups[4].Value.Replace("&#8211;", "-").Trim();
            spev.result = match.Groups[5].Value.Replace("&#8211;", "-").Trim();
            spev.outcome = match.Groups[6].Value.Replace("&#8211;", "-").Trim();
            if (match.Groups[7].Value == "&#8211;")
                spev.Wodds = 0;
            else
            {
                //Console.WriteLine("Converting {0} to double...", match.Groups[6].Value);
                spev.Wodds = (float)Convert.ToDouble(match.Groups[7].Value.Replace(".", ",").Trim());
            }

            return spev;
        }

        public PastSportEvent convertCurrentOfferMatchToOffer(Match match, round CurrentRound)
        {
            PastSportEvent spev = new PastSportEvent();

            // Determining index of SportEvent in question
            try   { spev.number = Convert.ToInt32(match.Groups[1].Value);                                          }
            catch { throw new InvalidOperationException("Cannot convert '" + match.Groups[1].Value + "' to int."); }

            spev.ID = 1000000 * CurrentRound.year + 10000 * CurrentRound.week + 1000 * CurrentRound.round_number + spev.number;

            spev.betting_event = match.Groups[6].Value;

            try   { spev.min_tie = Convert.ToInt32(match.Groups[5].Value);                                         }
            catch { throw new InvalidOperationException("Cannot convert '" + match.Groups[5].Value + "' to int."); }

            spev.sport = match.Groups[23].Value;
            spev.sport_event = match.Groups[22].Value;

            if (match.Groups[8].Value == "---")
                spev.Hodds = 0;
            else
            {
                try   { spev.Hodds = (float)Convert.ToDouble(match.Groups[8].Value);                                      }
                catch { throw new InvalidOperationException("Cannot convert '" + match.Groups[8].Value + "' to double."); }
            }
            if (match.Groups[11].Value == "---")
                spev.Dodds = 0;
            else
                spev.Dodds = (float)Convert.ToDouble(match.Groups[11].Value);
            if (match.Groups[14].Value == "---")
                spev.Vodds = 0;
            else
                spev.Vodds = (float)Convert.ToDouble(match.Groups[14].Value);
             
            return spev;
        }
    }
}
