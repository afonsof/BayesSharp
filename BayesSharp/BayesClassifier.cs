using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BayesSharp.Combiners;
using BayesSharp.Tokenizers;
using Newtonsoft.Json;

namespace BayesSharp
{
    public class BayesClassifier<TTokenType, TTagType> where TTagType : IComparable
    {
        private TagDictionary<TTokenType, TTagType> _tags = new TagDictionary<TTokenType, TTagType>();
        private TagDictionary<TTokenType, TTagType> _cache;

        private readonly ITokenizer<TTokenType> _tokenizer;
        private readonly ICombiner _combiner;

        private bool _mustRecache;
        private const double Tolerance = 0.0001;
        private const double Threshold = 0.1;

        public BayesClassifier(ITokenizer<TTokenType> tokenizer)
            : this(tokenizer, new RobinsonCombiner())
        {
        }

        public BayesClassifier(ITokenizer<TTokenType> tokenizer, ICombiner combiner)
        {
            if (tokenizer == null) throw new ArgumentNullException("tokenizer");
            if (combiner == null) throw new ArgumentNullException("combiner");

            _tokenizer = tokenizer;
            _combiner = combiner;

            _tags.SystemTag = new TagData<TTokenType>();
            _mustRecache = true;
        }

        /// <summary>
        /// Create a new tag, without actually doing any training.
        /// </summary>
        /// <param name="tagId">Tag Id</param>
        public void AddTag(TTagType tagId)
        {
            GetAndAddIfNotFound(_tags.Items, tagId);
            _mustRecache = true;
        }

        /// <summary>
        /// Remove a tag
        /// </summary>
        /// <param name="tagId">Tag Id</param>
        public void RemoveTag(TTagType tagId)
        {
            _tags.Items.Remove(tagId);
            _mustRecache = true;
        }

        /// <summary>
        /// Change the Id of a tag
        /// </summary>
        /// <param name="oldTagId">Old Tag Id</param>
        /// <param name="newTagId">New Tag Id</param>
        public void ChangeTagId(TTagType oldTagId, TTagType newTagId)
        {
            _tags.Items[newTagId] = _tags.Items[oldTagId];
            RemoveTag(oldTagId);
            _mustRecache = true;
        }

        /// <summary>
        /// Merge an existing tag into another
        /// </summary>
        /// <param name="sourceTagId">Tag to merged to destTagId and removed</param>
        /// <param name="destTagId">Destination tag Id</param>
        public void MergeTags(TTagType sourceTagId, TTagType destTagId)
        {
            var sourceTag = _tags.Items[sourceTagId];
            var destTag = _tags.Items[destTagId];
            var count = 0;
            foreach (var tagItem in sourceTag.Items)
            {
                count++;
                var tok = tagItem;
                if (destTag.Items.ContainsKey(tok.Key))
                {
                    destTag.Items[tok.Key] += count;
                }
                else
                {
                    destTag.Items[tok.Key] = count;
                    destTag.TokenCount += 1;
                }
            }
            RemoveTag(sourceTagId);
            _mustRecache = true;
        }

        /// <summary>
        /// Return a TagData object of a Tag Id informed
        /// </summary>
        /// <param name="tagId">Tag Id</param>
        public TagData<TTokenType> GetTagById(TTagType tagId)
        {
            return _tags.Items.ContainsKey(tagId) ? _tags.Items[tagId] : null;
        }

        /// <summary>
        /// Save Bayes Text Classifier into a file
        /// </summary>
        /// <param name="path">The file to write to</param>
        public void Save(string path)
        {
            using (var streamWriter = new StreamWriter(path, false, Encoding.UTF8))
            {
                JsonSerializer.Create().Serialize(streamWriter, _tags);
            }
        }

        /// <summary>
        /// Load Bayes Text Classifier from a file
        /// </summary>
        /// <param name="path">The file to open for reading</param>
        public void Load(string path)
        {
            using (var streamReader = new StreamReader(path, Encoding.UTF8))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    _tags = JsonSerializer.Create().Deserialize<TagDictionary<TTokenType, TTagType>>(jsonTextReader);
                }
            }
            _mustRecache = true;
        }

        /// <summary>
        /// Import Bayes Text Classifier from a json string
        /// </summary>
        /// <param name="json">The json content to be loaded</param>
        public void ImportJsonData(string json)
        {
            _tags = JsonConvert.DeserializeObject<TagDictionary<TTokenType, TTagType>>(json);
            _mustRecache = true;
        }

        /// <summary>
        /// Export Bayes Text Classifier to a json string
        /// </summary>
        public string ExportJsonData()
        {
            return JsonConvert.SerializeObject(_tags);
        }

        /// <summary>
        /// Return a sorted list of Tag Ids
        /// </summary>
        public IEnumerable<TTagType> TagIds()
        {
            return _tags.Items.Keys.OrderBy(p => p);
        }

        /// <summary>
        /// Train Bayes by telling him that input belongs in tag.
        /// </summary>
        /// <param name="tagId">Tag Id</param>
        /// <param name="input">Input to be trained</param>
        public void Train(TTagType tagId, string input)
        {
            var tokens = _tokenizer.Tokenize(input);
            var tag = GetAndAddIfNotFound(_tags.Items, tagId);
            _train(tag, tokens);
            _tags.SystemTag.TrainCount += 1;
            tag.TrainCount += 1;
            _mustRecache = true;
        }

        /// <summary>
        /// Untrain Bayes by telling him that input no more belongs in tag.
        /// </summary>
        /// <param name="tagId">Tag Id</param>
        /// <param name="input">Input to be untrained</param>
        public void Untrain(TTagType tagId, string input)
        {
            var tokens = _tokenizer.Tokenize(input);
            var tag = _tags.Items[tagId];
            if (tag == null)
            {
                return;
            }
            _untrain(tag, tokens);
            _tags.SystemTag.TrainCount += 1;
            tag.TrainCount += 1;
            _mustRecache = true;
        }

        /// <summary>
        /// Returns the scores in each tag the provided input
        /// </summary>
        /// <param name="input">Input to be classified</param>
        public Dictionary<TTagType, double> Classify(string input)
        {
            var tokens = _tokenizer.Tokenize(input).ToList();
            var tags = CreateCacheAnsGetTags();

            var stats = new Dictionary<TTagType, double>();

            foreach (var tag in tags.Items)
            {
                var probs = GetProbabilities(tag.Value, tokens).ToList();
                if (probs.Count() != 0)
                {
                    stats[tag.Key] = _combiner.Combine(probs);
                }
            }
            return stats.OrderByDescending(s => s.Value).ToDictionary(s => s.Key, pair => pair.Value);
        }

        #region Private Methods

        private void _train(TagData<TTokenType> tag, IEnumerable<TTokenType> tokens)
        {
            var tokenCount = 0;
            foreach (var token in tokens)
            {
                var count = tag.Get(token, 0);
                tag.Items[token] = count + 1;
                count = _tags.SystemTag.Get(token, 0);
                _tags.SystemTag.Items[token] = count + 1;
                tokenCount += 1;
            }
            tag.TokenCount += tokenCount;
            _tags.SystemTag.TokenCount += tokenCount;
        }

        private void _untrain(TagData<TTokenType> tag, IEnumerable<TTokenType> tokens)
        {
            foreach (var token in tokens)
            {
                var count = tag.Get(token, 0);
                if (count > 0)
                {
                    if (Math.Abs(count - 1) < Tolerance)
                    {
                        tag.Items.Remove(token);
                    }
                    else
                    {
                        tag.Items[token] = count - 1;
                    }
                    tag.TokenCount -= 1;
                }
                count = _tags.SystemTag.Get(token, 0);
                if (count > 0)
                {
                    if (Math.Abs(count - 1) < Tolerance)
                    {
                        _tags.SystemTag.Items.Remove(token);
                    }
                    else
                    {
                        _tags.SystemTag.Items[token] = count - 1;
                    }
                    _tags.SystemTag.TokenCount -= 1;
                }
            }
        }

        private static TagData<TTokenType> GetAndAddIfNotFound(IDictionary<TTagType, TagData<TTokenType>> dic, TTagType key)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            dic[key] = new TagData<TTokenType>();
            return dic[key];
        }

        private TagDictionary<TTokenType, TTagType> CreateCacheAnsGetTags()
        {
            if (!_mustRecache) return _cache;

            _cache = new TagDictionary<TTokenType, TTagType> { SystemTag = _tags.SystemTag };
            foreach (var tag in _tags.Items)
            {
                var thisTagTokenCount = tag.Value.TokenCount;
                var otherTagsTokenCount = Math.Max(_tags.SystemTag.TokenCount - thisTagTokenCount, 1);
                var cachedTag = GetAndAddIfNotFound(_cache.Items, tag.Key);

                foreach (var systemTagItem in _tags.SystemTag.Items)
                {
                    var thisTagTokenFreq = tag.Value.Get(systemTagItem.Key, 0.0);
                    if (Math.Abs(thisTagTokenFreq) < Tolerance)
                    {
                        continue;
                    }
                    var otherTagsTokenFreq = systemTagItem.Value - thisTagTokenFreq;

                    var goodMetric = thisTagTokenCount == 0 ? 1.0 : Math.Min(1.0, otherTagsTokenFreq / thisTagTokenCount);
                    var badMetric = Math.Min(1.0, thisTagTokenFreq / otherTagsTokenCount);
                    var f = badMetric / (goodMetric + badMetric);

                    if (Math.Abs(f - 0.5) >= Threshold)
                    {
                        cachedTag.Items[systemTagItem.Key] = Math.Max(Tolerance, Math.Min(1 - Tolerance, f));
                    }
                }
            }
            _mustRecache = false;
            return _cache;
        }

        private static IEnumerable<double> GetProbabilities(TagData<TTokenType> tag, IEnumerable<TTokenType> tokens)
        {
            var probs = tokens.Where(tag.Items.ContainsKey).Select(t => tag.Items[t]);
            return probs.OrderByDescending(p => p).Take(2048);
        }

        #endregion

    }
}