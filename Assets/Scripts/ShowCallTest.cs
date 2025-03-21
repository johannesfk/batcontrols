using UnityEngine;

public class ShowCallTest : MonoBehaviour
{
    private int counter = 0;

    void Start()
    {
        Debug.Log("Start is being called");

    }

    void Update()
    {
        counter += 1;
        Debug.Log($"This is update {counter}");
    }
}
