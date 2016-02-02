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

        private static Fraction _nan = new Fraction();
        public static Fraction NaN
        {
            get
            {
                return _nan;
            }
        }

        private Fraction()
        {
            Numerator = 0;
            Denominator = 0;
        }

        public Fraction(int numerator, int denominator) : this()
        {
            if (denominator == 0)
                return;

            Numerator = numerator;
            Denominator = denominator;
        }

        public override string ToString()
        {
            if (IsNaN)
                return "NaN";

            if (Numerator == 0 || Denominator == 1)
                return Numerator.ToString();

            return string.Format("{0}/{1}", Numerator, Denominator);
        }

        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            if (f1.IsNaN || f2.IsNaN)
                return NaN;

            Fraction f = new Fraction(
                f1.Numerator * f2.Numerator,
                f1.Denominator * f2.Denominator);

            f.reduce();

            return f;
        }

        public static Fraction operator /(Fraction f1, Fraction f2)
        {
            if (f1.IsNaN || f2.IsNaN)
                return NaN;

            Fraction f = new Fraction(
                (f2.Numerator < 0 ? -1 : 1) * f1.Numerator * f2.Denominator,
                (f2.Numerator < 0 ? -1 : 1) * f1.Denominator * f2.Numerator);

            f.reduce();

            return f;
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            if (f1.IsNaN || f2.IsNaN)
                return NaN;

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

        public static explicit operator int (Fraction f)
        {
            if (f.IsNaN)
                throw new Exception();

            return f.Numerator / f.Denominator;
        }

        public override bool Equals(object obj)
        {
            return Equals((Fraction)obj);
        }

        public override int GetHashCode()
        {
            if (IsNaN)
                return 0;

            return (Numerator / Denominator) ^ (Numerator % Denominator);
        }

        public bool Equals(Fraction f)
        {
            if (IsNaN && f.IsNaN)
                return true;

            if (IsNaN || f.IsNaN)
                return false;

            if (Numerator == 0 && f.Numerator == 0)
                return true;

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

        public static bool operator >(Fraction f1, Fraction f2)
        {
            if (f1.IsNaN || f2.IsNaN)
                return false;

            var f = f1 - f2;

            return f.Numerator > 0;
        }

        public static bool operator >=(Fraction f1, Fraction f2)
        {
            if (f1.IsNaN || f2.IsNaN)
                return false;

            var f = f1 - f2;

            return f.Numerator >= 0;
        }

        public static bool operator <(Fraction f1, Fraction f2)
        {
            return !(f1 >= f2);
        }

        public static bool operator <=(Fraction f1, Fraction f2)
        {
            return !(f1 > f2);
        }

        public bool IsNaN
        {
            get { return Denominator == 0; }
        }

        public bool IsInteger
        {
            get
            {
                if (IsNaN)
                    return false;

                return Numerator % Denominator == 0;
            }
        }

        private void reduce()
        {
            if (IsNaN)
                return;

            if (Denominator == 1 || Numerator == 0)
                return;

            var gcd = Utils.GCD(Numerator, Denominator);
            Numerator /= gcd;
            Denominator /= gcd;
        }
    }
}
