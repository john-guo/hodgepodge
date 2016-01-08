using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEbalance.Symbol
{
    public class MultiIon
    {
        private IList<Ion> _ions;
        private int _key;

        public MultiIon(Ion[] ions, int key)
        {
            _ions = new List<Ion>(ions);
            Key = key;
        }

        public IList<Ion> Ions
        {
            get
            {
                return _ions;
            }
        }

        public int Key { get; set; }

        public override string ToString()
        {
            var ret = _ions.Aggregate(String.Empty,
                    (seed, ion) => seed + ion.ToString());

            return Key > 1 ? string.Format("({0}){1}", ret, Key) : ret;
        }
    }
}
