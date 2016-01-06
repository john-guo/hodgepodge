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

        private void fromString(string str)
        {
            string atom = String.Empty;
            string key = String.Empty;
            int status = 0;
            bool isLeft = true;
            Ion ion;
            Molecule molecule;
            List<Ion> ions = new List<Ion>();

            for (int i = 0; i < str.Length; ++i)
            {
                if (!char.IsLetterOrDigit(str[i]))
                {
                    if (str[i] == '>')
                    {
                        isLeft = false;
                    }

                    if (status == 0)
                        continue;

                    status = 5;
                }
                else if (char.IsLetter(str[i]))
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
                            ion = new Ion(atom);
                        }
                        else
                        {
                            ion = new Ion(atom, int.Parse(key));
                        }
                        ions.Add(ion);
                        goto case 1;
                    case 5:
                        if (string.IsNullOrEmpty(key))
                        {
                            ion = new Ion(atom);
                        }
                        else
                        {
                            ion = new Ion(atom, int.Parse(key));
                        }
                        ions.Add(ion);

                        molecule = new Molecule(ions.ToArray());
                        if (isLeft)
                            left.Add(molecule);
                        else
                            right.Add(molecule);

                        ions.Clear();
                        status = 0;
                        break;
                }
            }

            if (status == 0)
                return;

            if (string.IsNullOrEmpty(key))
            {
                ion = new Ion(atom);
            }
            else
            {
                ion = new Ion(atom, int.Parse(key));
            }
            ions.Add(ion);

            molecule = new Molecule(ions.ToArray());
            if (isLeft)
                left.Add(molecule);
            else
                right.Add(molecule);

            ions.Clear();
            status = 0;
        }
    }
}
