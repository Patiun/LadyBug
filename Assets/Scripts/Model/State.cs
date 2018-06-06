using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class State {

    private readonly List<Transition> transitions; //List of transitions from this state
    private readonly Dictionary<string, Transition> transitionBySymbol; //Dictionary of transition based on a symbol
    private readonly int stateId; //ID of this state for lookup in the model's array of states
    private readonly string stateName; //Name of this state for later reference

    /// <summary>
    /// Creates a new state.
    /// </summary>
    /// <param name="id">ID of the state in Model's state array.</param>
    public State(int id)
    {
        this.stateId = id;
        this.stateName = "State " + stateId;
        this.transitions = new List<Transition>();
        this.transitionBySymbol = new Dictionary<string, Transition>();
    }

    /// <summary>
    /// Returns the ID of the state that results from taking the transition on symbol. If that transition does not exist, returns this states ID.
    /// </summary>
    /// <param name="symbol">String of the symbol to take the transition on</param>
    /// <returns>Int ID of the state in model's list of states</returns>
    public int TakeTransitionOn(string symbol)
    {
        if (HasTransitionOnSymbol(symbol))
        {
            Transition selectedTransition = transitionBySymbol[symbol];
            return selectedTransition.GetEndStateID();
        }
        return stateId;
    }

    /// <summary>
    /// Adds a transition on a symbol to the state. Adds to both the dictionary and the list.
    /// </summary>
    /// <param name="transition">Transition to add</param>
    /// <param name="symbol">Symbol to transition on</param>
    public void AddTransitionOnSymbol(Transition transition, string symbol)
    {
        this.transitionBySymbol.Add(symbol, transition);
        this.transitions.Add(transition);
    }

    /// <summary>
    /// Returns true if there is a transition on the symbol from this state.
    /// </summary>
    /// <param name="symbol">string of the symbol to check.</param>
    /// <returns>True if the state has a transition on symbol.</returns>
    public bool HasTransitionOnSymbol(string symbol)
    {
        return this.transitionBySymbol.ContainsKey(symbol);
    }

    /// <summary>
    /// Returns the state's ID.
    /// </summary>
    /// <returns>Int ID of the state in Model's state array.</returns>
    public int GetStateId()
    {
        return this.stateId;
    }

    /// <summary>
    /// Returns the name of the state.
    /// </summary>
    /// <returns>String name of the state.</returns>
    public string GetStateName()
    {
        return this.stateName;
    }

    /// <summary>
    /// Returns all of the transitions from this state.
    /// </summary>
    /// <returns>List of transitions from this state.</returns>
    public List<Transition> GetAllTransitions()
    {
        return this.transitions;
    }

    /// <summary>
    /// Returns the transition from this state on the symbol.
    /// </summary>
    /// <param name="symbol">String symbol for the transition</param>
    /// <returns></returns>
    public Transition GetTransitionOn(string symbol)
    {
        if (HasTransitionOnSymbol(symbol))
        {
            return transitionBySymbol[symbol];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Remove the transition on the selected symbol from the list of transitions.
    /// </summary>
    /// <param name="symbol">string selected symbol name</param>
    public void RemoveTransitionOn(string symbol)
    {
        Transition selectedTransition = transitionBySymbol[symbol];
        transitions.Remove(selectedTransition);
        transitionBySymbol.Remove(symbol);
    }
}
