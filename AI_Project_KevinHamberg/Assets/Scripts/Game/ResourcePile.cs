using TMPro;
using UnityEngine;

public class ResourcePile : MonoBehaviour
{
    [SerializeField] private int resourceLeft = 100;
    [SerializeField] private TMP_Text textThing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textThing.text = "Resource Left: " + resourceLeft;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Team 1 Worker" || other.gameObject.tag == "Team 2 Worker")
        {
            if (other.GetComponent<Worker>().GetCarry() == 0)
            {
                other.GetComponent<Worker>().SetCarry(1);
                resourceLeft--;
            }
        }
    }
}
