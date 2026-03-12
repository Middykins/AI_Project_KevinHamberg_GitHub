using System;
using UnityEngine;

public class Belief
{
    public string name;
    public Func<bool> condition = ()=> false;
    public Belief(string name, Func<bool> condition)
    {
        this.name = name;
        this.condition = condition;
    }

    public bool GetCondition()
    {
        return condition();
    }
}