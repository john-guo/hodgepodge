using System;
using System.Collections.Generic;
using System.Text;

namespace CEbalance.Symbol
{
    public class Equation
    {
        private IList<Molecule> left;
        private IList<Molecule> right;

        public Equation(string equation)
        {
            left = new List<Molecule>();
            right = new List<Molecule>();

            fromString(equation);
        }

        public IList<Molecule> Left
        {
            get
            {
                return left;
            }
        }

        public IList<Molecule> Right
        {
            get
            {
                return right;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < left.Count; ++i)
            {
                sb.Append(left[i].ToString());
                if (i < left.Count - 1)
                   sb.Append(" + ");
            }
            sb.Append(" -> ");
            for (int i = 0; i < right.Count; ++i)
            {
                sb.Append(right[i].ToString());
                if (i < right.Count - 1)
                    sb.Append(" + ");
            }

            return sb.ToString();
        }

        private Ion[] getIons(string str)
        {
            int status = 0;
            string atom = String.Empty;
            string key = String.Empty;
            List<Ion> ions = new List<Ion>();
            int ikey;

            for (int i = 0; i < str.Length; ++i)
            {
                if (char.IsLetter(str[i]))
                {
                    if (char.IsUpper(str[i]))
                    {
                        if (status == 0)
                            status = 1;
                        else
                            status = 4;
                    }
                    else
                    {
                        status = 2;
                    }
                }
                else if (char.IsDigit(str[i]))
                {
                    status = 3;
                }
                else
                {
                    throw new Exception();
                }

                switch (status)
                {
                    case 1:
                        atom = String.Empty;
                        key = String.Empty;
                        goto case 2;
                    case 2:
                        atom += str[i];
                        break;
                    case 3:
                        key += str[i];
                        break;
                    case 4:
                        if (string.IsNullOrEmpty(key))
                        {
                            ikey = 1;
                        }
                        else
                        {
                            ikey = int.Parse(key);
                        }
                        ions.Add(new Ion(atom, ikey));
                        goto case 1;
                }
            }

            if (string.IsNullOrEmpty(key))
            {
                ikey = 1;
            }
            else
            {
                ikey = int.Parse(key);
            }
            ions.Add(new Ion(atom, ikey));

            return ions.ToArray();
        }

        private void fromString(string str)
        {
            bool isLeft = true;
            string key = String.Empty;
            string ceStr = String.Empty;
            Molecule molecule = null;
            int status = 0;

            for (int i = 0; i < str.Length; ++i)
            {
                if (!char.IsLetterOrDigit(str[i]))
                {
                    if (!String.IsNullOrEmpty(ceStr))
                    {
                        foreach (var ion in getIons(ceStr))
                        {
                            molecule.AddIons(new Ion[] { ion });
                        }
                        ceStr = string.Empty;
                    }

                    if (str[i] == '(')
                    {
                        status = 2;

                        ceStr = string.Empty;
                        while (str[++i] != ')')
                        {
                            ceStr += str[i];
                        }

                        key = string.Empty;
                        while (char.IsDigit(str[++i]))
                        {
                            key += str[i];
                        }
                        --i;

                        int ikey = 1;
                        int.TryParse(key, out ikey);

                        molecule.AddIons(getIons(ceStr), ikey);

                        ceStr = string.Empty;
                        continue;
                    }

                    if (molecule != null)
                    {
                        if (isLeft)
                            left.Add(molecule);
                        else
                            right.Add(molecule);
                    }

                    if (str[i] == '>' || str[i] == '=')
                    {
                        isLeft = false;
                    }

                    status = 0;
                }
                else
                {
                    if (status == 0)
                        molecule = new Molecule();

                    status = 1;
                    ceStr += str[i];
                }
            }

            if (!String.IsNullOrEmpty(ceStr))
            {
                foreach (var ion in getIons(ceStr))
                {
                    molecule.AddIons(new Ion[] { ion });
                }
                ceStr = string.Empty;

                if (isLeft)
                    left.Add(molecule);
                else
                    right.Add(molecule);
            }
        }
    }
}
