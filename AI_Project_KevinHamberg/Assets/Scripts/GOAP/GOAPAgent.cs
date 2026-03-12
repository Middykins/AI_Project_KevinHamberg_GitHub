using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GOAPAgent : MonoBehaviour
{
    public Goal lastGoal;
    public Goal currentGoal;
    public Action currentAction;
    public Dictionary<string, Belief> beliefs = new();
    public HashSet<Action> actions = new();
    public HashSet<Goal> goals = new();
    public ActionPlan actionPlan;
    private GoapPlanner agentPlanner;

    public List<GameObject> workers = new();
    public List<GameObject> soldiers = new();
    public List<GameObject> humanSoldiers = new();

    public BaseScript baseScript;
    public GameObject target;
    public Transform playerBasePos;
    public Transform ownBasePos;

    float timer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetBelief();
        SetGoal();
        SetAction();
    }

    private void Awake()
    {
        agentPlanner = new GoapPlanner();
    }

    // Update is called once per frame
    private void Update()
    {

        if (currentAction == null)
        {
            Debug.Log("Calculating new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.actions.Count > 0)
            {
                currentGoal = actionPlan.goal;

                currentAction = actionPlan.actions.Pop();

                currentAction.Start();
            }
        }

        if (actionPlan != null && currentAction != null)
        {
            currentAction.Update(Time.deltaTime);

            if (currentAction.Complete)
            {
                currentAction.Stop();
                currentAction = null;
                if (actionPlan.actions.Count == 0)
                {
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }
    }

    void CalculatePlan()
    {
        var priorityLevel = currentGoal?.priority ?? 0;

        HashSet<Goal> goalsToCheck = goals;

        if (currentGoal != null)
        {
            goalsToCheck = new HashSet<Goal>(goals.Where(g => g.priority > priorityLevel));
        }

        var potentialPlan = agentPlanner.plan(this, goalsToCheck, lastGoal);
        if (potentialPlan != null)
        {
            actionPlan = potentialPlan;
        }
    }

    void SetBelief()
    {
        Belief belief = new Belief("Nothing", ()=> false);
        beliefs.Add(belief.name, belief);

        belief = new Belief("MoveArmy", () => false);
        beliefs.Add(belief.name, belief);

        belief = new Belief("MoveArmyToPlayer", () => false);
        beliefs.Add(belief.name, belief);

        belief = new Belief("MoveArmyToBase", () => false);
        beliefs.Add(belief.name, belief);

        belief = new Belief("NotAtPlayer", () => Vector3.Distance(soldiers[0].transform.position, playerBasePos.transform.position) > 2);
        beliefs.Add(belief.name, belief);

        belief = new Belief("NotAtBase", () => Vector3.Distance(soldiers[0].transform.position, ownBasePos.transform.position) > 2);
        beliefs.Add(belief.name, belief);

        belief = new Belief("CanRecruit", () => baseScript.GetResource() >= 5);
        beliefs.Add(belief.name, belief);

        belief = new Belief("NeedWorker", () => workers.Count < 4);
        beliefs.Add(belief.name, belief);

        belief = new Belief("NeedSoldier", () => workers.Count >= 4 || soldiers.Count < humanSoldiers.Count);
        beliefs.Add(belief.name, belief);

        belief = new Belief("MoreWorker", () => false);
        beliefs.Add(belief.name, belief);

        belief = new Belief("MoreSoldier", () => false);
        beliefs.Add(belief.name, belief);

        belief = new Belief("StrongerThanPlayer", () => soldiers.Count > humanSoldiers.Count);
        beliefs.Add(belief.name, belief);

        belief = new Belief("WeakerThanPlayer", () => !beliefs["StrongerThanPlayer"].GetCondition());
        beliefs.Add(belief.name, belief);
    }

    void SetGoal()
    {
        Goal goal = new Goal(1, "Idle");
        goal.beliefs.Add(beliefs["Nothing"]);
        goals.Add(goal);

        goal = new Goal(4, "MakeWorker");
        goal.beliefs.Add(beliefs["MoreWorker"]);
        goals.Add(goal);

        goal = new Goal(2, "MakeSoldier");
        goal.beliefs.Add(beliefs["MoreSoldier"]);
        goals.Add(goal);

        goal = new Goal(5, "AttackPlayer");
        goal.beliefs.Add(beliefs["MoveArmyToPlayer"]);
        goals.Add(goal);

        goal = new Goal(6, "Retreat");
        goal.beliefs.Add(beliefs["MoveArmyToBase"]);
        goals.Add(goal);
    }

    void SetAction()
    {
        Action action = new Action("Idle", new IdleStrategy());
        action.effects.Add(beliefs["Nothing"]);
        actions.Add(action);

        action = new Action("RecruitWorker", new RecruitStrategy(baseScript, 0));
        action.preconditions.Add(beliefs["NeedWorker"]);
        action.preconditions.Add(beliefs["CanRecruit"]);
        action.effects.Add(beliefs["MoreWorker"]);
        actions.Add(action);

        action = new Action("RecruitSoldier", new RecruitStrategy(baseScript, 1));
        action.preconditions.Add(beliefs["NeedSoldier"]);
        action.preconditions.Add(beliefs["CanRecruit"]);
        action.effects.Add(beliefs["MoreSoldier"]);
        actions.Add(action);

        /*action = new Action("MoveArmy", new MoveStrategy(targetPos, soldiers[0]));
        action.effects.Add(beliefs["MoveArmy"]);
        actions.Add(action);*/

        action = new Action("Attack", new MoveStrategy(target, playerBasePos, soldiers[0]));
        action.preconditions.Add(beliefs["StrongerThanPlayer"]);
        action.preconditions.Add(beliefs["NotAtPlayer"]);
        action.effects.Add(beliefs["MoveArmyToPlayer"]);
        actions.Add(action);

        action = new Action("Retreat", new MoveStrategy(target, ownBasePos, soldiers[0]));
        action.preconditions.Add(beliefs["WeakerThanPlayer"]);
        action.preconditions.Add(beliefs["NotAtBase"]);
        action.effects.Add(beliefs["MoveArmyToBase"]);
        actions.Add(action);
    }

    public void ResetPlan()
    {

    }
}
