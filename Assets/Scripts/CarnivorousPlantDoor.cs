using UnityEngine;



public class CarnivorousPlantDoor : MonoBehaviour

{

    private void OnTriggerEnter(Collider other)

    {

        // Check if the object entering is a bug 

        if (other.CompareTag("Bug"))

        {

            // Check if the bug has the Bug component 

            Bug bug = other.GetComponent<Bug>();



            if (bug != null)

            {

                if (!bug.isBig)

                {

                    Debug.Log("The plant sniffs the bug but stays shut. Maybe it wants something bigger...");

                }

                else

                {

                    Debug.Log("The plant chomps down the big bug and opens wide!");

                    Destroy(other.gameObject); // Remove the bug 

                    OpenDoor();

                }

            }

        }

    }



    void OpenDoor()

    {

        // Add your door opening logic here (animation, enabling passage, etc.) 

        Debug.Log("The door is now open.");

        GetComponent<Collider>().enabled = false; // Disable the collider to let the player pass 

    }

}