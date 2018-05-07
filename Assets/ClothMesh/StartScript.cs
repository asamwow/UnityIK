using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScript : MonoBehaviour {

    MeshCloth clothTest = new MeshCloth();
	// Use this for initialization
	void Start () {
        clothTest = gameObject.AddComponent(typeof(MeshCloth)) as MeshCloth;

        clothTest.addCollider(Vector3.zero, 7f);
    }
    int i = 0;
	// Update is called once per frame
	void FixedUpdate () {
        clothTest.Simulate();        
    }
}
