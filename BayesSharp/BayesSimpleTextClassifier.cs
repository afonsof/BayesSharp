using BayesSharp.Combiners;
using BayesSharp.Tokenizers;

namespace BayesSharp
{
    public class BayesSimpleTextClassifier : BayesClassifier<string, string>
    {
        public BayesSimpleTextClassifier()
            : base(new SimpleTextTokenizer(true, null))
        {
        }

        public BayesSimpleTextClassifier(ITokenizer<string> tokenizer)
            : base(tokenizer)
        {
        }

        public BayesSimpleTextClassifier(ITokenizer<string> tokenizer, ICombiner combiner)
            : base(tokenizer, combiner)
        {
        }
    }
}