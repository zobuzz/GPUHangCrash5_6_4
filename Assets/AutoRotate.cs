using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour {

    public float m_speed = 75;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        Quaternion rot = transform.localRotation;
        rot = Quaternion.AngleAxis( Time.smoothDeltaTime * m_speed, Vector3.up) * rot;
        transform.localRotation = rot;

    }
}
