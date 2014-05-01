[![Built with Grunt](https://cdn.gruntjs.com/builtwith.png)](http://gruntjs.com/)

[![Build Status](https://travis-ci.org/afonsof/BayesSharp.svg)](https://travis-ci.org/afonsof/BayesSharp)

BayesSharp
==========

Simple Naive Bayes text classifier

## Basic Usage

#### 1. Setting Up

Create a Console Application and install BayesSharp using Nuget:
```
PM> Install-Package BayesSharp
```

#### 2. Instantiate the BayesSimpleTextClassifier class
```c#
 var c = new BayesSimpleTextClassifier();
```

#### 3. Train the classifier with some data
```c#
c.Train("spam", "Buy now some pills. You'll receive a free prize");
c.Train("not spam", "Hello my friend, I miss you. I want to meet you soon.");
```

#### 4. Classify a new text
```c#
var result = c.Classify("Hi my friend, I want to buy a gift for you");
foreach(var item in result)
{
    Console.WriteLine(item.Key + ": " + item.Value);
}
```
In this case, the output will be:
```
not spam :0,946523050631535
spam: 0,748693773755381
```


## Contributing

Please use the issue tracker and pull requests.

## License
Copyright (c) 2014 Afonso França
Licensed under the MIT license.
