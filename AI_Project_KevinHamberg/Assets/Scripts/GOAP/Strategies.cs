using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.AI;

public interface IActionStrategy
{
    public bool canPerform { get; }
    public bool complete { get; }

    void Start()
    {

    }

    void Update(float time = 0)
    {
        
    }

    void Stop()
    {

    }
}

public class IdleStrategy : IActionStrategy
{
    public bool canPerform => true;

    public bool complete { get; set;}

    private float timer;

    public void Start()
    {
        complete = false;
    }

    public void Update(float deltaTime)
    {
        timer += deltaTime;

        if(timer > 2f)
        {
            complete = true;
        }
    }
}

public class RecruitStrategy : IActionStrategy
{
    int money;
    int option;
    BaseScript recruiter;
    public bool canPerform => money >= 5;

    public bool complete {get;set;}

    public RecruitStrategy(BaseScript baseScript, int index)
    {
        money = baseScript.GetResource();
        recruiter = baseScript;
        option = index;
    }

    public void Start()
    {
        if (option == 0)
        {
            recruiter.SpawnWorker();
        }

        else if (option == 1)
        {
            recruiter.SpawnSoldier();
        }
        complete = true;
    }
}

public class MoveStrategy : IActionStrategy
{
    Transform targetPos;
    Transform soldierPos;
    GameObject targetYes;
    public bool canPerform => !complete;

    public bool complete => Vector3.Distance(targetPos.position, soldierPos.position) < 1;

    public MoveStrategy(GameObject target, Transform position, GameObject soldier)
    {
        targetPos = position;
        soldierPos = soldier.transform;
        targetYes = target;
    }

    public void Start()
    {
        targetYes.transform.position = targetPos.position;
    }
}


