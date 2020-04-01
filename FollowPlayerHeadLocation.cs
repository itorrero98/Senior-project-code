using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerHeadLocation : MonoBehaviour {

    public Camera VRCam;

    private Transform bodyLoc;
    private Vector3 bodyYRotation;

	// Use this for initialization
	void Start () {
        bodyLoc = GetComponent<Transform>();
        bodyYRotation = bodyLoc.transform.rotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
        if (VRCam == null) return;

        bodyLoc.transform.position = OffsetBody();
        bodyYRotation = RotateWithHead();

        bodyLoc.transform.rotation = Quaternion.Euler(bodyYRotation);

        //bodyLoc.transform.rotation.y = 

	}

    private Vector3 OffsetBody()
    {
        Vector3 cameraPos = VRCam.transform.position;
        cameraPos.y -= .65f;

        return cameraPos;
    }

    private Vector3 RotateWithHead()
    {
        return new Vector3(0f, (VRCam.transform.localRotation.eulerAngles.y - 180), 0f);
    }
}
