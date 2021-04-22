using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace TippMix
{
    class PortfolioOptimizer
    {
        private static Model modell;
        private static List<int> weights = new List<int>();
        private static int sumOfWeights;
        private Distribution dist;
        private int numberOfBettables;
        private int testSetSize = 20000;
        static List<List<int>> testSet = new List<List<int>>();

        bool withPlusCondition;

        delegate double riskMeasure();

        public int getNumberOfBettables()
        {
            return modell.bettables.Count;
        }

        public PortfolioOptimizer(int W, bool WithPlusCondition)
        {
            Random rnd = new Random();

            modell = new odds_model(WithPlusCondition);
            modell.setBettables();

            setTestSet(rnd, testSetSize);

            numberOfBettables = modell.bettables.Count;
            sumOfWeights = W;

            withPlusCondition = WithPlusCondition;
            
            initialzeWeights();
        }

        public void produceStatistics()
        {
            Console.WriteLine("Determining round list...");
            SportEventDictionary ds = new SportEventDictionary();
            List<round> roundList = ds.ReadWhatToRead();
            Console.WriteLine("Determining round list done...");

            TextWriter logfile = new StreamWriter(Accessories.defaultPath + "logFile.txt");

            // Iterating through past rounds
            for (int i = 0; i <= 100; i++)
            {
                Console.WriteLine("Round: " + roundList[i].ToString() + ", vectorIterator = " + i.ToString());
                modell = new odds_model(true);
                modell.setBettablesForTest(roundList[i]);
                numberOfBettables = modell.bettables.Count;
                Random rnd = new Random();

                if (numberOfBettables != 0)
                {
                    initialzeWeights();

                    setTestSet(rnd, testSetSize);

                    setBetList();

                    string round = roundList[i].year.ToString() + roundList[i].week.ToString() + roundList[i].round_number.ToString();
                    string filename = round + ".txt";
                    TextWriter betList = new StreamWriter(Accessories.defaultPath + @"betLists\" + filename);
                    betList.WriteLine(betListToString());
                    betList.Close();

                    logfile.WriteLine(round + "\t{0}\t{1}\t{2}\t{3}", dist.getExpectedValue(), finalNetIncomeForTest(), dist.getUsedRiskMeasure(), 100 * finalNetIncomeForTest() / (1.0 * sumOfWeights));
                }
            }

            logfile.Close();
        }

        public float finalNetIncomeForTest()
        {
            Console.WriteLine("Started to count income...");
            float tempIncome = 0.0F;
            PastSportEvent tempSE;

            for (int i = 0; i < modell.bettables.Count; i++)
                if (modell.sportEvents.sportsEventList.TryGetValue(modell.bettables[i].ID, out tempSE))
                    if (tempSE.outcome == modell.bettables[i].outcome)
                        tempIncome += modell.bettables[i].odds * weights[i];

            Console.WriteLine("Finished to count income...");
            return tempIncome - (float)sumOfWeights;
        }

        public void setBetList()
        {
            List<int> prev = new List<int>();

            while (!weights.SequenceEqual(prev))
            {
                prev = weights;
                stepWeights();

                dist = new Distribution(weights);

                Console.WriteLine("Iteráció, {0} {1}", dist.getExpectedValue(), dist.getUsedRiskMeasure());
            }

            DateTime thisDay = DateTime.Today;
            string date = thisDay.Year.ToString() + "_" + thisDay.Month.ToString() + "_" + thisDay.Day.ToString();


            TextWriter tw = new StreamWriter(Accessories.defaultPath + "finalResult_" + date + "_" + sumOfWeights.ToString() + "_" + withPlusCondition.ToString() + ".txt");
            tw.WriteLine(betListToString());
            tw.Close();
        }

        public string betListToString()
        {
            string returnSting = "";

            for (int i = 0; i < modell.bettables.Count; i++)
                returnSting += modell.bettables[i].ID + "\t" + modell.bettables[i].getNumber().ToString() + "\t" + modell.bettables[i].outcome + "\t" + weights[i] + "\n";

            return returnSting;
        }

        public class Distribution
        {
            SortedDictionary<float, double> probabilities = new SortedDictionary<float, double>();
            int count = modell.bettables.Count;
            public static int numberOfPoints;
            List<int> ownWeights;
            double ratioAllPossibilitiesAndN;
            double norm;
            
            public Distribution(List<int> w)
            {
                ownWeights = w;
                Int64 allPossibilities = (Int64)Math.Pow(2, count);
                ratioAllPossibilitiesAndN = (1.0 * allPossibilities) / (1.0 * numberOfPoints);
                fillProbabilities();
                norm = getNorm();
            }

            public double getUsedRiskMeasure()
            {
                //return getSemiVariance();
                return getUpperVaR(0.95);
            }

            public double getExpectedValue()
            {
                double ret = 0;

                foreach (KeyValuePair<float, double> pair in probabilities)
                    ret += pair.Key * pair.Value;

                return ret / norm;
            }

            public double getNorm()
            {
                double ret = 0;

                foreach (KeyValuePair<float, double> pair in probabilities)
                    ret += pair.Value;

                return ret;
            }

            public double getMedian()
            {
                if (probabilities.Count % 2 == 0)
                    return 0.5 * (probabilities.ElementAt((int)(0.5 * probabilities.Count)).Value + probabilities.ElementAt((int)(0.5 * probabilities.Count + 1)).Value);
                else
                    return probabilities.ElementAt(probabilities.Count / 2).Value;
            }

            public double getStandardDeviation()
            {
                double expectedValue = getExpectedValue();
                double sumOfSquares = 0.0;

                foreach (KeyValuePair<float, double> pair in probabilities)
                    sumOfSquares += pair.Value * Math.Pow((pair.Key - expectedValue), 2);

                return Math.Sqrt(sumOfSquares);
            }
            
            public double getMeanAbsuluteDeviance()
            {
                throw new NotImplementedException();
            }

            public double getSemiVariance()
            {
                double expectedValue = getExpectedValue();
                double sumOfSquares = 0.0;

                foreach (KeyValuePair<float, double> pair in probabilities)
                    if (pair.Key < 0)
                        sumOfSquares += pair.Value * Math.Pow((pair.Key - expectedValue), 2);

                return sumOfSquares;
            }

            public double getUpperVaR(double alpha)
            {
                double cumulatedProbabilities = 0;
                double aim = (1.0 - alpha);

                foreach (KeyValuePair<float, double> pair in probabilities)
                {
                    cumulatedProbabilities += pair.Value / norm;
                    if (cumulatedProbabilities > aim)
                        return -1.0 * pair.Key;
                }

                return -1;
            }

            public double getLowerVaR(double alpha)
            {
                double cumulatedProbabilities = 0;
                double aim = (1.0 - alpha) * norm;
                float prev = -1.0F * sumOfWeights;

                foreach (KeyValuePair<float, double> pair in probabilities)
                {
                    cumulatedProbabilities += pair.Value;
                    if (cumulatedProbabilities > aim)
                        return prev;
                    prev = pair.Key;
                }

                return -1;
            }

            public double getCVaR(double confidenceLevel)
            {
                throw new NotImplementedException();
            }

            public double getEstimatedShortfall(double confidenceLevel)
            {
                throw new NotImplementedException();
            }

            public double getProbabilityOfLoss()
            {
                double ret = 0;

                foreach (KeyValuePair<float, double> pair in probabilities)
                    if (pair.Key < 0)
                        ret += pair.Value;
                    else
                        break;

                return ret / norm;
            }

            private void fillProbabilities()
            {
                for (int j = 0; j < numberOfPoints; j++)
                {
                    float tempIncome = 0.0F;
                    double tempProbability = 1.0;

                    for (int i = 0; i < modell.bettables.Count; i++)
                    {
                        tempIncome += ownWeights[i] * (testSet[j][i] * modell.bettables[i].odds - 1);
                        tempProbability *= 2 * testSet[j][i] * modell.bettables[i].probability - modell.bettables[i].probability - testSet[j][i] + 1;
                    }

                    if (probabilities.ContainsKey(tempIncome))
                        probabilities[tempIncome] += tempProbability;
                    else
                        probabilities.Add(tempIncome, tempProbability);
                }
            }
        }
        
        static void setTestSet(Random rnd, int number)
        {
            testSet = new List<List<int>>();

            int count = modell.bettables.Count;

            if (Math.Pow(2, count) <= number)
            {
                number = (int)Math.Pow(2, count);
                testSet = generateAllPossibilities(number);
            }
            else
            {
                for (int i = 0; i < number; i++)
                    addVectorToTestSet(rnd);
            }

            Distribution.numberOfPoints = number;

        }

        static List<List<int>> generateAllPossibilities(int number)
        {
            //int count = modell.bettables.Count;
            int count = number;
            int allPossibilities = (int)Math.Pow(2, count);
            List<List<int>> wholeList = new List<List<int>>();

            for (int vectorIterator = 0; vectorIterator < allPossibilities; vectorIterator++)
            {
                List<int> tempList = new List<int>();

                int tempSum = vectorIterator;

                tempList = new List<int>(new int[count]);

                int counter = 0;
                while (tempSum >= 1)
                {
                    int remainder = tempSum % 2;
                    tempSum = (int)Math.Floor(1.0 * tempSum / 2.0);

                    if (remainder == 1)
                        tempList[counter] = 1;

                    counter++;
                }

                wholeList.Add(tempList);
            }

            return wholeList;
        }

        static void addVectorToTestSet(Random rnd)
        {
            List<int> sTemp = new List<int>();

            foreach (Bettable bettable in modell.bettables)
                sTemp.Add(oneWithProbabilityOf(bettable.probability, rnd));

            testSet.Add(sTemp);
        }

        static int oneWithProbabilityOf(double probability, Random rnd)
        {
            if (rnd.NextDouble() < probability)
                return 1;
            else
                return 0;
        }

        public void DEBUG()
        {
            Random rnd = new Random();
            Distribution d;
            Console.WriteLine("Distribution done, setting random vector...");
            setTestSet(rnd, 100000);
            Console.WriteLine("Generating istribution...");
            d = new Distribution(weights);

            Console.WriteLine("Writing to file...");
            TextWriter tw1 = new StreamWriter(Accessories.defaultPath + "debuggggggg.dat");

            foreach(List<int> actual in testSet)
            {
                foreach (int n in actual)
                {
                    tw1.Write("{0}\t", n);
                }
                tw1.Write("\n");
            }
            foreach (Bettable bettable in modell.bettables)
            {
                tw1.Write("{0}\t", bettable.probability);
            }
            tw1.Write("\n\n");
            foreach (Bettable bettable in modell.bettables)
            {
                tw1.Write("{0}\t", bettable.odds);
            }
            foreach (int weigh in weights)
            {
                tw1.Write("{0}\t", weigh);
            }
            tw1.Close();

            Console.WriteLine("Yield: {0}, PoL: {1}", d.getExpectedValue(), d.getUsedRiskMeasure());
        }

        private void initialzeWeights()
        {
            fillWeightsWithZeros();

            for(int i = 0; i < sumOfWeights; i++)
                weights[i % numberOfBettables]++;
        }

        private void fillWeightsWithZeros()
        {
            weights = new List<int>();

            for (int i = 0; i < numberOfBettables; i++)
                weights.Add(0);
        }

        private void stepWeights()
        {
            List<List<int>> neighbors = determineNeighbors();
            weights = getMinimax(neighbors);
        }

        //TODO: Refactor
        private List<int> getMinimax(List<List<int>> listOfVectors)
        {
            Distribution d;
            int indexOfMinimal;
            int i = 0, j = 1;
            d = new Distribution(listOfVectors[0]);
            double risk = d.getUsedRiskMeasure();
            SortedDictionary<double, List<int>> listOfMinimals = new SortedDictionary<double, List<int>>();

            foreach (List<int> actual in listOfVectors)
            {
                d = new Distribution(actual);
                double tempProb = d.getUsedRiskMeasure();
//Console.WriteLine("\t{0} of {1} calculated: PoL = {2}, expectedYield = {3}", j, listOfVectors.Count, tempProb, d.getExpectedValue());
j++;
                if (tempProb == risk)
                    if (!listOfMinimals.ContainsKey(d.getExpectedValue()))
                        listOfMinimals.Add(d.getExpectedValue(), actual);
                if (tempProb < risk)
                {
                    listOfMinimals.Clear();
                    listOfMinimals.Add(d.getExpectedValue(), actual);
                    risk = tempProb;
                    indexOfMinimal = i;
                }
                i++;
            }

            return listOfMinimals.Last().Value;
        }

        private List<List<int>> determineNeighbors()
        {
            List<List<int>> listOfNeighbors = new List<List<int>>();
            listOfNeighbors.Add(weights);

            for (int i = 0; i < numberOfBettables; i++)
            {
                if (weights[weights.Count - 1] > 0)
                    listOfNeighbors.Add(ithIncremented(i));
                if (weights[i] > 0)
                    listOfNeighbors.Add(ithDecremented(i));
            }

            return listOfNeighbors;
        }

        private static List<int> ithDecremented(int i)
        {
            List<int> temp = new List<int>(weights);
            temp[i]--;
            temp[weights.Count - 1]++;
            return temp;
        }

        private static List<int> ithIncremented(int i)
        {
            List<int> temp = new List<int>(weights);
            temp[i]++;
            temp[weights.Count - 1]--;
            return temp;
        }

        private int sumOfWeightsExceptLast()
        {
            int returnValue = 0;

            for(int j = 0; j < weights.Count - 1; j++)
                returnValue += weights[j];

            return returnValue;
        }

    }
}