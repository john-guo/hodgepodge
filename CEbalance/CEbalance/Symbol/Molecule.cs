using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CEbalance.Symbol
{
    public class Molecule
    {
        private IList<Ion> formula;
        
        public Molecule(Ion[] ions)
        {
            formula = new List<Ion>(ions);
            Factor = 1;
        } 

        public IList<Ion> Formula
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
