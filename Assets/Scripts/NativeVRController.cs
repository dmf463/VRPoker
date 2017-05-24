using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR; //we need this for VR functions

public class NativeVRController : MonoBehaviour {

    public Transform leftHand, righthand;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        leftHand.localPosition = InputTracking.GetLocalPosition(VRNode.LeftHand);
        leftHand.localRotation = InputTracking.GetLocalRotation(VRNode.LeftHand);

        righthand.localPosition = InputTracking.GetLocalPosition(VRNode.RightHand);
        righthand.localRotation = InputTracking.GetLocalRotation(VRNode.RightHand);

	}
}
