using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CEbalance.Math
{
    public class Fraction
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; }

        public Fraction(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}", Numerator, Denominator);
        }

        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            Fraction f = new Fraction(
                f1.Numerator * f2.Numerator,
                f1.Denominator * f2.Denominator);

            f.reduce();

            return f;
        }

        public static Fraction operator /(Fraction f1, Fraction f2)
        {
            Fraction f = new Fraction(
                (f2.Numerator < 0 ? -1 : 1) * f1.Numerator * f2.Denominator,
                (f2.Numerator < 0 ? -1 : 1) * f1.Denominator * f2.Numerator);

            f.reduce();

            return f;
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            Fraction f = new Fraction(
                f1.Numerator * f2.Denominator + f2.Numerator * f1.Denominator,
                f1.Denominator * f2.Denominator
                );

            f.reduce();

            return f;
        }

        public static Fraction operator -(Fraction f)
        {
            return new Fraction(-f.Numerator, f.Denominator);
        }

        public static Fraction operator -(Fraction f1, Fraction f2)
        {
            return f1 + (-f2);
        }

        public static implicit operator Fraction(int num)
        {
            return new Fraction(num, 1);
        }

        public static implicit operator int (Fraction f)
        {
            return f.Numerator / f.Denominator;
        }

        public override bool Equals(object obj)
        {
            return Equals((Fraction)obj);
        }

        public override int GetHashCode()
        {
            return (Numerator / Denominator) ^ (Numerator % Denominator);
        }

        public bool Equals(Fraction f)
        {
            this.reduce();
            f.reduce();

            return Numerator == f.Numerator && Denominator == f.Denominator;
        }

        public static bool operator ==(Fraction f1, Fraction f2)
        {
            return f1.Equals(f2);
        }

        public static bool operator !=(Fraction f1, Fraction f2)
        {
            return !(f1 == f2);
        }

        public bool IsInteger
        {
            get
            {
                return Numerator % Denominator == 0;
            }
        }

        private void reduce()
        {
            if (Denominator == 1)
                return;
            var gcd = Utils.GCD(Numerator, Denominator);
            Numerator /= gcd;
            Denominator /= gcd;
        }
    }
}
