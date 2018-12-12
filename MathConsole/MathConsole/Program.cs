using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace MathConsole
{
    internal struct Point
    {
        public double x;
        public double y;

        public static Point Zero = new Point() { x = 0.0, y = 0.0 };

        public bool Equals(Point p)
        {
            return x == p.x && y == p.y;
        }

        public override bool Equals(object obj)
        {
            return Equals((Point)obj);
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public override string ToString()
        {
            return $"{x}, {y}";
        }

        public override int GetHashCode()
        {
            return (int)Math.Floor(Math.Cos(x) * Math.Sin(y));
        }
    }
    
    enum AngleType { Acute, Right, Obtuse, Straight, Perigon };

    struct Angle
    {
        public double radian;

        public double angle => radian * 180 / Math.PI;

        public AngleType type
        {
            get
            {
                var a = Math.IEEERemainder(Math.Abs(angle), 360);
                if (a > 0 && a < 90)
                    return AngleType.Acute;
                if (a == 90 || a == 270)
                    return AngleType.Right;
                if (a > 90 && a < 180)
                    return AngleType.Obtuse;
                if (a == 180)
                    return AngleType.Straight;
                if (a == 0 || a == 360)
                    return AngleType.Perigon;
                if (a > 180 && a < 270)
                    return AngleType.Obtuse;

                return AngleType.Acute;
            }
        }

        public override string ToString()
        {
            return $"{angle}";
        }
    }

    enum TriangleType { Acute, Right, Obtuse, Invalid }

    struct Triangle
    {
        public Point A;
        public Point B;
        public Point C;

        public Func<Point, Point, Point, Angle> func;

        public Angle a
        {
            get
            {
                return func(A, B, C);
            }
        }

        public Angle b
        {
            get
            {
                return func(B, A, C);
            }
        }

        public Angle c
        {
            get
            {
                return func(C, A, B);
            }
        }

        public TriangleType type
        {
            get
            {
                if (a.type == AngleType.Obtuse || b.type == AngleType.Obtuse || c.type == AngleType.Obtuse)
                    return TriangleType.Obtuse;

                if (a.type == AngleType.Right || b.type == AngleType.Right || c.type == AngleType.Right)
                    return TriangleType.Right;

                if (a.type == AngleType.Straight || b.type == AngleType.Straight || c.type == AngleType.Straight)
                    return TriangleType.Invalid;

                if (a.type == AngleType.Perigon || b.type == AngleType.Perigon || c.type == AngleType.Perigon)
                    return TriangleType.Invalid;

                return TriangleType.Acute;
            }
        }

        public override string ToString()
        {
            return $"({A}) ({B}) ({C}) ({a}, {b}, {c}) {type}";
        }
    }

    class Program
    {
        static double dist(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
        }

        static Angle GetAngleByCos(Point p, Point p1, Point p2)
        {
            var a = dist(p, p1);
            var b = dist(p, p2);
            var c = dist(p1, p2);

            //2abcosc = a^2 + b^2 - c^2

            Angle angle = new Angle
            {
                radian = Math.Acos((Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2)) / (2 * a * b))
            };

            return angle;
        }

        struct Vector2D
        {
            public double v1;
            public double v2;

            public double dot(Vector2D v)
            {
                return v1 * v.v1 + v2 * v.v2;
            }

            public double module => Math.Sqrt(Math.Pow(v1, 2) + Math.Pow(v2, 2));
        }


        static Angle GetAngleByVector(Point p, Point p1, Point p2)
        {
            Vector2D
            v1 = new Vector2D
            {
                v1 = p1.x - p.x,
                v2 = p1.y - p.y,
            },
            v2 = new Vector2D
            {
                v1 = p2.x - p.x,
                v2 = p2.y - p.y,
            };

            Angle angle = new Angle
            {
                radian = Math.Acos(v1.dot(v2) / (v1.module * v2.module))
            };

            return angle;
        }
        
        static void Main(string[] args)
        {
            Triangle tc = new Triangle() { func = GetAngleByCos };
            Triangle tv = new Triangle() { func = GetAngleByVector };

            Point p1 = new Point() { x = 1, y = 1 };
            Point p2 = new Point() { x = 0, y = 0 };
            Point p3 = new Point() { x = 0, y = 2 };

            tv.A = tc.A = p1;
            tv.B = tc.B = p2;
            tv.C = tc.C = p3;

            var w = Stopwatch.StartNew();
            Console.WriteLine($"{tc}");
            Console.WriteLine(w.ElapsedTicks);

            w = Stopwatch.StartNew();
            Console.WriteLine($"{tv}");
            Console.WriteLine(w.ElapsedTicks);

            Console.ReadLine();

            Random r = new Random();

            Dictionary<TriangleType, int> stat = new Dictionary<TriangleType, int>()
            {
                { TriangleType.Acute, 0 },
                { TriangleType.Right, 0 },
                { TriangleType.Obtuse, 0 },
                { TriangleType.Invalid, 0 },
            };

            int num = 0;
            while (true)
            {
                tv.A.x = r.Next(Int32.MaxValue);
                tv.A.y = r.Next(Int32.MaxValue);
                tv.B.x = r.Next(Int32.MaxValue);
                tv.B.y = r.Next(Int32.MaxValue);
                tv.C.x = r.Next(Int32.MaxValue);
                tv.C.y = r.Next(Int32.MaxValue);
                stat[tv.type]++;

                //tc.A.x = r.Next(Int32.MaxValue) / 1000000d;
                //tc.A.y = r.Next(Int32.MaxValue) / 1000000d;
                //tc.B.x = r.Next(Int32.MaxValue) / 1000000d;
                //tc.B.y = r.Next(Int32.MaxValue) / 1000000d;
                //tc.C.x = r.Next(Int32.MaxValue) / 1000000d;
                //tc.C.y = r.Next(Int32.MaxValue) / 1000000d;
                //stat[tc.type]++;
                num++;

                Console.WriteLine($"total:{num}, acute:{stat[TriangleType.Acute]},right:{stat[TriangleType.Right]},obtuse:{stat[TriangleType.Obtuse]},invalid:{stat[TriangleType.Invalid]},p:{stat[TriangleType.Obtuse] * 1d / num}");
            }
        }
    }
}
