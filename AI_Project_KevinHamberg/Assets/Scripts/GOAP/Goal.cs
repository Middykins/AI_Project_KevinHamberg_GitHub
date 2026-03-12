using System.Collections.Generic;
using UnityEngine;

public class Goal
{
    public int priority;
    public string name;
    public HashSet<Belief> beliefs = new();
    
    public Goal(int priority, string name)
    {
        this.priority = priority;
        this.name = name;
    }


}
