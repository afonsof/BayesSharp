using System.Collections.Generic;

namespace BayesSharp
{
    public class TagDictionary<TTokenType, TTagType>
    {
        public TagDictionary()
        {
            Items = new Dictionary<TTagType, TagData<TTokenType>>();
        }
        public Dictionary<TTagType, TagData<TTokenType>> Items { get; private set; }
        public TagData<TTokenType> SystemTag { get; set; }
    }
}