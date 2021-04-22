using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using System.Threading;

namespace TippMix
{
    class Maino
    {
        static void Main(string[] args)
        {

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            //List<List<int>> lista = generateAllPossibilities(3);
            //spd.uploadFile();
            PortfolioOptimizer port = new PortfolioOptimizer(5, false);
            Console.WriteLine("Bettables: " + port.getNumberOfBettables());
            Console.WriteLine("1 Optimizer has been created w 5...");
            port.setBetList();
            Console.WriteLine("1 Bet list done w 5...");
            port = new PortfolioOptimizer(20, false);
            Console.WriteLine("1 Optimizer has been created w 20...");
            port.setBetList();
            Console.WriteLine("1 Bet list done w 20...");
            port = new PortfolioOptimizer(50, false);
            Console.WriteLine("1 Optimizer has been created w 50...");
            port.setBetList();
            Console.WriteLine("1 Bet list done w 50...");
            port = new PortfolioOptimizer(100, false);
            Console.WriteLine("1 Optimizer has been created w 100...");
            port.setBetList();
            Console.WriteLine("1 Bet list done w 100...");

            port = new PortfolioOptimizer(5, true);
            Console.WriteLine("2 Bettables: " + port.getNumberOfBettables());
            Console.WriteLine("2 Optimizer has been created w 5...");
            port.setBetList();
            Console.WriteLine("2 Bet list done w 5...");
            port = new PortfolioOptimizer(20, true);
            Console.WriteLine("2 Optimizer has been created w 20...");
            port.setBetList();
            Console.WriteLine("2 Bet list done w 20...");
            port = new PortfolioOptimizer(50, true);
            Console.WriteLine("2 Optimizer has been created w 50...");
            port.setBetList();
            Console.WriteLine("2 Bet list done w 50...");
            port = new PortfolioOptimizer(100, true);
            Console.WriteLine("2 Optimizer has been created w 100...");
            port.setBetList();
            Console.WriteLine("2 Bet list done w 100...");
            
            
            /*
            PortfolioOptimizer om = new PortfolioOptimizer(25);
            Console.WriteLine("Optimizer has been created...");
            om.produceStatistics();
            //Console.WriteLine(om.finalNetIncomeForTest().ToString());

            Console.WriteLine("Done.");
             * */
            Console.ReadKey();
        }
    }
}
