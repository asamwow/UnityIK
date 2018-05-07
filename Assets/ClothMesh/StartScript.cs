using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScript : MonoBehaviour {

    MeshCloth clothTest = new MeshCloth();

    GameObject testSphere;
    Transform sphereTrans;
    // Use this for initialization
    void Start () {
        testSphere = GameObject.Find("Sphere");
        sphereTrans = testSphere.transform;

        clothTest = gameObject.AddComponent(typeof(MeshCloth)) as MeshCloth;

        clothTest.addCollider(sphereTrans.position, sphereTrans.localScale.x / 2);
    }
    int i = 0;
	// Update is called once per frame
	void FixedUpdate () {

        clothTest.Simulate();        
    }
}
