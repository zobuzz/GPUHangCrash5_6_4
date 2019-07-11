using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCanvas : MonoBehaviour {


    public GameObject m_prefab;

    private GameObject instantiate = null;
	// Use this for initialization
	void Start () 
    {
		
	}
    float timer = 0;
    float interval = 1;
	// Update is called once per frame
	void Update () 
    {
        timer += Time.smoothDeltaTime;

        if(timer > interval)
        {
            if(null != m_prefab)
            {
                Destroy(instantiate);
            }
            instantiate = Instantiate(m_prefab);
            instantiate.name = "Prefab" + Time.frameCount;
            timer = 0;
            interval = Random.Range(0,1000);
            interval /= 1000.0f;

            //if(interval < 1)
            //{
            //    interval = 0.001f;
            //}
        }
    }
}
