using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnyderStart : MonoBehaviour
{

    public GameObject spawn;
    public Text text;
    public Model model;
    public int currentGeneration = 0;
    //public float alpha, beta, gamma, zeta, eta, kappa, nu, tau;
    private List<float> times;
    private float startTime;
    public int timesFoundFood;

    // Use this for initialization
    void Start()
    {
        times = new List<float>();
        text.text = "Generation " + currentGeneration;
        //Model.parameters = new float[] { alpha, beta, gamma, zeta, eta, kappa, nu, tau };
        BuildModel();
        GameObject newSpawn = Instantiate(spawn);
        LadyBug brain = newSpawn.GetComponent<LadyBug>();
        brain.AcceptModel(model);
        startTime = Time.time;
    }

    public void NextGeneration(Model m)
    {
        m.ForceTimeOut();
        times.Add(Time.time - startTime);
        times.Sort();
        currentGeneration++;
        text.text = "Generation " + currentGeneration + "\nFastest Time: " + times[0] + "\nFound food: " + timesFoundFood;
        model = m;
        GameObject newSpawn = Instantiate(spawn);
        LadyBug brain = newSpawn.GetComponent<LadyBug>();
        brain.AcceptModel(m);
        brain.generationID = currentGeneration;
        startTime = Time.time;
    }

    private void BuildModel()
    {
        List<string> Sigma = new List<string>();
        List<string> Delta = new List<string>();
        List<State> states = new List<State>();
        List<State> rewards = new List<State>();
        List<State> punishments = new List<State>();

        Sigma.AddRange(new string[] { "1N", "1S", "1W", "1E", "2N", "2S", "2W", "2E", "F" });
        Delta.AddRange(new string[] { "LEFT", "RIGHT", "FORWARD" });

        State s0 = new State(0); //Default
        State s1 = new State(1); //1N
        State s2 = new State(2); //2N
        State s3 = new State(3); //1E
        State s4 = new State(4); //2E
        State s5 = new State(5); //1S
        State s6 = new State(6); //2S
        State s7 = new State(7); //1W
        State s8 = new State(8); //2W
        State s9 = new State(9); //F

        states.AddRange(new State[] { s1, s2, s3, s4, s5, s6, s7, s8, s9 });
        rewards.AddRange(new State[] { s9 });
        punishments.AddRange(new State[] { });

        Transition transition0_1 = new Transition(0, 1, "1N");
        Transition transition0_2 = new Transition(0, 2, "2N");
        Transition transition0_3 = new Transition(0, 3, "1E");
        Transition transition0_4 = new Transition(0, 4, "2E");
        Transition transition0_5 = new Transition(0, 5, "1S");
        Transition transition0_6 = new Transition(0, 6, "2S");
        Transition transition0_7 = new Transition(0, 7, "1W");
        Transition transition0_8 = new Transition(0, 8, "2W");
        Transition transition0_9 = new Transition(0, 9, "F");

        transition0_1.SetConfidence(100);
        transition0_2.SetConfidence(100);
        transition0_3.SetConfidence(100);
        transition0_4.SetConfidence(100);
        transition0_5.SetConfidence(100);
        transition0_6.SetConfidence(100);
        transition0_7.SetConfidence(100);
        transition0_8.SetConfidence(100);
        transition0_9.SetConfidence(100);
        s0.AddTransitionOnSymbol(transition0_1, transition0_1.GetSymbol());
        s0.AddTransitionOnSymbol(transition0_2, transition0_2.GetSymbol());
        s0.AddTransitionOnSymbol(transition0_3, transition0_3.GetSymbol());
        s0.AddTransitionOnSymbol(transition0_4, transition0_4.GetSymbol());
        s0.AddTransitionOnSymbol(transition0_5, transition0_5.GetSymbol());
        s0.AddTransitionOnSymbol(transition0_6, transition0_6.GetSymbol());
        s0.AddTransitionOnSymbol(transition0_7, transition0_7.GetSymbol());
        s0.AddTransitionOnSymbol(transition0_8, transition0_8.GetSymbol());
        s0.AddTransitionOnSymbol(transition0_9, transition0_9.GetSymbol());

        Distribution canForward = (new Distribution(Delta));
        canForward.SetDistribution(new List<Symbol>() { new Symbol("LEFT", 0.333f), new Symbol("RIGHT", 0.333f), new Symbol("FORWARD", 0.333f), new Symbol("", 0f) });
        Distribution cannotForward = (new Distribution(Delta));
        cannotForward.SetDistribution(new List<Symbol>() { new Symbol("LEFT", 0.5f), new Symbol("RIGHT", 0.5f), new Symbol("FORWARD", 0.0f), new Symbol("", 0f) });

        transition0_1.SetDistribution(cannotForward);
        transition0_2.SetDistribution(cannotForward);
        transition0_3.SetDistribution(canForward);
        transition0_4.SetDistribution(canForward);
        transition0_5.SetDistribution(cannotForward);
        transition0_6.SetDistribution(cannotForward);
        transition0_7.SetDistribution(cannotForward);
        transition0_8.SetDistribution(canForward);

        model = new Model(states, rewards, punishments,Sigma, Delta, s0 );
    }
}
