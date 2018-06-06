using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    private Model model;

	// Use this for initialization
	void Start ()
    {
        Model.epsilonProbabilityParameter = 0.0f;
        Model.expectationChangeParameter = 0.05f;
        Model.confidenceLossParameter = 0.05f;
        Model.conditioningLearningParameter = 0.1f;
        Model.disconnectionTimeParameter = 2.0f;
        Model.probailityPropagationParameter = 0.2f;
        Model.temporalFalloffParameter = 0.9f;
        Model.supervisedLearningParameter = 0.1f;

        State s0 = new State(0);

        List<State> states = new List<State>() {s0};
        List<State> rewardStates = new List<State>();
        List<State> punishmentStates = new List<State>();
        List<string> inputSymbols = new List<string>() {"A","B","C"};
        List<string> outputSymbols = new List<string>() {"ah","eek","oh"};
        model = new Model(states,rewardStates,punishmentStates,inputSymbols,outputSymbols,states[0]);

        Model.epsilonProbabilityParameter = 0.0f;

        Transition t0_0 = new Transition(0, 0, Symbol.EPSILON);
        s0.AddTransitionOnSymbol(t0_0, t0_0.GetSymbol());
    }
	
	// Update is called once per frame
	void Update () {
        model.Tick(Time.deltaTime);
        if (Input.GetKeyDown("1"))
        {
            model.HandleInput(new List<Symbol>() { new Symbol("A", 1.0f) });
        }
        if (Input.GetKeyDown("2"))
        {
            model.HandleInput(new List<Symbol>() { new Symbol("B", 1.0f) });
        }
        if (Input.GetKeyDown("3"))
        {
            model.HandleInput(new List<Symbol>() { new Symbol("C", 1.0f) });
        }
    }
}
