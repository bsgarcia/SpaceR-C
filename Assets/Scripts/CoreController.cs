using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreController : MonoBehaviour
{
    private ParticleSystem.MainModule ps;
    private Vector3 pos;

    // Start is called before the first frame update
    void Start()
    {
         ps = GetComponent<ParticleSystem>().main;
         pos = transform.localPosition;
            

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("up"))
        {
            IncreaseSize();
        }
        else
        {
            DecreaseSize();
        }

    }

    void IncreaseSize()
    {
        Vector3 newPosition = pos;
        newPosition.z -= 3f;
        transform.localPosition = newPosition;
        ps.startSize = new ParticleSystem.MinMaxCurve(1.9f, 3f);       

    }

    void DecreaseSize()
    {
        ps.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);

        transform.localPosition = pos;


    }
}
