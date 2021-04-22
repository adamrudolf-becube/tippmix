using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TippMix
{
    /// <summary>
    /// Base class for sport events.
    /// </summary>
    class SportEvent
    {
        /// <summary>
        /// It's a unique ID of an event. It's YYYYWWRNNN: Year, week, round, and number.
        /// </summary>
        public long ID;
        public DateTime date;

        /// <summary>
        /// Determines the round (year, week and round number) from the SporrtEvent object's ID attribute.
        /// </summary>
        /// <returns> a round object which belongs to the current SportEvent object.</returns>
        public round getRound()
        {
            int year = (int)Math.Floor(1.0 * ID / 1000000.0);
            int week = (int)Math.Floor((1.0 * ID - 1000000.0 * year) / 10000.0);
            int round_number = (int)Math.Floor((1.0 * ID - 1000000.0 * year - 10000.0 * week) / 1000.0);
            
            round ret = new round(year, week, round_number);

            return ret;
        }

        public int getRoundNumber()
        {
            return (int)Math.Floor(1.0 * ID / 1000.0);
        }

        /// <summary>
        /// Determines the number of a SportEvent object from it's ID.
        /// </summary>
        /// <remarks>The number of a SportEvent object is the number within one round. It starts from 1 and usually less then 256. It is the last 3 digits of the ID.</remarks>
        /// <returns> the number of the SportEvent object in an int.</returns>
        public int getNumber()
        {
            return (int)(ID % (long)1000);
        }
    }

    /// <summary>
    /// One option to bet. Belongs to a certain outcome of a SportEvent. Contains information about the ID,  Child class of SportEvent.
    /// </summary>
    class Bettable : SportEvent
    {
        public string outcome;
        public float odds;
        public double probability;

        public Bettable(long _ID, string _outcome, float _odds, double _probability)
        {
            ID = _ID;
            outcome = _outcome;
            odds = _odds;
            probability = _probability;
        }
    }

    /// <summary>
    /// Sores elements of the current offer.
    /// </summary>
    class Offer : SportEvent
    {
        public int number;
        public string betting_event;
        public int min_tie;
        public string sport;
        public string sport_event;
        public float Hodds;
        public float Dodds;
        public float Vodds;
    }

    /// <summary>
    /// A sport event which already happened. Inherits from Offer, because we know offers from the past (in the ideal case), plus reults.
    /// </summary>
    class PastSportEvent : Offer
    {
        
        public float    Wodds;
        /// <summary>
        /// Can be either "H", "D", "V" or "N" if there is no result yet.
        /// </summary>
        public string   outcome = "N";
        public string   result = "";

        public PastSportEvent()
        {
            this.ID = 0;
            this.number = 0;
            this.betting_event = "N/A";
            this.min_tie = -1;
            this.sport = "N/A";
            this.sport_event = "N/A";
            this.Hodds = 0;
            this.Dodds = 0;
            this.Vodds = 0;
            this.Wodds = 0;
        }

        public PastSportEvent(PastSportEvent subject)
        {
            this.ID = subject.ID;
            this.number = subject.number;
            this.betting_event = subject.betting_event;
            this.min_tie = subject.min_tie;
            this.sport = subject.sport;
            this.sport_event = subject.sport_event;
            this.Hodds = subject.Hodds;
            this.Dodds = subject.Dodds;
            this.Vodds = subject.Vodds;
            this.Wodds = subject.Wodds;
            this.date = subject.date;
        }

        public bool containsOffer()
        {
            return Hodds != 0 || Dodds != 0 || Vodds != 0;
        }

        /// <summary>
        /// Returns the string representation of the SportEventinstance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string ret;
            ret = System.String.Format("Number: {0}, bet event: {1}\n", number, betting_event);
            ret += System.String.Format("Min tie: {0}, sport: {1}, sport event: {2}\n", min_tie, sport, sport_event);
            ret += System.String.Format("Odds: H {0}, D {1}, V {2}\n\n", Hodds, Dodds, Vodds);
            return ret;
        }

        public string ToTabSeparatedString()
        {
            return System.String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}", ID, number, betting_event, min_tie, sport, sport_event, result, outcome, Hodds, Dodds, Vodds, Wodds);
        }
    }
}
