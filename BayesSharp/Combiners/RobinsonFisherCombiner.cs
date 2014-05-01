using System;
using System.Collections.Generic;
using System.Linq;

namespace BayesSharp.Combiners
{
    public class RobinsonFisherCombiner : ICombiner
    {
        /// <summary>
        /// Computes the probability of a message being spam (Robinson-Fisher method)
        /// H = C-1( -2.ln(prod(p)), 2*n )
        /// S = C-1( -2.ln(prod(1-p)), 2*n )
        /// I = (1 + H - S) / 2
        /// </summary>
        /// <param name="numbers">List of numbers to be combined</param>
        public double Combine(IEnumerable<double> numbers)
        {
            var probList = numbers.ToList();
            var n = probList.Count();
            double h;
            double s;
            try
            {
                h = Chi2P(-2.0 * Math.Log(probList.Aggregate(1.0, (x, y) => x * y)), 2 * n);
            }
            catch (OverflowException)
            {
                h = 0.0;
            }
            try
            {
                s = Chi2P(-2.0 * Math.Log(probList.Aggregate(1.0, (x, y) => x * (1 - y))), 2 * n);
            }
            catch (OverflowException)
            {
                s = 0.0;
            }
            return (1 + h - s) / 2;
        }

        private static double Chi2P(double chi, double df)
        {
            var m = chi / 2.0;
            var term = Math.Exp(-m);
            var sum = term;
            for (var i = 1; i <= df / 2; i++)
            {
                term *= m / i;
                sum += term;
            }
            return Math.Min(sum, 1.0);
        }
    }
}