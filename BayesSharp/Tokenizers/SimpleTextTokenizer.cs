using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BayesSharp.Tokenizers
{
    /// <summary>
    /// A simple regex-based whitespace tokenizer.
    /// </summary>
    public class SimpleTextTokenizer : ITokenizer<string>
    {
        private readonly Regex _wordRe = new Regex(@"\w+");
        private readonly bool _convertToLower;
        private readonly List<string> _ignoreList;

        public SimpleTextTokenizer(): this(true, null)
        {
        }

        /// <param name="convertToLower">Tokens must be converted to lower case</param>
        /// <param name="ignoreList">Tokens that will be ignored</param>
        public SimpleTextTokenizer(bool convertToLower, List<string> ignoreList)
        {
            _ignoreList = ignoreList;
            _convertToLower = convertToLower;
        }

        /// <param name="input">String to be broken</param>
        public IEnumerable<string> Tokenize(object input)
        {
            if (input.GetType() != typeof (string))
            {
                throw new FormatException(string.Format("Expected string, given {0}", input.GetType()));
            }
            var tokens = MatchTokens(input);
            if (_ignoreList == null)
            {
                return tokens;
            }
            return tokens.Where(token => !_ignoreList.Contains(token));
        }

        private IEnumerable<string> MatchTokens(object input)
        {
            foreach (Match match in _wordRe.Matches((string) input))
            {
                if (_convertToLower)
                {
                    yield return match.Value.ToLower();
                }
                else
                {
                    yield return match.Value;
                }
            }
        }
    }
}