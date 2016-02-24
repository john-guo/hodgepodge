using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

namespace roslynTest
{
    public class Class1
    {

        static void Main()
        {
            var tree = CSharpSyntaxTree.ParseText(@"_(){
                () => 10;
                //i => ++i;
                //i => i;
                //()=> 3;
                //5+3;
                /*from method in type.GetMethods()
                        let parameters = from p in method.GetParameters() select p.ParameterType
                        where method.Name == name
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where parameters.SequenceEqual(arguments, new ParameterComparer())
                        select method;*/
            ");

            var p = new ExpressionParser();
            var a = p.Parse<Func<int,int,int>>("(i,j)=>i=1").Compile();
            var i = a(2,1);
            Console.ReadLine();
        }
    }
}
