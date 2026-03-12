using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapPlanner
{
    public ActionPlan plan(GOAPAgent agent, HashSet<Goal> goals, Goal mostRecentGoal = null)
    {
        List<Goal> orderedGoals = goals
            .Where(g => g.beliefs.Any(b => !b.GetCondition()))
            .OrderByDescending(g => g == mostRecentGoal ? g.priority - 0.01f : g.priority)
            .ToList();

        foreach(var goal in orderedGoals)
        {
            NodeGOAP goalNode = new NodeGOAP(null, null, goal.beliefs, 0);

            if(FindPath(goalNode, agent.actions)){
                if (goalNode.deadEnd)
                    continue;

                Stack<Action> actionStack = new Stack<Action>();
                while (goalNode.leaves.Count > 0)
                {
                    var cheapestLeaf = goalNode.leaves.OrderBy(leaf => leaf.cost).First();
                    goalNode = cheapestLeaf;
                    actionStack.Push(cheapestLeaf.action);
                }

                return new ActionPlan(goal, actionStack, goalNode.cost);
            }
        }
        Debug.LogWarning("No plan found!");
        return null;
    }

    bool FindPath(NodeGOAP parent, HashSet<Action> actions)
    {
        foreach( var action in actions)
        {
            var requiredBeliefs = parent.required;
            //There is no action to take if belief is true
            requiredBeliefs.RemoveWhere(b => b.GetCondition());

            if (requiredBeliefs.Count == 0)
                return true;

            if (action.effects.Any(requiredBeliefs.Contains))
            {
                var newRequiredBeliefs = new HashSet<Belief>(requiredBeliefs);
                newRequiredBeliefs.ExceptWith(action.effects);
                newRequiredBeliefs.UnionWith(action.preconditions);

                var newNode = new NodeGOAP(parent, action, newRequiredBeliefs, parent.cost + action.cost);
                //recursive search
                if(FindPath(newNode, actions))
                {
                    parent.leaves.Add(newNode);
                    newRequiredBeliefs.ExceptWith(newNode.action.preconditions);
                }

                //if all beliefs have been satisfied, return true
                if (newRequiredBeliefs.Count == 0)
                    return true;
            }
        }

        return false;
    }
}

public class NodeGOAP
{
    public NodeGOAP parent;
    public Action action;
    public HashSet<Belief> required;
    public List<NodeGOAP> leaves;
    public float cost;

    public bool deadEnd => leaves.Count == 0 && action == null;

    public NodeGOAP(NodeGOAP parent, Action action, HashSet<Belief> required, float cost)
    {
        this.parent = parent;
        this.action = action;
        this.required = new HashSet<Belief>(required);
        this.cost = cost;
        leaves = new();
    }
}


public class ActionPlan
{
    public Goal goal;
    public Stack<Action> actions;
    public float totalCost;

    public ActionPlan(Goal goal, Stack<Action> actions, float totalCost)
    {
        this.goal = goal;
        this.actions = actions;
        this.totalCost = totalCost;
    }
}
