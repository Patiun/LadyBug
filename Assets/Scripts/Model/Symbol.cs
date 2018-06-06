using System;

[Serializable]
public class Symbol {

    public static readonly string EPSILON = "";//String denoting epsilon or nothing
    public static readonly Symbol EPSILON_SYMBOL = new Symbol(EPSILON, 0.0f);//Symbol denoting epsilon or nothing

    private readonly string symbolName;//name of the symbol
    private readonly float strength;//Strength of the symbol

    /// <summary>
    /// Creates a new Symbol with the inputed name and strength.
    /// </summary>
    /// <param name="name">Symbol name</param>
    /// <param name="strength">Symbol strength</param>
    public Symbol(string name, float strength)
    {
        this.symbolName = name;
        this.strength = strength;
    }

    /// <summary>
    /// Returns the strength of the symbol.
    /// </summary>
    /// <returns>Float strength value</returns>
    public float GetStrength()
    {
        return this.strength;
    }

    /// <summary>
    /// Returns the name of the symbol.
    /// </summary>
    /// <returns>String symbol name</returns>
    public string GetName()
    {
        return this.symbolName;
    }
}
