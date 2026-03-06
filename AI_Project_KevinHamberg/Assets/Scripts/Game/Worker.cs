using UnityEngine;

public class Worker : MonoBehaviour
{
    [SerializeField] private int carry = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetCarry()
    {
        return carry;
    }

    public void SetCarry(int amount)
    {
        carry = amount;
    }
}
