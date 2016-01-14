using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CEbalance.Math;
using System.Linq.Expressions;

namespace CEbalance
{
    public sealed class Calculate24
    {
        private const int CalculateCount = 4;
        private const int CalculateResult = 24;

        public enum OpFlag
        {
            None = -1,
            Plus = 0,
            Minus = 1,
            Mul = 2,
            Div = 3,
        }

        private static Dictionary<OpFlag, Func<Fraction, Fraction, Fraction>> _calculator =
            new Dictionary<OpFlag, Func<Fraction, Fraction, Fraction>>
            {
                { OpFlag.Plus, (x, y) => x + y },
                { OpFlag.Minus, (x, y) => x - y },
                { OpFlag.Mul, (x, y) => x * y },
                { OpFlag.Div, (x, y) => x / y }
            };
        private static Dictionary<OpFlag, string> _opName =
            new Dictionary<OpFlag, string>
            {
                { OpFlag.Plus, "+" },
                { OpFlag.Minus, "-" },
                { OpFlag.Mul, "*" },
                { OpFlag.Div, "/" },
            };
        private static IEnumerable<OpFlag>[] _ops;
        private static IEnumerable<int>[] _precedenceOp;

        private static Dictionary<OpFlag, Func<Expression, Expression, Expression>> _opExpression =
            new Dictionary<OpFlag, Func<Expression, Expression, Expression>>
            {
                { OpFlag.Plus, Expression.Add },
                { OpFlag.Minus, Expression.Subtract },
                { OpFlag.Mul, Expression.Multiply },
                { OpFlag.Div, Expression.Divide }
            };


        static Calculate24()
        {
            var cCount = _calculator.Count;

            _precedenceOp = Enumerable.Range(0, CalculateCount - 1)
                .Permutation(CalculateCount - 1)
                .ToArray();

            _ops =
                Enumerable.Range(0, (CalculateCount - 1) * cCount)
                .Combination(CalculateCount - 1)
                .Select(
                    ar => ar.Select(i => (OpFlag)(i % cCount))
                ).ToArray();
        }


        private int[] _numbers;
        private IEnumerable<int>[] _permNumbers;
        private IList<Tuple<int[], OpFlag[], int[]>> _result;

        private IList<Expression> _expressions;

        private IList<CalculateExpression> _arithmetics;

        private Calculate24(int[] numbers)
        {
            if (numbers.Length != CalculateCount)
                throw new Exception();

            _numbers = numbers;
            _permNumbers = Enumerable.Range(0, CalculateCount)
                .Permutation(CalculateCount)
                .ToArray();
        }

        public Calculate24(int a, int b, int c, int d)
            : this(new int[]{a, b, c, d })
        {

        }

        private void calculate2(bool calAll)
        { 
            List<int> effectlist = new List<int>();

            _expressions = new List<Expression>();
            Dictionary<int, Expression> tempExp = new Dictionary<int, Expression>();

            foreach (var numbers in _permNumbers)
            {
                var arrayNumber = numbers.ToArray();

                foreach (var ops in _ops)
                {
                    var arrayOp = ops.ToArray();

                    foreach (var precendences in _precedenceOp)
                    {
                        effectlist.Clear();
                        tempExp.Clear();
                        Expression e = null;
                        int pCount = CalculateCount - 1;
                        foreach (var p in precendences)
                        {
                            int left, right;
                            Expression e_left, e_right;

                            if (p > 0)
                                left = p - 1;
                            else
                                left = p;
                            if (p + 1 < pCount)
                                right = p + 1;
                            else
                                right = p;

                            if (tempExp.ContainsKey(left))
                            {
                                e_left = tempExp[left];
                                effectlist.Add(left);
                            }
                            else
                            {
                                e_left = Expression.Constant((Fraction)_numbers[arrayNumber[p]]);
                            }
                            if (tempExp.ContainsKey(right))
                            {
                                e_right = tempExp[right];
                                effectlist.Add(right);
                            }
                            else
                            {
                                e_right = Expression.Constant((Fraction)_numbers[arrayNumber[p + 1]]);
                            }
                            e = tempExp[p] = _opExpression[arrayOp[p]](e_left, e_right);

                            foreach (var ei in effectlist)
                            {
                                tempExp[ei] = e;
                            }
                        }

                        var n = Expression.Lambda<Func<Fraction>>(e).Compile()();
                        if (n.IsInteger && n == CalculateResult)
                        {
                            _expressions.Add(e);

                            if (!calAll)
                                return;
                        }
                    }
                }
            }
        }


        private void calculate(bool calAll)
        {
            _result = new List<Tuple<int[], OpFlag[], int[]>>();
            Fraction[] tempResult = new Fraction[CalculateCount - 1];
            List<int> effectlist = new List<int>();

            foreach (var numbers in _permNumbers)
            {
                var arrayNumber = numbers.ToArray();

                foreach (var ops in _ops)
                {
                    var arrayOp = ops.ToArray();

                    foreach (var precendences in _precedenceOp)
                    {
                        for (int i = 0; i < tempResult.Length; ++i)
                        {
                            tempResult[i] = Fraction.NaN;
                        }

                        effectlist.Clear();
                        var f = Fraction.NaN;

                        foreach (var p in precendences)
                        {
                            Fraction f1, f2;
                            int left, right;

                            if (p > 0)
                                left = p - 1;
                            else
                                left = p;
                            if (p + 1 < tempResult.Length)
                                right = p + 1;
                            else
                                right = p;

                            if (!tempResult[left].IsNaN)
                            {
                                f1 = tempResult[left];
                                effectlist.Add(left);
                            }
                            else
                                f1 = _numbers[arrayNumber[p]];

                            if (!tempResult[right].IsNaN)
                            {
                                f2 = tempResult[right];
                                effectlist.Add(right);
                            }
                            else
                                f2 = _numbers[arrayNumber[p + 1]];

                            f = tempResult[p] = _calculator[arrayOp[p]](f1, f2);

                            if (f.IsNaN)
                                break;

                            foreach (var ei in effectlist)
                            {
                                tempResult[ei] = f;
                            }
                        }

                        if (f.IsInteger && f == CalculateResult)
                        {
                            var equ = precendences.ToArray();
                            _result.Add(new Tuple<int[], OpFlag[], int[]>(
                                arrayNumber,
                                arrayOp,
                                equ));

                            //if (!_result.Any(l =>
                            //    l.Item1.SequenceEqual(arrayNumber)
                            //    && l.Item2.SequenceEqual(arrayOp)
                            //    && l.Item3.SequenceEqual(equ)))
                            //{
                            //    _result.Add(new Tuple<int[], OpFlag[], int[]>(
                            //        arrayNumber,
                            //        arrayOp,
                            //        equ));
                            //}

                            if (!calAll)
                                return;
                        }

                    }
                }
            }
        }

        private void calculate3(bool calAll)
        {
            List<int> effectlist = new List<int>();

            _arithmetics = new List<CalculateExpression>();
            Dictionary<int, CalculateExpression> tempExp = new Dictionary<int, CalculateExpression>();

            foreach (var numbers in _permNumbers)
            {
                var arrayNumber = numbers.ToArray();
                foreach (var ops in _ops)
                {
                    var arrayOp = ops.ToArray();

                    foreach (var precendences in _precedenceOp)
                    {
                        effectlist.Clear();
                        tempExp.Clear();
                        CalculateExpression e = null;
                        int pCount = CalculateCount - 1;
                        foreach (var p in precendences)
                        {
                            int left, right;
                            CalculateExpression ce_left = null, ce_right = null;

                            if (p > 0)
                                left = p - 1;
                            else
                                left = p;
                            if (p + 1 < pCount)
                                right = p + 1;
                            else
                                right = p;

                            if (tempExp.ContainsKey(left))
                            {
                                ce_left = tempExp[left];
                                effectlist.Add(left);
                            }
                            else
                            {
                                ce_left = CalculateExpression.Constant(_numbers[arrayNumber[p]]);
                            }
                            if (tempExp.ContainsKey(right))
                            {
                                ce_right = tempExp[right];
                                effectlist.Add(right);
                            }
                            else
                            {
                                ce_right = CalculateExpression.Constant(_numbers[arrayNumber[p + 1]]);
                            }

                            e = tempExp[p] = CalculateExpression.Calculate(ce_left, arrayOp[p], ce_right);

                            foreach (var ei in effectlist)
                            {
                                tempExp[ei] = e;
                            }
                        }

                        var n = e.Eval();

                        if (n.IsInteger && n == CalculateResult)
                        {
                            _arithmetics.Add(e);

                            if (!calAll)
                                return;
                        }
                    }
                }
            }
        }

        abstract class CalculateExpression
        {
            public const int childLeft = 1;
            public const int childRight = 2;

            public OpFlag op = OpFlag.None;
            public Fraction numLeft = 0;
            public Fraction numRight = 0;
            public int index = 0;
            public int childPos = 0;

            public CalculateExpression Parent = null;
            public CalculateExpression Left = null;
            public CalculateExpression Right = null;

            public abstract override string ToString();
            public abstract Fraction Eval();

            public static ArithmeticExpression Calculate(CalculateExpression left, OpFlag op, CalculateExpression right)
            {
                var e = new ArithmeticExpression();
                e.op = op;
                e.Left = left;
                e.Right = right;

                left.Parent = e;
                left.childPos = childLeft;
                right.Parent = e;
                right.childPos = childRight;

                return e;
            }

            public static FractionExpression Constant(Fraction f)
            {
                var e = new FractionExpression();
                e.constant = f;
                return e;
            }
        }

        class FractionExpression : CalculateExpression
        {
            public Fraction constant = Fraction.NaN;

            public override Fraction Eval()
            {
                return constant;
            }

            public override string ToString()
            {
                return constant.ToString();
            }
        }

        class ArithmeticExpression : CalculateExpression
        {
            public static Dictionary<OpFlag, int> opPrecendence = new Dictionary<OpFlag, int>
            {
                { OpFlag.Plus, 0 },
                { OpFlag.Minus, 0 },
                { OpFlag.Mul, 1 },
                { OpFlag.Div, 1 },
            };

            public static HashSet<OpFlag> opSpecial = new HashSet<OpFlag>
            {
                OpFlag.Minus, OpFlag.Div
            };

            public override Fraction Eval()
            {
                return _calculator[op](Left.Eval(), Right.Eval());
            }

            public override string ToString()
            {
                string outputFormat = "{0}";

                string leftString, rightString;
                if (Parent != null 
                    && (opPrecendence[Parent.op] > opPrecendence[op]
                        || (childPos == childRight
                            && opSpecial.Contains(Parent.op)))
                   )
                {
                    outputFormat = "({0})";
                }

                if (Left != null)
                    leftString = Left.ToString();
                else
                    leftString = numLeft.ToString();

                if (Right != null)
                    rightString = Right.ToString();
                else
                    rightString = numRight.ToString();

                return string.Format(outputFormat, 
                    string.Format("{0}{1}{2}", 
                    leftString, 
                    _opName[op], 
                    rightString));
            }
        }

        public IList<string> RunFast(bool calAll = true)
        {
            var result = new List<string>();
            calculate(calAll);

            foreach (var r in _result)
            {
                ArithmeticExpression root = null;
                for (int i = 0; i < r.Item3.Length; ++i)
                {
                    ArithmeticExpression n = new ArithmeticExpression();
                    n.index = r.Item3[i];
                    n.op = r.Item2[n.index];
                    n.numLeft = _numbers[r.Item1[n.index]];
                    n.numRight = _numbers[r.Item1[n.index + 1]];

                    if (root == null)
                    {
                        root = n;
                    }
                    else
                    {
                        if (n.index > root.index)
                        {
                            n.Left = root;
                            root.childPos = CalculateExpression.childLeft;
                        }
                        else
                        {
                            n.Right = root;
                            root.childPos = CalculateExpression.childRight;
                        }

                        root.Parent = n;
                        root = n;
                    }
                }

                if (root == null)
                    continue;

                if (root.index == 1)
                {
                    bool adjLeft = true;
                    if (root.Left == null)
                        adjLeft = false;

                    if (adjLeft)
                    {
                        var node = root.Left.Left == null ? root.Left.Right : root.Left.Left;
                        node.childPos = CalculateExpression.childRight;
                        root.Right = node;
                        root.Left.Left = root.Left.Right = null;
                    }
                    else
                    {
                        var node = root.Right.Left == null ? root.Right.Right : root.Right.Left;
                        node.childPos = CalculateExpression.childLeft;
                        root.Left = node;
                        root.Right.Left = root.Right.Right = null;
                    }
                }

                var exp = root.ToString();
                if (!result.Contains(exp))
                    result.Add(exp);
            }

            return result;
        }

        public IList<string> RunSlow(bool calAll = true)
        {
            var result = new List<string>();
            calculate2(calAll);

            foreach (var e in _expressions)
            {
                var exp = e.ToString();
                if (!result.Contains(exp))
                    result.Add(exp);
            }

            return result;
        }

        public IList<string> Run(bool calAll = true)
        {
            var result = new List<string>();
            calculate3(calAll);

            foreach (var e in _arithmetics)
            {
                var exp = e.ToString();
                if (!result.Contains(exp))
                    result.Add(exp);
            }

            return result;
        }

        public bool Passed
        {
            get
            {
                if (_expressions != null && _expressions.Count > 0)
                    return true;

                if (_arithmetics != null && _arithmetics.Count > 0)
                    return true;

                return _result == null ? false : _result.Count > 0;
            }
        }

        //public IList<Tuple<int[], OpFlag[], int[]>> Result
        //{
        //    get { return _result; }
        //}
    }
}
