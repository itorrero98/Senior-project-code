using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class pickupScript : MonoBehaviour {

    private Throwable throwableRef;
	// Use this for initialization
	void Start () {
        throwableRef = gameObject.GetComponent<Throwable>();
        throwableRef.onDetachFromHand.AddListener(GameObjectReleased);
	}

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Layer collided with: " + collision.gameObject.layer);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "PlayerHand")
        {
            gameObject.GetComponentInChildren<Animator>().SetBool("isWaiting", false);

        }
    }

    public void GameObjectReleased()
    {
            PlayerManager.m_Instance.PlayerCollectedPOI();
            Destroy(gameObject);

    }

    // Update is called once per frame
    void Update () {
		
	}
}
