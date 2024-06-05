using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ExampleClass
{
    public float numbers;
}
public class ExampleClass2
{
    List<ExampleClass> example = new List<ExampleClass>();
    ExampleClass newThing = new();
    ExampleClass anotherThing = new();
    ExampleClass anotherThing2 = new();
    List<float> example2 = new List<float>();
    public ExampleClass2()
    {
        example.Add(anotherThing);
        example.Add(anotherThing2);
        example.Add(newThing);
        example[0].numbers = 9;
        bool exampleBool = example.Any((example) => example == newThing);
        float otherExampleAverage = 0;
        float example2Average = example.Average(exa => exa.numbers);
        example.Where(ggol => ggol.numbers == 9).ToList();
        foreach (var e in example)
        {
            e.numbers = otherExampleAverage;
        }
    }
    
}