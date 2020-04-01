using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portalManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player triggered the portal");
        if(other.gameObject.tag == "PlayerHand")
        {
            MazeScript.m_Instance.PortalTransPlayer();
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
