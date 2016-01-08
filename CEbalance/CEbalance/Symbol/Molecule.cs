using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CEbalance.Symbol
{
    public class Molecule
    {
        private IList<MultiIon> formula;
        
        public Molecule()
        {
            formula = new List<MultiIon>();
            Factor = 1;
        } 

        public void AddIons(Ion[] ions, int key = 1)
        {
            formula.Add(new MultiIon(ions, key));
        }

        public IList<MultiIon> Formula
        {
            get
            {
                return formula;
            }
        }

        public override string ToString()
        {
            var ret = formula.Aggregate(String.Empty,
                (seed, ion) => seed + ion.ToString());

            return Factor > 1 ? String.Format("{0}{1}", Factor, ret) : ret;
        }

        public int Factor
        {
            get; set;
        }
    }
}
