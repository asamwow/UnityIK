using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClawController : MonoBehaviour {

	[SerializeField]
	Transform[] fingerClosedPositions;

	[SerializeField]
	Transform[] fingerOpenPositions;

	[SerializeField]
	JointSystem[] fingers;

	const int fingerCount = 3;

	public IEnumerator Open() {
		for (int i = 0; i < fingerCount; i++) {
			fingers[i].setTarget(fingerOpenPositions[i]);
		}
		while (!fingers[0].done || !fingers[1].done || !fingers[2].done) {
			yield return null;
		}
	}

	public IEnumerator Close() {
		for (int i = 0; i < fingerCount; i++) {
			fingers[i].setTarget(fingerClosedPositions[i]);
		}
		while (!fingers[0].done || !fingers[1].done || !fingers[2].done) {
			yield return null;
		}
	}
}
