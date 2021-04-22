using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TippMix
{
    class odds_model : Model
    {
        Hist1D<float> histOfOdds;
        Hist1D<float> histOfWinningOdds;

        int binNumbers = 600;
        float binSizes = 0.1F;
        bool withPlusCondition;

        public odds_model(bool WithPlusCondition)
        {
            Console.WriteLine("Filling old events...");
            sportEvents.fillFromFile();
            Console.WriteLine("Filling current events...");
            offers.getCurrentOffers();
            withPlusCondition = WithPlusCondition;
        }

        public override void setBettables()
        {
            bettables.Clear();
            initHistograms();

            foreach (KeyValuePair<long, PastSportEvent> pair in offers.sportsEventList)
                addOfferToBettablesList(pair.Value);
        }

        public override void setBettablesForTest(round now)
        {
            bettables.Clear();
            initHistogramsForTest(now);

            foreach (KeyValuePair<long, PastSportEvent> pair in sportEvents.sportsEventList)
                if (now.CompareTo(pair.Value.getRound()) == 0)
                    addOfferToBettablesList(pair.Value);
        }

        private void initHistograms()
        {
            histOfOdds = getHistOfOdds(sportEvents.sportsEventList);
            histOfWinningOdds = getHistOfWinningOddsAlternative(sportEvents.sportsEventList);
        }

        private void initHistogramsForTest(round now)
        {
            histOfOdds = getHistOfOddsForTest(sportEvents.sportsEventList, now);
            histOfWinningOdds = getHistOfWinningOddsAlternativeForTest(sportEvents.sportsEventList, now);
        }

        private void addOfferToBettablesList(Offer offer)
        {
            double probabilityOfHOdds = getProbabilityForOdds(offer.Hodds);
            double probabilityOfDOdds = getProbabilityForOdds(offer.Dodds);
            double probabilityOfVOdds = getProbabilityForOdds(offer.Vodds);

            bool HOfferExists = offer.Hodds != 0;
            bool DOfferExists = offer.Dodds != 0;
            bool VOfferExists = offer.Vodds != 0;

            bool HYieldIsGreaterThanOne = offer.Hodds * probabilityOfHOdds > 1.0;
            bool DYieldIsGreaterThanOne = offer.Dodds * probabilityOfDOdds > 1.0;
            bool VYieldIsGreaterThanOne = offer.Vodds * probabilityOfVOdds > 1.0;

            if (HOfferExists && HYieldIsGreaterThanOne)
                if ((probabilityOfHOdds > 0.5) || !withPlusCondition)
                    bettables.Add(new Bettable(offer.ID, "H", offer.Hodds, probabilityOfHOdds));
            if (DOfferExists && DYieldIsGreaterThanOne /*&& (probabilityOfDOdds > 0.5)*/)
                if ((probabilityOfDOdds > 0.5) || !withPlusCondition)
                    bettables.Add(new Bettable(offer.ID, "D", offer.Dodds, probabilityOfDOdds));
            if (VOfferExists && VYieldIsGreaterThanOne /*&& (probabilityOfVOdds > 0.5)*/)
                if ((probabilityOfVOdds > 0.5) || !withPlusCondition)
                    bettables.Add(new Bettable(offer.ID, "V", offer.Vodds, probabilityOfVOdds));
        }

        private double getProbabilityForOdds(float odds)
        {
            if (odds * binSizes < binNumbers)
                return (1.0 * getHistValueForData(histOfWinningOdds, odds)) / (1.0 * getHistValueForData(histOfOdds, odds));
            return 0;
        }

        private int getHistValueForData(Hist1D<float> hist, float data)
        {
            int tempIndex = hist.whichBin(data);
            if (tempIndex < hist.binNumber)
                return hist.hist[tempIndex];
            return -1;
        }

        private Hist1D<float> getHistOfOdds(SortedDictionary<long, PastSportEvent> sportsEventList)
        {
            Hist1D<float> hist = new Hist1D<float>(binSizes, binNumbers);

            foreach (KeyValuePair<long, PastSportEvent> pair in sportsEventList)
                hist = addOddsToHistogram(hist, pair);

            return hist;
        }

        private Hist1D<float> getHistOfOddsForTest(SortedDictionary<long, PastSportEvent> sportsEventList, round now)
        {
            Hist1D<float> hist = new Hist1D<float>(binSizes, binNumbers);

            foreach (KeyValuePair<long, PastSportEvent> pair in sportsEventList)
                if(now.CompareTo(pair.Value.getRound()) > 0)
                    hist = addOddsToHistogram(hist, pair);

            return hist;
        }

        private static Hist1D<float> addOddsToHistogram(Hist1D<float> hist, KeyValuePair<long, PastSportEvent> pair)
        {
            if (pair.Value.Hodds != 0)
                hist.addData(pair.Value.Hodds);

            if (pair.Value.Dodds != 0)
                hist.addData(pair.Value.Dodds);

            if (pair.Value.Vodds != 0)
                hist.addData(pair.Value.Vodds);

            return hist;
        }

        private Hist1D<float> getHistOfWinningOddsAlternative(SortedDictionary<long, PastSportEvent> sportsEventList)
        {
            Hist1D<float> hist = new Hist1D<float>(binSizes, binNumbers);

            foreach (KeyValuePair<long, PastSportEvent> pair in sportsEventList)
                hist = addWinningOddsToHistogram(hist, pair);

            return hist;
        }

        private Hist1D<float> getHistOfWinningOddsAlternativeForTest(SortedDictionary<long, PastSportEvent> sportsEventList, round now)
        {
            Hist1D<float> hist = new Hist1D<float>(binSizes, binNumbers);

            foreach (KeyValuePair<long, PastSportEvent> pair in sportsEventList)
                if (pair.Value.containsOffer() && now.CompareTo(pair.Value.getRound()) > 0)
                    hist = addWinningOddsToHistogram(hist, pair);

            return hist;
        }

        private static Hist1D<float> addWinningOddsToHistogram(Hist1D<float> hist, KeyValuePair<long, PastSportEvent> pair)
        {
            if (pair.Value.outcome == "H")
                hist.addData(pair.Value.Hodds);

            if (pair.Value.outcome == "D")
                hist.addData(pair.Value.Dodds);

            if (pair.Value.outcome == "V")
                hist.addData(pair.Value.Vodds);

            return hist;
        }

        private Hist1D<float> getHistOfWinningOdds(SortedDictionary<long, PastSportEvent> sportsEventList)
        {
            Hist1D<float> hist = new Hist1D<float>(binSizes, binNumbers);

            foreach (KeyValuePair<long, PastSportEvent> pair in sportsEventList)
                if (pair.Value.containsOffer())
                    hist.addData(pair.Value.Wodds);

            return hist;
        }
    }
}
