using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TippMix
{
    /// <summary>
    /// Absctract superclass for the probability estimator models. Provides a common function and attribute set so the different models are interchangable.
    /// </summary>
    abstract class Model
    {
        public SportEventDictionary sportEvents = new SportEventDictionary();
        //public SortedDictionary<long, Offer> offers = new SortedDictionary<long,Offer>();
        public SportEventDictionary offers = new SportEventDictionary();
        public List<Bettable> bettables = new List<Bettable>();

        public abstract void setBettables();

        public abstract void setBettablesForTest(round now);

    }
}
