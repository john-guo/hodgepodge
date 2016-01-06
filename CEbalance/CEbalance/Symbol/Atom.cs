using System;
using System.Collections.Generic;
using System.Text;

namespace CEbalance.Symbol
{
    public class Atom
    {
        public string Name { get; private set; }
        public Atom(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
