using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitCollisionCheck : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.m_Instance.AllPOIsCollected)
        {
            GameManager.m_Instance.StopGameTimer();
        }
    }
}
