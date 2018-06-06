using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadyBug : MonoBehaviour
{

    private Model model;
    public string[] possiblePositions = new string[] { "1N", "1S", "1W", "1E", "2N", "2S", "2W", "2E" };
    public string currentPos;
    public float dist = 5f;
    public float turn = 90f;
    public float timeDelay;
    public float curTime;
    public bool hasFood;
    public int generationID;
    public int maxSteps;
    public int curSteps;
    public bool ready;
    public float waitTime;
    public ParticleSystem foodReward;

    // Use this for initialization
    void OnEnable()
    {
        curSteps = 0;
        ChooseRandomStartingPoint();
        StartCoroutine(GetReady());
    }

    // Update is called once per frame
    void Update()
    {
        if (ready)
        {
            if (!hasFood)
            {
                if (curTime >= timeDelay)
                {
                    Debug.Log(model);
                    model.HandleInput(new List<Symbol>() { new Symbol(currentPos, 1.0f) });
                    Symbol output = model.RetireveOutput();
                    Debug.Log(output.GetName() + " : " + currentPos);
                    Step(output.GetName());
                    curTime = 0;
                }
                else
                {
                    curTime += Time.deltaTime;
                }
            }
        }
    }

    private IEnumerator GetReady()
    {
        yield return new WaitForSeconds(waitTime);
        ready = true;
    }

    private void NewGeneration()
    {
        GameObject.Find("Spawner").GetComponent<SnyderStart>().NextGeneration(model);
        Debug.LogWarning("[ALERT] New Generation " + (generationID + 1));
        Destroy(this.gameObject);
    }

    public void AcceptModel(Model m)
    {
        model = m;
    }

    private void ChooseRandomStartingPoint()
    {
        currentPos = possiblePositions[Random.Range(0, possiblePositions.Length)];
        switch (currentPos)
        {
            case "1E":
                TurnRight();
                currentPos = "1E";
                break;
            case "2E":
                transform.position += transform.right * dist;
                TurnRight();
                currentPos = "2E";
                break;
            case "1W":
                TurnLeft();
                currentPos = "1W";
                break;
            case "2W":
                transform.position += transform.right * dist;
                TurnLeft();
                currentPos = "2W";
                break;
            case "2N":
                transform.position += transform.right * dist;
                break;
            case "1S":
                transform.Rotate(new Vector3(0, 180, 0));
                break;
            case "2S":
                transform.position += transform.right * dist;
                transform.Rotate(new Vector3(0, 180, 0));
                break;
        }
    }

    private void Step(string action)
    {
        curSteps++;
        if (curSteps >= maxSteps)
        {
            NewGeneration();
            return;
        }
        switch (action)
        {
            case "LEFT":
                TurnLeft();
                break;
            case "RIGHT":
                TurnRight();
                break;
            case "FORWARD":
                MoveForward();
                break;
        }
    }

    private void TurnLeft()
    {
        switch (currentPos)
        {
            case "1E":
                currentPos = "1N";
                break;
            case "2E":
                currentPos = "2N";
                break;
            case "1W":
                currentPos = "1S";
                break;
            case "2W":
                currentPos = "2S";
                break;
            case "1N":
                currentPos = "1W";
                break;
            case "2N":
                currentPos = "2W";
                break;
            case "1S":
                currentPos = "1E";
                break;
            case "2S":
                currentPos = "2E";
                break;
        }
        transform.Rotate(new Vector3(0, -turn, 0));
    }

    private void TurnRight()
    {
        switch (currentPos)
        {
            case "1E":
                currentPos = "1S";
                break;
            case "2E":
                currentPos = "2S";
                break;
            case "1W":
                currentPos = "1N";
                break;
            case "2W":
                currentPos = "2N";
                break;
            case "1N":
                currentPos = "1E";
                break;
            case "2N":
                currentPos = "2E";
                break;
            case "1S":
                currentPos = "1W";
                break;
            case "2S":
                currentPos = "2W";
                break;
        }
        transform.Rotate(new Vector3(0, turn, 0));
    }

    private void MoveForward()
    {
        switch (currentPos)
        {
            case "1E":
                currentPos = "2E";
                transform.position += transform.forward * dist;
                break;
            case "2E":
                currentPos = "F";
                transform.position += transform.forward * dist / 2;
                foodReward.Play();
                hasFood = true;
                GameObject.Find("Spawner").GetComponent<SnyderStart>().timesFoundFood++;
                NewGeneration();
                break;
            case "2W":
                currentPos = "1W";
                transform.position += transform.forward * dist;
                break;
        }
    }
}
