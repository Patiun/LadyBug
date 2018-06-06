using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Distribution {

    private Dictionary<string, float> outputs;//Dictionary of symbols to probability

    /// <summary>
    /// Initializes the distribution using the static outputSymbols and epsilonProbabilityParameter from Model
    /// </summary>
    public Distribution()
    {
        outputs = new Dictionary<string, float>();
        outputs.Add(Symbol.EPSILON, Model.epsilonProbabilityParameter);
        float difference = 1f - Model.epsilonProbabilityParameter;
        if (difference > 0)
        {
            float probability = 0f;
            if (Model.outputSymbols.Contains(Symbol.EPSILON))
            {
                probability = difference / (Model.outputSymbols.Count-1);
            }
            else
            {
                probability = difference / Model.outputSymbols.Count;
            }
            foreach(String symbolName in Model.outputSymbols)
            {
                if (symbolName != Symbol.EPSILON)
                {
                    outputs.Add(symbolName, probability);
                }
            }
        } else
        {
            foreach (String symbolName in Model.outputSymbols)
            {
                if (symbolName != Symbol.EPSILON)
                {
                    outputs.Add(symbolName, 0.0f);
                }
            }
        }
    }

    public Distribution(List<string> Delta)
    {
        outputs = new Dictionary<string, float>();
        float chance = 1f / Delta.Count;
        for (int i = 0; i < Delta.Count; i++)
        {
            outputs.Add(Delta[i], chance);
        }
    }

    public void SetDistribution(List<Symbol> toBe)
    {
        outputs = new Dictionary<string, float>();
        foreach(Symbol pair in toBe)
        {
            outputs.Add(pair.GetName(), pair.GetStrength());
        }
    }

    /// <summary>
    /// Chooses a symbol to output based on each symbols output weight.
    /// </summary>
    /// <returns>string of a symbol randomyl chosen based on weights</returns>
    public string ChooseOutput()
    {
        string output = "";
        float target = UnityEngine.Random.Range(0.0f, 1.0f);
        float window = 0.0f;
        //Check if the random value is within a certain probability range based on the distribution of probilities across all possible outputs
        foreach(string symbol in outputs.Keys)
        {
            window += outputs[symbol];
            if (target <= window)
            {
                return symbol;
            }
        }
        return output;
    }

    /// <summary>
    /// Increases the probability of symbol and brings all symbols to the same scale out of 1
    /// </summary>
    /// <param name="symbol">String symbol to be increased</param>
    /// <param name="amount">Float amount to increase the symbol probability by</param>
    public void IncreaseSymbolProbability(string symbol,float amount)
    {
        outputs[symbol] = (outputs[symbol] + amount) / (1 + amount);
        foreach(string otherSymbol in outputs.Keys)
        {
            if (otherSymbol != symbol) {
                outputs[otherSymbol] = outputs[otherSymbol] / (1 + amount);
            }
        }
    }

    /// <summary>
    /// Decreases the probability of symbol by raising the probability of all other symbols and brings all symbols to the same scale of 1.
    /// </summary>
    /// <param name="symbol">String symbol to be decreased</param>
    /// <param name="amount">float amount to decrease by</param>
    public void DecreaseSymbolProbability(string symbol,float amount)
    {
        outputs[symbol] = outputs[symbol] / (1 + amount);
        foreach (string otherSymbol in outputs.Keys)
        {
            if (otherSymbol != symbol)
            {
                outputs[otherSymbol] = (outputs[otherSymbol] + amount/(outputs.Keys.Count-1)) / (1 + amount);
            }
        }
    }
}
