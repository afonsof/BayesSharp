using System.Collections.Generic;

namespace BayesSharp
{
    public class TagData<TTokenType>
    {
        public int TrainCount { get; set; }
        public int TokenCount { get; set; }
        public Dictionary<TTokenType, double> Items { get; private set; }

        public TagData()
        {
            Items = new Dictionary<TTokenType, double>();
            TrainCount = 0;
        }

        public double Get(TTokenType token, double defaultValue)
        {
            if (Items.ContainsKey(token))
            {
                return Items[token];
            }
            return defaultValue;
        }
    }
}