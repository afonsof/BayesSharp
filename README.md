[![Built with Grunt](https://cdn.gruntjs.com/builtwith.png)](http://gruntjs.com/)

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

#### 2. Train the classifier with some data
```c#
c.Train("spam", "Buy now some pills");
c.Train("ham", "Hello my friend, I miss you");
```

#### 3. Classify a new text
```c#
var result = c.Classify("Hi my friend, I want to buy a car");
foreach(var item in result)
{
    Console.WriteLine(item.Key + ":" + item.Value);
}
```


## Contributing

Please use the issue tracker and pull requests.

## License
Copyright (c) 2014 Afonso França
Licensed under the MIT license.
