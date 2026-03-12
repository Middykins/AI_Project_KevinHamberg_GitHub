using System.Resources;
using System.Security;
using TMPro;
using UnityEngine;

public class BaseScript : MonoBehaviour
{
    [SerializeField] private int resourceAvailable = 0;
    [SerializeField] private TMP_Text textThing;
    [SerializeField] private GameObject Worker;
    [SerializeField] private GameObject Soldier;

    public GOAPAgent agent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textThing.text = "Available to Spend: " + resourceAvailable;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Team 1 Worker" || other.gameObject.tag == "Team 2 Worker")
        {
            if (other.GetComponent<Worker>().GetCarry() != 0)
            {
                other.GetComponent<Worker>().SetCarry(0);
                resourceAvailable++;
            }
        }
    }

    public int GetResource()
    {
        return resourceAvailable;
    }

    public void SpawnWorker()
    {
        if (resourceAvailable >= 5)
        {
            if(this.tag == "Team 2 Base")
            {
                agent.workers.Add(Instantiate(Worker));
            }
            else
            {
                Instantiate(Worker);
            }
            
            resourceAvailable -= 5;
        }
    }

    public void SpawnSoldier()
    {
        if (resourceAvailable >= 5)
        {
            if(this.tag == "Team 2 Base")
            {
                agent.soldiers.Add(Instantiate(Soldier));
            }
            else if(this.tag == "Team 1 Base")
            {
                agent.humanSoldiers.Add(Instantiate(Soldier));
            }
            else
            {
                Instantiate(Soldier);
            }
            resourceAvailable -= 5;
        }
    }
}
