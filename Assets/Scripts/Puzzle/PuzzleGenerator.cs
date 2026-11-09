using System.Collections.Generic;
using UnityEngine;

// One multiple-choice question: the text, four options, and which option is correct.
public struct PuzzleQuestion
{
    public string question;
    public string[] options;
    public int correctIndex;
}

// A fraction kept in lowest terms, so answers are easy to compare as text.
public struct Fraction
{
    public int num;
    public int den;

    public Fraction(int n, int d)
    {
        int g = Gcd(Mathf.Abs(n), Mathf.Abs(d));
        if (g == 0) g = 1;
        num = n / g;
        den = d / g;
    }

    static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    public override string ToString() => den == 1 ? num.ToString() : num + "/" + den;
}

// Makes fraction questions. Difficulty 1 = add, 2 = multiply, 3 = divide.
public static class PuzzleGenerator
{
    public static PuzzleQuestion Generate(int difficulty)
    {
        Fraction answer;
        string text;

        switch (difficulty)
        {
            case 1:
                {
                    int den = Random.Range(3, 10);
                    int a = Random.Range(1, den);
                    int c = Random.Range(1, den);
                    answer = new Fraction(a + c, den);
                    text = a + "/" + den + " + " + c + "/" + den + " = ?";
                    break;
                }
            case 2:
                {
                    int a = Random.Range(1, 6), b = Random.Range(2, 9);
                    int c = Random.Range(1, 6), d = Random.Range(2, 9);
                    answer = new Fraction(a * c, b * d);
                    text = a + "/" + b + " x " + c + "/" + d + " = ?";
                    break;
                }
            default:
                {
                    int a = Random.Range(1, 6), b = Random.Range(2, 9);
                    int c = Random.Range(1, 6), d = Random.Range(2, 9);
                    answer = new Fraction(a * d, b * c);
                    text = a + "/" + b + " divided by " + c + "/" + d + " = ?";
                    break;
                }
        }

        // Build three wrong answers close to the right one.
        var options = new List<string> { answer.ToString() };
        int guard = 0;
        while (options.Count < 4 && guard++ < 200)
        {
            int n = answer.num + Random.Range(-3, 4);
            int d = answer.den + Random.Range(-2, 3);
            if (n <= 0 || d <= 0) continue;

            string s = new Fraction(n, d).ToString();
            if (!options.Contains(s)) options.Add(s);
        }
        while (options.Count < 4) options.Add("0");

        // Put the correct answer in a random position.
        int correctIndex = Random.Range(0, 4);
        (options[0], options[correctIndex]) = (options[correctIndex], options[0]);

        return new PuzzleQuestion
        {
            question = text,
            options = options.ToArray(),
            correctIndex = correctIndex
        };
    }
}
