using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Transition {

    public bool isTemporary;//Mark for if the transition is temporary

    private readonly int startStateID;//ID of the starting state of the transition in the model's list of states
    private readonly int endStateID;//ID of the ending state of the transition in the model's list of states
    private readonly string symbolName;//Name of the symbol that this transition is taken on
    private Distribution outputDistribution;//Distribution of output symbols that this transition uses to determine its output
    private float confidence = 0.1f;//Confidence value of the transition, initialized at 0.1
    private Dictionary<Transition, float> expectations;//Expectations that this transition proceeds another transition

    /// <summary>
    /// Initalizes the transition from the state with startID as an ID to the state with endID as an ID on symbol
    /// </summary>
    /// <param name="startID">Int ID of the state in model's list of states</param>
    /// <param name="endID">Int ID of the state in model's list of states</param>
    /// <param name="symbol">String symbol that this transition is taken on</param>
    public Transition(int startID,int endID,string symbol)
    {
        this.startStateID = startID;
        this.endStateID = endID;
        this.symbolName = symbol;
        this.outputDistribution = new Distribution();
        this.expectations = new Dictionary<Transition, float>();
    }

    /// <summary>
    /// Outputs a string symbol from the transition.
    /// </summary>
    /// <returns>Returns the string from the distribution's ChooseOutput method.</returns>
    public string GetOutput()
    {
        return this.outputDistribution.ChooseOutput();
    }

    /// <summary>
    /// Returns the start state's ID in model's list of states.
    /// </summary>
    /// <returns>Int ID of the start state.</returns>
    public int GetStartStateID()
    {
        return this.startStateID;
    }

    /// <summary>
    /// Returns the end state's ID in model's list of states.
    /// </summary>
    /// <returns>Int ID of the end state</returns>
    public int GetEndStateID()
    {
        return this.endStateID;
    }

    /// <summary>
    /// Returns the confidence of the transition.
    /// </summary>
    /// <returns>Float confidence value</returns>
    public float GetConfidence()
    {
        return confidence;
    }

    /// <summary>
    /// Sets the confidence to the input value.
    /// </summary>
    /// <param name="value">float value to set confidence to</param>
    public void SetConfidence(float value)
    {
        confidence = value;
    }

    /// <summary>
    /// Returns the symbolName of the symbol this transition is taken on.
    /// </summary>
    /// <returns>String symbolName</returns>
    public string GetSymbol()
    {
        return symbolName;
    }

    /// <summary>
    /// Returns the outputDistribution of this transition
    /// </summary>
    /// <returns>Distribution of all possible output symbols</returns>
    public Distribution GetDistribution()
    {
        return this.outputDistribution;
    }

    public void SetDistribution(Distribution newDist)
    {
        outputDistribution = newDist;
    }

    public void CopyFrom(Transition otherTransition)
    {
        //Make Sure to use copy !!!!!
        outputDistribution = otherTransition.GetDistribution();
        confidence = otherTransition.GetConfidence();
    }

    /// <summary>
    /// Returns if this transition has an expectation that otherTransition follows it.
    /// </summary>
    /// <param name="otherTransition">Transition to check for expectation with</param>
    /// <returns>True if the otherTransition is in this transtions list of expectations</returns>
    public bool HasExpectationWith(Transition otherTransition)
    {
        if (otherTransition == null)
        {
            return false;
        }
        return expectations.ContainsKey(otherTransition);
    }

    /// <summary>
    /// Creates an expectation that otherTransition follows this transtion and sets it equal to Model's static expectationChangeParameter.
    /// </summary>
    /// <param name="otherTransition">Transition to form the expectation with</param>
    public void CreateExpectationWith(Transition otherTransition)
    {
        expectations.Add(otherTransition, Model.expectationChangeParameter);
    }

    /// <summary>
    /// Strengthens the expectation that otherTransition follows this transition based on Model's static expectationChangeParameter
    /// </summary>
    /// <param name="otherTransition">Transition to strengthen expectation with</param>
    public void StrengthenExpectationWith(Transition otherTransition)
    {
        if (HasExpectationWith(otherTransition))
        {
            float change = Model.expectationChangeParameter * (1 - expectations[otherTransition]);
            expectations[otherTransition] += change;
            WeakenConfidence(1 - Model.confidenceLossParameter * Math.Abs(change));
        }
    }

    /// <summary>
    /// Weakens the expectation that otherTransition follows this transition based on Model's static expectationChangeParameter
    /// </summary>
    /// <param name="otherTransition">Transition to strengthen expectation with</param>
    public void WeakenExpectationWith(Transition otherTransition)
    {
        if (HasExpectationWith(otherTransition))
        {
            float change = -Model.expectationChangeParameter * expectations[otherTransition];
            expectations[otherTransition] += change;
            WeakenConfidence(1 - Model.confidenceLossParameter * Math.Abs(change));
        }
    }

    /// <summary>
    /// Increases the probabilty of outputing symbol by amount related to the confidence in this transition.
    /// </summary>
    /// <param name="symbol">symbol to be updated</param>
    /// <param name="amount">amount to update</param>
    public void IncreaseProbabilityFor(string symbol, float amount)
    {
        outputDistribution.IncreaseSymbolProbability(symbol, amount/confidence);
    }

    /// <summary>
    /// Decreases the probabilty of outputing symbol by amount related to the confidence in this transition.
    /// </summary>
    /// <param name="symbol">symbol to be updated</param>
    /// <param name="amount">amount to update</param>
    public void DecreaseProbabilityFor(string symbol, float amount)
    {
        outputDistribution.DecreaseSymbolProbability(symbol, amount/confidence);
    }

    /// <summary>
    /// Lowers the confidence by multiplying it by the amount. Used when changing expectations.
    /// </summary>
    /// <param name="amount">Float amount to weaken the confidence by</param>
    private void WeakenConfidence(float amount)
    {
        confidence = confidence * amount;
    }

    /// <summary>
    /// Addes the amount to the confidence
    /// </summary>
    /// <param name="amount">float amount to add to the confidence</param>
    public void UpdateConfidence(float amount)
    {
        confidence = confidence + amount;
    }
}
