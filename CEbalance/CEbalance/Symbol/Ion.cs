using System;
using System.Collections.Generic;
using System.Text;

namespace CEbalance.Symbol
{
    public class Ion : Atom
    {
        public int Key { get; private set; }

        public Ion(string name, int key = 1): base(name)
        {
            Key = key;
        }

        public override string ToString()
        {
            if (Key > 1)
                return String.Format("{0}{1}", base.ToString(), Key);
            else
                return base.ToString();
        }
    }
}
