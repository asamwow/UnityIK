using System;
using UnityEngine;

public class GameController : MonoBehaviour {

    public void Start() {
        Vector3[] joints = {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 2)
        };
        Vector3[] newJointPositions = IK.InverseTransform(joints, new Vector3(0, 2, 0));
        for (int i = 0; i < newJointPositions.Length; i++) {
            Debug.Log(newJointPositions[i]);
        }
    }
}