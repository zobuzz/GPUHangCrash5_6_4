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
    float interval = 2;
    // Update is called once per frame

    int writeCount = 0;
    int readCount = 0;

	void Update () 
    {
        if (RTCapture.S_CPState == E_CaptureState.E_WRITE)
        {
            WriteCache();
        }
        else if (RTCapture.S_CPState == E_CaptureState.E_READ)
        {
            ReadCache();
        }
    }

    void WriteCache()
    {
        if (writeCount > 20)
        {
            return;
        }
        timer += Time.smoothDeltaTime;

        if (timer > interval)
        {
            writeCount++;
            if (null != m_prefab)
            {
                Destroy(instantiate);
            }
            instantiate = Instantiate(m_prefab);
            instantiate.name = "Prefab" + Time.frameCount;
            timer = 0;

            //interval = Random.Range(0,1000);
            //interval /= 1000.0f;
        }
    }

    void ReadCache()
    {
        if (readCount > 20)
        {
            return;
        }
        timer += Time.smoothDeltaTime;

        if (timer > interval)
        {
            readCount++;
            if (null != m_prefab)
            {
                Destroy(instantiate);
            }
            instantiate = Instantiate(m_prefab);
            instantiate.name = "Prefab" + Time.frameCount;
            timer = 0;
        }
    }

    string BtnState = "Write Cache";
    private void OnGUI()
    {
        if( GUI.Button( new Rect( Screen.width / 2 - Screen.width/16, 0, Screen.width/8, Screen.height/8), BtnState) )
        {
            BtnState = "Read Cache";

            RTCapture.S_CPState = E_CaptureState.E_READ;
        }
    }
}
