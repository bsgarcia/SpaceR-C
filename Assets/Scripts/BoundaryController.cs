using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryController : MonoBehaviour
{
    private GameController gameController;

    void Start () 
    {

        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
    
    }

	void OnTriggerExit(Collider other)
    {
        if (tag == "BoundaryMissed")
        {  
            gameController.MissedTrial();
            gameController.AllowWave(true);
            Destroy(other.gameObject);
        }

        if ((tag == "BoundaryShootable") &&  (other.tag == "Bolt"))
        {
            gameController.FadeAndDestroyOption(other.gameObject, 1f);

        }

    }   
        
}
