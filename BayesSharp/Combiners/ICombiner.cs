using System.Collections.Generic;

namespace BayesSharp.Combiners
{
    public interface ICombiner
    {
        /// <summary>
        /// Combine a list of numbers
        /// </summary>
        /// <param name="numbers">List of numbers to be combined</param>
        double Combine(IEnumerable<double> numbers);
    }
}