using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TippMix
{
    class Bet
    {
        public long ID;
        public long number;
        public string outcome;
        public int money;

        public Bet(long _ID, long _number, string _outcome, int _money)
        {
            ID = _ID;
            number = _number;
            outcome = _outcome;
            money = _money;
        }
    }
}
