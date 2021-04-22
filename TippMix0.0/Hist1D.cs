using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TippMix
{
    using System.IO;

    /// <summary>
    /// This class represents a 1 dimensional histogram. Similar to the class Distribution, but bin size is constant and is not normalized.
    /// </summary>
    /// <typeparam name="T">Class puts this type of data to bins. Must be able to be converted to double.</typeparam>
    class Hist1D<T>
    {
        /// <summary>
        /// Number of bins in the histogram.
        /// </summary>
        public int binNumber;
        /// <summary>
        /// Size (= width) of one bin. "Right side" of the histogram is binNumber * binSize.
        /// </summary>
        public double binSize;
        /// <summary>
        /// Class stores the values in this array. This is the histogram itself.
        /// </summary>
        public int[] hist;

        /// <summary>
        /// Returns the index of the bin in the histogram which contains the 'data' if the bin size is given.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when method can not convert T to double.</exception>
        /// <param name="data">The numerical data which we want to put in a bin. Type must be able to be converted to double.</param>
        /// <returns>an integer which is the index of the bin data belongs to.</returns>
        public int whichBin(T data)
        {
            double dataDouble;
            try
            {
                dataDouble = Convert.ToDouble(data);
            }
            catch
            {
                throw new InvalidOperationException
                ("T must be a type convertable to double.");
            }
            return (int)Math.Floor((1.0 * dataDouble + (0.5 * binSize)) / (1.0 * binSize));
        }

        /// <summary>
        /// Adds 1 to the bin data belongs to.
        /// </summary>
        /// <param name="data">The numerical data which we want to put in a bin. Type must be able to be converted to double.</param>
        public void addData(T data)
        {
            int bin = whichBin(data);
            if (bin < binNumber)
                hist[bin]++;
        }

        /// <summary>
        /// Fills all of the bins with the given value.
        /// </summary>
        /// <param name="value">Sets all of the bin values to this value.</param>
        public void fillWith(int value)
        {
            for (int i = 0; i < hist.Count(); i++)
                hist[i] = value;
        }

        /// <summary>
        /// Initializes a new instance of Hist1D with tha given bin size and bin number, filled with the elements of dataSet.
        /// </summary>
        /// <param name="dataSet">List of T type objects. Elements must be able to be converted to double.</param>
        /// <param name="binSize_">Size (= width) of one bin.</param>
        /// <param name="binNumber_">Number of the binSize_ sized bins.</param>
        public Hist1D(List<T> dataSet, double binSize_, int binNumber_)
        {
            binSize = binSize_;
            binNumber = binNumber_;
            hist = new int[binNumber_];
            fillWith(0);
            foreach (T data in dataSet)
                addData(data);
        }

        /// <summary>
        /// Initializes a new instance of Hist1D with tha given bin size and bin number, filled with zeros.
        /// </summary>
        /// <param name="binSize_">Size (= width) of one bin.</param>
        /// <param name="binNumber_">Number of the binSize_ sized bins.</param>
        public Hist1D(double binSize_, int binNumber_)
        {
            binSize = binSize_;
            binNumber = binNumber_;
            hist = new int[binNumber_];
            fillWith(0);
        }

        /// </summary>
        /// <remarks>This method uses the System.TextWriter class to write to a file. The first two lines are comments,
        /// beginning with the '#' mark for further working with the file in GNUPLOT. User can name the data so in the comments
        /// that name will be shown.
        /// Each line of the file represents a bin.
        /// The method writes the index of the bin, the bin limit, and the integer value of the bin.
        /// <param name="fileName">Method will name the .dat file afterthis at the given path. Must not contain extension.</param>
        /// <param name="path">Method will save the file at this path. Can be omitted. If omitted, method will save file to TwitterBase.defaultPath.</param>
        /// <param name="dataName">Name of the data. Appear in the dat description line of the file. Default value is "data".</param>
        /// <param name="comment">Optional comment line. Method will write this string at the first line of the file after a '#' mark. If omitted, first line is left uot entirely.</param>
        public void writeToFile(string fileName, string path = Accessories.defaultPath, string dataName = "data", string comment = "")
        {
            TextWriter tw1 = new StreamWriter(path + fileName + ".dat");
            if (comment != "")
                tw1.WriteLine("#" + comment);
            tw1.WriteLine("#index " + dataName + " count");
            for (int j = 0; j < binNumber - 1; j++)
                tw1.WriteLine("{0}\t{1}\t{2}", j, j * binSize, hist[j]);
            tw1.Close();
        }

        /// </summary>
        /// <remarks>This method uses the System.TextWriter class to write to a file. The first two lines are comments,
        /// beginning with the '#' mark for further working with the file in GNUPLOT. User can name the data so in the comments
        /// that name will be shown.
        /// Each line of the file represents a bin.
        /// The method writes the index of the bin, the bin limit, and the integer value of the bin.
        /// <param name="fileName">Method will name the .dat file afterthis at the given path. Must not contain extension.</param>
        /// <param name="substitute">In the file, method writes this value in the place of zeros. It is useful if we draw data in logscale.</param>
        /// <param name="path">Method will save the file at this path. Can be omitted. If omitted, method will save file to TwitterBase.defaultPath.</param>
        /// <param name="dataName">Name of the data. Appear in the dat description line of the file. Default value is "data".</param>
        /// <param name="comment">Optional comment line. Method will write this string at the first line of the file after a '#' mark. If omitted, first line is left uot entirely.</param>
        public void writeToFile(string fileName, double substitute, string path = Accessories.defaultPath, string dataName = "data", string comment = "")
        {
            TextWriter tw1 = new StreamWriter(path + fileName + ".dat");
            if (comment != "")
                tw1.WriteLine("#" + comment);
            tw1.WriteLine("#index " + dataName + " count");
            for (int j = 0; j < binNumber - 1; j++)
                if (hist[j] == 0)
                    tw1.WriteLine("{0}\t{1}\t{2}", j, j * binSize, substitute);
                else
                    tw1.WriteLine("{0}\t{1}\t{2}", j, j * binSize, hist[j]);
            tw1.Close();
        }
    }
}
