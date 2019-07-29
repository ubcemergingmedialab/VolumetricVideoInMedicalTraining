using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsatingLight : MonoBehaviour {

    private Light myLight;
    private float maxIntensity = 5f;
    bool reachedMax = false;

	// Use this for initialization
	void Start () {
        myLight = GetComponent<Light>();
	}

    // Update is called once per frame
    void Update()
    {
      
        if (!reachedMax)
        {
            myLight.intensity += 0.04f;
            if (myLight.intensity >= maxIntensity)
            {
                reachedMax = true;
            }
        }
        else
        {
            myLight.intensity -= 0.04f;
            if (myLight.intensity <= 1)
            {
                reachedMax = false;
            }
        }
         
    }
}
