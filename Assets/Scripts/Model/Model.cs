using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Model{
    //Model Parameters
    public static float expectationChangeParameter; //alpha - how fast expectations are acquired and lost
    public static float confidenceLossParameter; //beta - how fast confidence is lost
    public static float conditioningLearningParameter; //gamma - how fast learning takes place during conditioning
    public static float supervisedLearningParameter; //zeta - how fast learning takes place during supervised learning
    public static float probailityPropagationParameter; //nu - how much the probability changes are propagated to other transitions with the same input symbol
    public static float temporalFalloffParameter; //kappa - how heavily past actions fall off in importance
    public static float epsilonProbabilityParameter; //eta - probaility that is assigned to epislon as an output
    public static float disconnectionTimeParameter; //tau - how long between events until they are no longer related

    //Base Variables
    public static List<string> outputSymbols; //Delta - List of all possible output symbols STATIC
    public static List<string> inputSymbols; //Sigma - List of all possible input symbols STATIC
    private readonly List<State> allStates; //Q - List of all states in this model
    private readonly List<State> rewardStates; //R - List of only reward states
    private readonly List<State> punishmentStates; //P - List of only punishment states
    private readonly State initialState; //q0 - The starting state

    //Tracking Variables
    private State currentState; //c - The current state
    private State lastState; //ql - The last state visited TODO: Think about implementing a stack as "memory" of states
    private State anchorState; //qa - The state that all new states will transition back to
    private Symbol currentSymbol; //ad,sd - Current strongest input symbol strength pair
    private Symbol lastSymbol; //al - Last strongest input symbol strength pair
    private Symbol currentOutput; //o - Current output symbol strength pair
    private Symbol lastOutput; //ol - Last output symbol strength pair
    private List<Symbol> currentInput; //I - current list of symbol strength pairs
    private List<Symbol> lastInput; //Il - last list of symbol strength pairs

    /// <summary>
    /// Struct to hold the pair of marked symbols and distributions.
    /// </summary>
    private struct MarkedEntry
    {
        public string symbolName;
        public Transition transition;

        public MarkedEntry(string symbolName,Transition transition)
        {
            this.symbolName = symbolName;
            this.transition = transition;
        }
    }
    private List<MarkedEntry> marked;//List of marked entries containing symbol and distribution pairs
    private List<Distribution> conditioned;//List of conditioned distributions
    private float timeSinceLastInput;//How much time has passed since it took its last input in

    /// <summary>
    /// Initializes the model by reading in the input parameters.
    /// </summary>
    /// <param name="allStates">List of all starting states.</param>
    /// <param name="rewardStates">List of all reward states.</param>
    /// <param name="punishmentStates">List of all punishement states.</param>
    /// <param name="Sigma">List of all possible input symbols to handle</param>
    /// <param name="Delta">List of all possible output symbols to handle</param>
    /// <param name="initialState">State in allStates to start the model on</param>
    public Model(List<State> allStates, List<State> rewardStates, List<State> punishmentStates, List<string> Sigma, List<string> Delta, State initialState)
    {
        //Load in starting variables
        this.allStates = allStates;
        this.rewardStates = rewardStates;
        this.punishmentStates = punishmentStates;
        inputSymbols = Sigma;
        outputSymbols = Delta;
        this.initialState = initialState;

        //Setup tracking variables
        currentState = this.initialState;
        lastState = this.initialState;
        anchorState = this.initialState;
        currentSymbol = Symbol.EPSILON_SYMBOL;
        lastSymbol = Symbol.EPSILON_SYMBOL;
        currentOutput = Symbol.EPSILON_SYMBOL;
        lastOutput = Symbol.EPSILON_SYMBOL;
        currentInput = new List<Symbol>();
        lastInput = new List<Symbol>();
        marked = new List<MarkedEntry>();
        conditioned = new List<Distribution>();
        timeSinceLastInput = 0f;
    }

    //!!!!! - TODO - !!!!!
    public void Tick(float deltaTime) //TODO needs to handle deltaTime
    {
        timeSinceLastInput += deltaTime;
        if (IsEventNoLongerRelevent()) //Step 2
        {
            Debug.Log("Num States: " + allStates.Count);
            if (currentState.HasTransitionOnSymbol(Symbol.EPSILON)) //Is c on E defined
            {
                Transition currentEpsilonTransition = currentState.GetTransitionOn(Symbol.EPSILON);
                if (currentEpsilonTransition.isTemporary)
                {
                    currentEpsilonTransition.isTemporary = false;
                }
                lastState = currentState;
                //Debug.Log(currentState.TakeTransitionOn(Symbol.EPSILON));
                currentState = GetStateByID(currentState.TakeTransitionOn(Symbol.EPSILON)); //Get state by ID after taking epsilon transition on current state
            }
            anchorState = currentState;
            lastSymbol = Symbol.EPSILON_SYMBOL;
            lastOutput = Symbol.EPSILON_SYMBOL;
            UnmarkAll();
        }
    }

    public void ForceTimeOut()
    {
        Debug.Log("Num States: " + allStates.Count);
        if (currentState.HasTransitionOnSymbol(Symbol.EPSILON)) //Is c on E defined
        {
            Transition currentEpsilonTransition = currentState.GetTransitionOn(Symbol.EPSILON);
            if (currentEpsilonTransition.isTemporary)
            {
                currentEpsilonTransition.isTemporary = false;
            }
            lastState = currentState;
            //Debug.Log(currentState.TakeTransitionOn(Symbol.EPSILON));
            currentState = GetStateByID(currentState.TakeTransitionOn(Symbol.EPSILON)); //Get state by ID after taking epsilon transition on current state
        }
        anchorState = currentState;
        lastSymbol = Symbol.EPSILON_SYMBOL;
        lastOutput = Symbol.EPSILON_SYMBOL;
        UnmarkAll();
    }

    /*
     * Abstracted Step2 out of the system to account for real time and let input handling happen seperately
     */

    /// <summary>
    /// Takes the list of symbol strength pairs, creates new neurons if needed, advances to the next neuron based on the strongest pair, produces an output, and updates the system to reflect the new knowledge from the input.
    /// </summary>
    /// <param name="newInput">List of Symbols to be handled</param>
    public void HandleInput(List<Symbol> newInput)
    {
        timeSinceLastInput = 0f;
        //Step 3
        lastInput = currentInput;
        currentInput = newInput;
        //Step 4
        currentSymbol = GetStrongestPair(currentInput);
        //Step 5
        CreateNewTransitions();
        //Debug.Log(currentState.GetStateName());
        Transition currentTransition = currentState.GetTransitionOn(currentSymbol.GetName());
        //Debug.Log("Retrieving transitionf from: " + currentSymbol.GetName());
        //Step 6
        lastOutput = currentOutput;
        //Step 7
        //Debug.Log(currentTransition);
        currentOutput = new Symbol(currentTransition.GetOutput(),(currentSymbol.GetStrength()*currentTransition.GetConfidence())/(1+ currentTransition.GetConfidence()));
        HandleOutput(currentOutput);
        //Step 8
        Mark(currentOutput.GetName(), currentTransition);
        //Step 9
        UpdateExpectations();
        //Step 10
        lastState = currentState;
        //Step 11
        currentState = GetStateByID(currentTransition.GetEndStateID());
        //Step 12
        lastSymbol = currentSymbol;
        //Step 13
        if (rewardStates.Contains(currentState))
        {
            ApplyReward();
        }
        else if (punishmentStates.Contains(currentState))
        {
            ApplyPunishment();
        }
        else
        {
            ApplyConditioning();
        }
    }

    public void HandleOutput(Symbol selectedOutput)
    {
        //TODO
        Debug.Log(selectedOutput.GetName() + " " + selectedOutput.GetStrength());
    }

    public Symbol RetireveOutput()
    {
        return currentOutput;
    }

    /// <summary>
    /// Creates a new transition for every input symbol if a transition does not exist from the currentState on that symbol.
    /// </summary>
    private void CreateNewTransitions()
    {
        if (currentState.HasTransitionOnSymbol(Symbol.EPSILON))
        {
            Transition epsilonTransition = currentState.GetTransitionOn(Symbol.EPSILON);
            if (epsilonTransition.isTemporary)
            {
                currentState.RemoveTransitionOn(Symbol.EPSILON);
            }
        }
        foreach(Symbol inputSymbol in currentInput)
        {
            string symbolName = inputSymbol.GetName();
            if (!currentState.HasTransitionOnSymbol(symbolName))
            {
                State newState = new State(allStates.Count);
                allStates.Add(newState);
                Transition newTransition = new Transition(currentState.GetStateId(), newState.GetStateId(), symbolName);
                currentState.AddTransitionOnSymbol(newTransition, symbolName);
               // Debug.Log("Making transition on " + symbolName);
                Transition newEpsilonTransition = new Transition(newState.GetStateId(), anchorState.GetStateId(), Symbol.EPSILON) { isTemporary = true };
                newState.AddTransitionOnSymbol(newEpsilonTransition, Symbol.EPSILON);
                foreach(State selectedState in allStates)
                {
                    if (selectedState.HasTransitionOnSymbol(symbolName))
                    {
                        Transition selectedTransition = selectedState.GetTransitionOn(symbolName);
                        newTransition.CopyFrom(selectedTransition);
                        State selectedEndState = GetStateByID(selectedTransition.GetEndStateID());
                        if (rewardStates.Contains(selectedEndState))
                        {
                            rewardStates.Add(newState);
                        }
                        else if (punishmentStates.Contains(selectedEndState))
                        {
                            punishmentStates.Add(newState);
                        }
                        break;
                    }
                }
            }

        }
    }

    /// <summary>
    /// Updates the expectations around the currentState by strenghtening the expectation with the previous transition to the current one and all transitions that entered in the same input list. It also weakens the transitions that were not considered.
    /// </summary>
    private void UpdateExpectations()
    {
        Transition lastTransition = lastState.GetTransitionOn(lastSymbol.GetName());
        Transition currentTransition = currentState.GetTransitionOn(currentSymbol.GetName());
        if (lastTransition != null)
        {
            if (lastTransition.HasExpectationWith(currentTransition))
            {
                lastTransition.StrengthenExpectationWith(currentTransition);
                currentTransition.StrengthenExpectationWith(lastTransition);
            }
            else
            {
                lastTransition.CreateExpectationWith(currentTransition);
                currentTransition.CreateExpectationWith(lastTransition);
            }

            foreach (string symbolA in inputSymbols)
            {
                if (!SymbolInInput(symbolA))
                {
                    Transition symbolTransition = currentState.GetTransitionOn(symbolA);
                    if (lastTransition.HasExpectationWith(symbolTransition))
                    {
                        lastTransition.WeakenExpectationWith(symbolTransition);
                        symbolTransition.WeakenExpectationWith(lastTransition);
                    }
                }
                foreach (string symbolB in inputSymbols)
                {
                    if (symbolA != symbolB)
                    {
                        Transition aTransition = currentState.GetTransitionOn(symbolA);
                        Transition bTransition = currentState.GetTransitionOn(symbolB);
                        if (aTransition != null)
                        {
                            if (SymbolInInput(symbolA) && SymbolInInput(symbolB))
                            {
                                if (aTransition.HasExpectationWith(bTransition))
                                {
                                    aTransition.StrengthenExpectationWith(bTransition);
                                }
                                else
                                {
                                    aTransition.CreateExpectationWith(bTransition);
                                }
                            }
                            else if (SymbolInInput(symbolA) || SymbolInInput(symbolB))
                            {
                                if (aTransition.HasExpectationWith(bTransition))
                                {
                                    aTransition.WeakenExpectationWith(bTransition);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Apply reward to all transitions output distributions with decreasing values over time.
    /// </summary>
    private void ApplyReward()
    {
        float timeFactor = 1;
        foreach(MarkedEntry entry in marked)
        {
            string selectedSymbol = entry.symbolName;
            Transition selectedTransition = entry.transition; //Should this be the last transition on the chosen inputsymbol?
            float amount = supervisedLearningParameter * timeFactor * currentSymbol.GetStrength();
            selectedTransition.IncreaseProbabilityFor(selectedSymbol, amount);
            selectedTransition.UpdateConfidence(amount);
            foreach(State state in allStates)
            {
                if (state.GetStateId() != selectedTransition.GetStartStateID())
                {
                    Transition stateTransition = state.GetTransitionOn(selectedTransition.GetSymbol());
                    if (stateTransition != null)
                    {
                        stateTransition.IncreaseProbabilityFor(selectedSymbol, amount * probailityPropagationParameter);
                        stateTransition.UpdateConfidence(amount * probailityPropagationParameter);
                    }
                }
            }
            timeFactor = timeFactor * temporalFalloffParameter;
        }
        UnmarkAll();
    }

    /// <summary>
    /// Apply punishment to all transitions output distributions with decreasing values over time.
    /// </summary>
    private void ApplyPunishment()
    {
        float timeFactor = 1;
        foreach (MarkedEntry entry in marked)
        {
            string selectedSymbol = entry.symbolName;
            Transition selectedTransition = entry.transition; //Should this be the last transition on the chosen inputsymbol?
            float amount = supervisedLearningParameter * timeFactor * currentSymbol.GetStrength();
            selectedTransition.DecreaseProbabilityFor(selectedSymbol, amount);
            selectedTransition.UpdateConfidence(amount * probailityPropagationParameter);
            foreach (State state in allStates)
            {
                if (state.GetStateId() != selectedTransition.GetStartStateID())
                {
                    Transition stateTransition = state.GetTransitionOn(selectedTransition.GetSymbol());
                    if (stateTransition != null)
                    {
                        stateTransition.DecreaseProbabilityFor(selectedSymbol, amount * probailityPropagationParameter);
                        stateTransition.UpdateConfidence(amount * probailityPropagationParameter);
                    }
                }
            }
            timeFactor = timeFactor * temporalFalloffParameter;
        }
        UnmarkAll();
    }

    /// <summary>
    /// Starts conditioning cycle by updating the lastOutput across associated transitions
    /// </summary>
    private void ApplyConditioning()
    {
        conditioned = new List<Distribution>();
        if (lastOutput != Symbol.EPSILON_SYMBOL && lastOutput.GetName() != currentOutput.GetName())
        {
            Transition lastTransition = lastState.GetTransitionOn(lastSymbol.GetName());
            foreach(string symbol in inputSymbols)
            {
                Transition selectedTransition = lastState.GetTransitionOn(symbol);
                if (lastTransition.HasExpectationWith(selectedTransition) && SymbolInLastInput(symbol))
                {
                    selectedTransition.IncreaseProbabilityFor(lastOutput.GetName(), conditioningLearningParameter * currentSymbol.GetStrength());
                }
            }
            foreach(State state in allStates)
            {
                foreach (string symbol in inputSymbols)
                {
                    Transition selectedTransition = state.GetTransitionOn(symbol);
                    if (selectedTransition != null)
                    {
                        if (selectedTransition.GetEndStateID() == lastState.GetStateId())
                        {
                            selectedTransition.IncreaseProbabilityFor(lastOutput.GetName(), conditioningLearningParameter * currentSymbol.GetStrength());
                        }
                    }
                }
            }
            foreach(string symbol in inputSymbols)
            {
                Transition selectedTransition = lastState.GetTransitionOn(symbol);
                if (selectedTransition != null)
                {
                    Distribution selectedDistribution = selectedTransition.GetDistribution();
                    if (lastTransition.HasExpectationWith(selectedTransition) && SymbolInLastInput(symbol) && !conditioned.Contains(selectedDistribution))
                    {
                        selectedTransition.UpdateConfidence(conditioningLearningParameter * currentSymbol.GetStrength());
                        UpdateConditioning(lastState, symbol, currentSymbol.GetStrength() / selectedTransition.GetConfidence());
                    }
                }
                foreach(State selectedState in allStates)
                {
                    selectedTransition = selectedState.GetTransitionOn(symbol);
                    if (selectedTransition != null)
                    {
                        if (selectedTransition.GetEndStateID() == lastState.GetStateId())
                        {
                            if (selectedTransition != null)
                            {
                                Distribution selectedDistribution = selectedTransition.GetDistribution();
                                if (lastTransition.HasExpectationWith(selectedTransition) && !conditioned.Contains(selectedDistribution))
                                {
                                    selectedTransition.UpdateConfidence(conditioningLearningParameter * currentSymbol.GetStrength());
                                    UpdateConditioning(selectedState, symbol, currentSymbol.GetStrength() / selectedTransition.GetConfidence());
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recursively applys conditioning. See ApplyConditioning. Base case: Strength < 0
    /// </summary>
    /// <param name="selectedState">Starting state</param>
    /// <param name="selectedSymbol">Starting symbol</param>
    /// <param name="strength">Decreasing strength</param>
    private void UpdateConditioning(State selectedState, string selectedSymbol, float strength)
    {
        if (strength > 0)
        {
            Transition selectedTransition = selectedState.GetTransitionOn(selectedSymbol);
            if (selectedTransition != null)
            {
                foreach (string symbol in inputSymbols)
                {
                    Transition symbolTransition = selectedState.GetTransitionOn(symbol);
                    if (symbolTransition != null)
                    {
                        Distribution symbolDistribution = symbolTransition.GetDistribution();
                        if (selectedTransition.HasExpectationWith(symbolTransition) && SymbolInLastInput(symbol) && !conditioned.Contains(symbolDistribution))
                        {
                            symbolTransition.IncreaseProbabilityFor(lastOutput.GetName(),conditioningLearningParameter*strength);
                            symbolTransition.UpdateConfidence(conditioningLearningParameter * strength);
                            conditioned.Add(symbolDistribution);
                            UpdateConditioning(selectedState, symbol, strength / symbolTransition.GetConfidence());
                        }
                    }
                    foreach(State state in allStates)
                    {
                        Transition stateTransition = state.GetTransitionOn(symbol);
                        if (stateTransition != null)
                        {
                            Distribution stateDistribution = stateTransition.GetDistribution();
                            if (selectedTransition.HasExpectationWith(stateTransition) && stateTransition.GetEndStateID() == selectedState.GetStateId() && !conditioned.Contains(stateDistribution))
                            {
                                stateTransition.IncreaseProbabilityFor(lastOutput.GetName(), conditioningLearningParameter * strength);
                                stateTransition.UpdateConfidence(conditioningLearningParameter * strength);
                                conditioned.Add(stateDistribution);
                                UpdateConditioning(state, symbol, strength / stateTransition.GetConfidence());
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns if the symbolName is in any symbol in the currentInput
    /// </summary>
    /// <param name="symbolName">String name of the symbol to check</param>
    /// <returns>True if the symbolName is in the currentInput</returns>
    private bool SymbolInInput(string symbolName)
    {
        foreach(Symbol symbol in currentInput)
        {
            if (symbolName == symbol.GetName())
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns if the symbolName is in any symbol in the lastInput
    /// </summary>
    /// <param name="symbolName">String name of the symbol to check</param>
    /// <returns>True if the symbolName is in the lastInput</returns>
    private bool SymbolInLastInput(string symbolName)
    {
        foreach (Symbol symbol in lastInput)
        {
            if (symbolName == symbol.GetName())
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Creates a MarkedEntry for the symbol name and the transition's distribution and adds it to the list of markedEntries
    /// </summary>
    /// <param name="selectedSymbol">String name of the selectedSymbol</param>
    /// <param name="selectedTransition">Transition that has the distribution that produced the selectedSymbol</param>
    private void Mark(string selectedSymbol, Transition selectedTransition)
    {
        marked.Add(new MarkedEntry(selectedSymbol, selectedTransition));
    }

    /// <summary>
    /// Empties the list of markedEntries by setting it to an empty list.
    /// </summary>
    private void UnmarkAll()
    {
        marked = new List<MarkedEntry>();
    }

    /// <summary>
    /// Chooses the symbol strength pair with the highest strength.
    /// </summary>
    /// <param name="selectedInput">List of symbol strength pairs</param>
    /// <returns>Strongest symbol strength pair</returns>
    private Symbol GetStrongestPair(List<Symbol> selectedInput)
    {
        Symbol output = Symbol.EPSILON_SYMBOL;
        foreach(Symbol symbol in selectedInput)
        {
            if (symbol.GetStrength() > output.GetStrength())
            {
                output = symbol;
            }
        }
        return output;
    }

    /// <summary>
    /// Determines if states should collapse down on epsilon transitions
    /// </summary>
    /// <returns>Returns true if timeSinceLastInput >= disconnectionTimeParameter</returns>
    private bool IsEventNoLongerRelevent()
    {
        if (timeSinceLastInput >= disconnectionTimeParameter) {
            timeSinceLastInput = 0f;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the state that has the selected ID.
    /// </summary>
    /// <param name="ID">Int ID of the state in allStates</param>
    /// <returns>State with ID in allStates</returns>
    public State GetStateByID(int ID)
    {
        if (ID >= 0 && ID < allStates.Count) {
            return allStates[ID];
        }
        else
        {
            return null;
        }
    }
}
