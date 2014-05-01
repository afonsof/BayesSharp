using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BayesSharp.Combiners;
using BayesSharp.Tokenizers;
using NUnit.Framework;

namespace BayesSharp.UnitTests
{
    [TestFixture]
    public class BasicTests
    {
        [Test]
        public void TestSpanHam()
        {
            var t = new BayesSimpleTextClassifier();
            t.Train("span", "bad");
            t.Train("ham", "good");

            var res = t.Classify("this is a bad sentence");
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(0.9999, res["span"]);
        }

        [Test]
        public void TestLanguageDiscover()
        {
            var t = new BayesSimpleTextClassifier();
            t.Train("french", "le la les du un une je il elle de en");
            t.Train("german", "der die das ein eine");
            t.Train("spanish", "el uno una las de la en");
            t.Train("english", "the it she he they them are were to");
            t.Train("english", "the rain in spain falls mainly on the plain");
            var res = t.Classify("uno das je de la elle in");

            Assert.AreEqual(4, res.Count);
            Assert.AreEqual(0.9999, res["english"]);
            Assert.AreEqual(0.9999, res["german"]);
            Assert.AreEqual(0.67285006523593538, res["french"]);
            Assert.AreEqual(0.58077905232271598d, res["spanish"]);
        }

        [Test]
        public void TestNewTag()
        {
            var t = new BayesSimpleTextClassifier();
            t.AddTag("teste");
            Assert.IsNotNull(t.GetTagById("teste"));
        }

        [Test]
        public void TestRemoveTag()
        {
            var t = new BayesSimpleTextClassifier();
            t.Train("teste", "Bla");
            Assert.IsNotNull(t.GetTagById("teste"));
            t.RemoveTag("teste");
            Assert.IsNull(t.GetTagById("teste"));
        }

        [Test]
        public void TestChangeTag()
        {
            var t = new BayesSimpleTextClassifier();
            t.Train("teste", "Bla");
            Assert.IsNull(t.GetTagById("teste2"));
            t.ChangeTagId("teste", "teste2");
            Assert.IsNull(t.GetTagById("teste"));
            Assert.IsNotNull(t.GetTagById("teste2"));
        }

        [Test]
        public void TestMergeTags()
        {
            var t = new BayesSimpleTextClassifier();
            t.Train("bom", "gordo");
            t.Train("mal", "magro");
            var output = t.Classify("gordo magro");

            Assert.AreEqual(2, output.Count);
            Assert.AreEqual(0.9999, output["bom"]);
            Assert.AreEqual(0.9999, output["mal"]);

            t.MergeTags("mal", "bom");
            output = t.Classify("gordo magro");

            Assert.AreEqual(1, output.Count);
            Assert.AreEqual(0.9999, output["bom"]);
        }

#if !MONO
        [Test]
        public void TestSaveAndLoad()
        {
            var path = new FileInfo(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath).Directory.FullName + @"\bayes.json";
            var t = new BayesSimpleTextClassifier();
            t.Train("teste", "Afonso França");
            t.Save(path);
            var output = t.Classify("Afonso França");
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual(0.9999, output["teste"]);

            var t1 = new BayesSimpleTextClassifier();
            t1.Load(path);
            output = t1.Classify("Afonso França");

            Assert.AreEqual(1, output.Count);
            Assert.AreEqual(0.9999, output["teste"]);
        }
#endif

        [Test]
        public void TestUntrain()
        {
            var t = new BayesSimpleTextClassifier();
            t.Train("teste", "Afonso França");
            t.Untrain("teste", "França");

            var res = t.Classify("França");
            Assert.AreEqual(0, res.Count);
        }

        [Test]
        public void TestTagIds()
        {
            var t = new BayesSimpleTextClassifier();
            t.Train("teste", "Afonso França");
            t.Train("teste1", "Afonso França");

            var res = t.TagIds().ToList();
            Assert.AreEqual(2, res.Count());
            Assert.AreEqual("teste", res[0]);
            Assert.AreEqual("teste1", res[1]);
        }

        [Test]
        public void TestRobinsonFisherCombiner()
        {
            var t = new BayesSimpleTextClassifier();
            t.Train("Alimentação", "Ipiranga AMPM");
            t.Train("Alimentação", "Restaurante Bobs");
            t.Train("Combustível", "Posto Ipiranga");

            var res = t.Classify("Restaurante Ipiranga");
            Assert.AreEqual(2, res.Count());
            Assert.AreEqual(0.84415961583962162, res["Alimentação"]);
            Assert.AreEqual(0.33333333333333326, res["Combustível"]);


            t = new BayesSimpleTextClassifier(new SimpleTextTokenizer(), new RobinsonFisherCombiner());
            t.Train("Alimentação", "IPIRANGA AMPM");
            t.Train("Alimentação", "Restaurante Bobs");
            t.Train("Combustível", "Posto Ipiranga");

            res = t.Classify("Restaurante Ipiranga");
            Assert.AreEqual(2, res.Count());
            Assert.AreEqual(0.99481185089082513, res["Alimentação"]);
            Assert.AreEqual(0.38128034540863015, res["Combustível"]);
        }

        [Test]
        public void TestCatsAndDogs()
        {
            var ignoreList = new List<string> {"the", "my", "i", "dont"};
            var cls = new BayesSimpleTextClassifier(new SimpleTextTokenizer(true, ignoreList));
            cls.Train("dog", "Dogs are awesome, cats too. I love my dog");
            cls.Train("cat", "Cats are more preferred by software developers. I never could stand cats. I have a dog");
            cls.Train("dog", "My dog's name is Willy. He likes to play with my wife's cat all day long. I love dogs");
            cls.Train("cat", "Cats are difficult animals, unlike dogs, really annoying, I hate them all");
            cls.Train("dog", "So which one should you choose? A dog, definitely.");
            cls.Train("cat", "The favorite food for cats is bird meat, although mice are good, but birds are a delicacy");
            cls.Train("dog", "A dog will eat anything, including birds or whatever meat");
            cls.Train("cat", "My cat's favorite place to purr is on my keyboard");
            cls.Train("dog", "My dog's favorite place to take a leak is the tree in front of our house");

            Assert.AreEqual("cat", cls.Classify("This test is about cats.").First().Key);
            Assert.AreEqual("cat", cls.Classify("I hate ...").First().Key);
            Assert.AreEqual("cat", cls.Classify("The most annoying animal on earth.").First().Key);
            Assert.AreEqual("cat", cls.Classify("My precious, my favorite!").First().Key);
            Assert.AreEqual("cat", cls.Classify("Get off my keyboard!").First().Key);
            Assert.AreEqual("cat", cls.Classify("Kill that bird!").First().Key);
            Assert.AreEqual("dog", cls.Classify("This test is about dogs.").First().Key);
            Assert.AreEqual("dog",cls.Classify("Cats or Dogs?").First().Key);
            Assert.AreEqual("dog",cls.Classify("What pet will I love more?").First().Key);  
            Assert.AreEqual("cat",cls.Classify("Willy, where the heck are you?").First().Key);
            Assert.AreEqual("dog",cls.Classify("Why is the front door of our house open?").First().Key);

            var res = cls.Classify("The preferred company of software developers.");
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(0.9999, res["cat"]);
            Assert.AreEqual(0.9999, res["dog"]);
        }
    }
}
