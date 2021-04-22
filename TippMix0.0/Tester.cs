using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TippMix
{
    class Tester
    {
        Model model;
        PortfolioOptimizer portfolioOptimizer;
        round nowTesting;

        public Tester(Model _model, PortfolioOptimizer _po)
        {
            model = _model;
            portfolioOptimizer = _po;
        }

        public void runTest()
        {
            // To be implemented
        }

        private double getRelativeIncomeForOneRound(round now)
        {
            model.setBettablesForTest(now);
            return 0.0;
        }
    }
}
