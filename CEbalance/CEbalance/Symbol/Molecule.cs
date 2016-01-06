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
            return formula.Aggregate(String.Empty, 
                (seed, ion) => seed + ion.ToString());
        }
    }
}
