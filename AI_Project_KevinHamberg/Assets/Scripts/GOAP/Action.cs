using System.Collections.Generic;
using UnityEngine;

public class Action
{
    public string name;
    public float cost;
    public HashSet<Belief> preconditions = new();
    public HashSet<Belief> effects = new();

    public IActionStrategy actionStrategy;

    public Action(string name, IActionStrategy actionStrategy, float cost = 1)
    {
        this.name = name;
        this.cost = cost;
        this.actionStrategy = actionStrategy;
    }

    public bool Complete => actionStrategy.complete;

    public void Start() => actionStrategy.Start();

    public void Stop() => actionStrategy.Stop();

    public void Update(float time)
    {
        if (actionStrategy.canPerform)
        {
            actionStrategy.Update(time);
        }
        if (!Complete)
        {
            return;
        }
        else
        {
            foreach (var belief in effects)
            {
                belief.GetCondition();
            }
        }
    }
}
