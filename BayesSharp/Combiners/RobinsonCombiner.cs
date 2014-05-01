using System;
using System.Collections.Generic;
using System.Linq;

namespace BayesSharp.Combiners
{
    public class RobinsonCombiner : ICombiner
    {
        /// <summary>
        /// Computes the probability of a message being spam (Robinson's method)
        /// P = 1 - prod(1-p)^(1/n)
        /// Q = 1 - prod(p)^(1/n)
        /// S = (1 + (P-Q)/(P+Q)) / 2
        /// </summary>
        /// <param name="numbers">List of numbers to be combined</param>
        public double Combine(IEnumerable<double> numbers)
        {
            var probList = numbers.ToList();
            var nth = 1.0 / probList.Count();
            var p = 1.0 - Math.Pow(probList.Aggregate(1.0, (x, y) => x * (1 - y)), nth);
            var q = 1.0 - Math.Pow(probList.Aggregate(1.0, (x, y) => x * y), nth);
            var s = (p - q) / (p + q);
            return (1 + s) / 2;
        }
    }
}
