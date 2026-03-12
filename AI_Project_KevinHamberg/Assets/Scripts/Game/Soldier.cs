using UnityEngine;

public class Soldier : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.tag == "Team 1 Soldier" && (other.tag == "Team 2 Soldier" || other.tag == "Team 2 Worker"))
        {
            Destroy(other.gameObject);
        }
        else if (this.tag == "Team 2 Soldier" && (other.tag == "Team 1 Soldier" || other.tag == "Team 1 Worker"))
        {
            Destroy(other.gameObject);
        }
    }
}
