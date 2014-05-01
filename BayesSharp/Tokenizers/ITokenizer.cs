using System.Collections.Generic;

namespace BayesSharp.Tokenizers
{
    /// <summary>
    /// Break a string in a serie of string tokens
    /// </summary>
    public interface ITokenizer<out TTokenType>
    {
        /// <param name="input">String to be broken</param>
        IEnumerable<TTokenType> Tokenize(object input);
    }
}