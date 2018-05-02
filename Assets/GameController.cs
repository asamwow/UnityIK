using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	[SerializeField]
	Transform startingHandLocation;

	[SerializeField]
	Transform ballGrabHandLocation;

	[SerializeField]
	Transform ballDropHandLocation;

	[SerializeField]
	Transform ball;

	[SerializeField]
	JointSystem mainArm;
	
	[SerializeField]
	ClawController claw;

	void Start() {
		StartCoroutine(controlArm());
		// StartCoroutine(testClaw());
	}

	IEnumerator testClaw() {
		yield return StartCoroutine(claw.Open());
		yield return StartCoroutine(claw.Close());
				yield return StartCoroutine(claw.Open());
		yield return StartCoroutine(claw.Close());
				yield return StartCoroutine(claw.Open());
		yield return StartCoroutine(claw.Close());
	}

	IEnumerator controlArm() {
		Rigidbody ballRigidBody = ball.GetComponent<Rigidbody>();
		mainArm.setTarget(startingHandLocation);
		while (!mainArm.done) {
			yield return null;
		}
		yield return StartCoroutine(claw.Open());
		yield return new WaitForSeconds(0.1f);
		mainArm.setTarget(ballGrabHandLocation);
		while (!mainArm.done) {
			yield return null;
		}
		yield return StartCoroutine(claw.Close());
		ball.transform.parent = claw.transform;
		ballRigidBody.constraints = RigidbodyConstraints.FreezeAll;
		yield return new WaitForSeconds(0.5f);
		mainArm.setTarget(ballDropHandLocation);
		while (!mainArm.done) {
			yield return null;
		}		
		ball.transform.parent = null;
		ballRigidBody.constraints = RigidbodyConstraints.None;
		yield return StartCoroutine(claw.Open());
	}
}
