using System.Diagnostics;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] GOAPAgent agent;
    [SerializeField] TMP_Text debug;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string stack = "", beliefs = "";
        foreach (var action in agent.actionPlan?.actions)
        {
            stack += action.name + "\n";
        }

        foreach (var action in agent.beliefs)
        {
            beliefs += action.Key + " " + action.Value.GetCondition() + "\n";
        }
        debug.text = "Current Goal:\n" + agent.currentGoal?.name + "\n\nCurrentAction:\n" + agent.currentAction?.name +
            "\n\nPlanStack:\n" + stack + "\nBeliefs:\n" + beliefs;
    }
}
