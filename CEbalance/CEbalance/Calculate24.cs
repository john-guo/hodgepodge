using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CEbalance.Math;

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

        static Calculate24()
        {
            var cCount = _calculator.Count;

            _precedenceOp = Enumerable.Range(0, CalculateCount - 1)
                .Permutation(CalculateCount - 1).ToArray();

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

        private void calculate(bool calAll)
        {
            _result = new List<Tuple<int[], OpFlag[], int[]>>();
            Fraction[] tempResult = new Fraction[CalculateCount - 1];
            List<int> effectlist = new List<int>();

            foreach (var numbers in _permNumbers)
            {
                foreach (var ops in _ops)
                {
                    var arrayNumber = numbers.ToArray();
                    var arrayOp = ops.ToArray();

                    foreach (var precendences in _precedenceOp)
                    {
                        StringBuilder sb = new StringBuilder();

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

                            if (!_result.Any(l => 
                                l.Item1.SequenceEqual(arrayNumber)
                                && l.Item2.SequenceEqual(arrayOp)
                                && l.Item3.SequenceEqual(equ)))
                            {
                                _result.Add(new Tuple<int[], OpFlag[], int[]>(
                                    arrayNumber,
                                    arrayOp,
                                    equ));
                            }

                            if (!calAll)
                                return;
                        }

                    }
                }
            }
        }

        class OpNode
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

            public const int childLeft = 1;
            public const int childRight = 2;

            public OpFlag op = OpFlag.None;
            public int num1 = 0;
            public int num2 = 0;
            public int index = 0;
            public int childPos = 0;

            public OpNode Parent = null;
            public OpNode Left = null;
            public OpNode Right = null;

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
                    leftString = num1.ToString();

                if (Right != null)
                    rightString = Right.ToString();
                else
                    rightString = num2.ToString();

                return string.Format(outputFormat, 
                    string.Format("{0}{1}{2}", 
                    leftString, 
                    _opName[op], 
                    rightString));
            }
        }

        public IList<string> Run(bool calAll = true)
        {
            var result = new List<string>();
            calculate(calAll);

            foreach (var r in _result)
            {
                OpNode root = null;
                for (int i = 0; i < r.Item3.Length; ++i)
                {
                    OpNode n = new OpNode();
                    n.index = r.Item3[i];
                    n.op = r.Item2[n.index];
                    n.num1 = _numbers[r.Item1[n.index]];
                    n.num2 = _numbers[r.Item1[n.index + 1]];

                    if (root == null)
                    {
                        root = n;
                    }
                    else
                    {
                        if (n.index > root.index)
                        {
                            n.Left = root;
                            root.childPos = OpNode.childLeft;
                        }
                        else
                        {
                            n.Right = root;
                            root.childPos = OpNode.childRight;
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
                        node.childPos = OpNode.childRight;
                        root.Right = node;
                        root.Left.Left = root.Left.Right = null;
                    }
                    else
                    {
                        var node = root.Right.Left == null ? root.Right.Right : root.Right.Left;
                        node.childPos = OpNode.childLeft;
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

        public bool Passed
        {
            get
            {
                return _result == null ? false : _result.Count > 0;
            }
        }

        public IList<Tuple<int[], OpFlag[], int[]>> Result
        {
            get { return _result; }
        }
    }
}
