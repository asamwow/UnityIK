using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointSystem : MonoBehaviour {

	// [SerializeField]
	// Vector3 target;

	[SerializeField]
	Transform target;

	[SerializeField]
	Transform[] joints;

	[SerializeField]
	float[] jointDistances;

	[SerializeField]
	float[] jointConstraints;

	Coroutine IKCoroutine = null;

	void Start() {
		if (jointDistances.Length != joints.Length - 1) {
			jointDistances = new float[joints.Length-1];
			for (int i = 0; i < joints.Length - 1; i++) {
				jointDistances[i] = Vector3.Distance(joints[i].position, joints[i+1].position);
			}
		}
	}

	void Update() {
		// Debug.Log(IKCoroutine);
		if (IKCoroutine != null) {
			// StopCoroutine(IKCoroutine);
			return;
		}
		
		IKCoroutine = StartCoroutine(IterateIK());
	}

	IEnumerator IterateIK(float speed = 1f) {
		// Check for malformed parameters
		if (joints.Length == 0) {
			Debug.LogError("No Joint Positions passed into inverse kinomatic function.");
			yield break;
		}
		if (jointDistances.Length != joints.Length - 1) {
			Debug.LogError("Incorrect number of parameters passed into inverse kinomatic function.");
			yield break;
		}

		int jointCount = joints.Length;

		// Find the max span of the model
		float maxSpan = 0f;
		foreach (float jointDistance in jointDistances) {
			maxSpan += jointDistance;
		}

		float targetSpan = Vector3.Distance(joints[0].position, target.position);
		
		// If target distance is out of reach of the model
		if (maxSpan < targetSpan) {
			//TODO
			Debug.LogWarning("TODO: find closest point to target that is within attainable span");
			yield break;
			// or do this?
			// // Distance from joint positions and the final target
			// float[] targetDistances = new float[jointCount];
			// float[] jointTargetFraction = new float [jointCount];
			// for (int i = 0; i < jointCount-1; i++) {
			// 	targetDistances[i] = Vector3.Distance(joints[i], target);
			// 	jointTargetFraction[i] = jointDistances[i] / targetDistances[i];
			// 	// Estimate joint position by linearly interpolating based on how much of the distance to target can be coverd by this joint
			// 	destinationPositions[i+1] = Vector3.Lerp(joints[i], target, jointTargetFraction[i]);
			// }
		}

		Vector3[] jointPositions = new Vector3[joints.Length];

		for (int i = 0; i < jointCount; i++) {
			jointPositions[i] = joints[i].position;
		}

		int iterations = 0; // for debuging
		while (Vector3.Distance(jointPositions[jointCount-1], target.position) > 0.1f) {
			// First, set last joint position at target
			jointPositions[jointCount-1] = target.position;
			// create a line between the current joint and the previous one, move the previous one along that line until it is at desired distance
			for (int i = jointCount-1; i > 0; i--) {
				Vector3 difference = jointPositions[i-1] - jointPositions[i];
				Vector3 previousDirection;
				if (i == jointCount-1) {
					previousDirection = difference;
				} else {
					previousDirection = jointPositions[i] - jointPositions[i+1];
				}
				float angle = Vector3.Angle(previousDirection, difference);
				if (angle > jointConstraints[i-1] || angle < -jointConstraints[i-1]) {
					Debug.Log("Joint " + i + " out of constraint at angle " + angle);
					// Quaternion.AxisAngle()
					Quaternion jointRotation = Quaternion.AngleAxis(jointConstraints[i-1], Vector3.Cross(previousDirection, difference));
					jointPositions[i-1] = jointRotation * previousDirection.normalized + jointPositions[i];
					// Debug.Log(Vector3.Angle(jointPositions[i] - jointPositions[i-1], jointPositions[i+1] - jointPositions[i]));
					// Debug.Log(jointRotation * Vector3.forward);
				} else {
					jointPositions[i-1] = difference.normalized * jointDistances[i-1] + jointPositions[i];
				}
			}
			// Set base joint to original location
			jointPositions[0] = Vector3.zero; //TODO replace this with original joint location
			for (int i = 0; i < jointCount - 1; i++) {
				Vector3 difference = jointPositions[i+1] - jointPositions[i];
				Vector3 previousDirection;
				if (i == 0) {
					previousDirection = Vector3.forward;
				} else {
					previousDirection = jointPositions[i] - jointPositions[i-1];
				}
				float angle = Vector3.Angle(previousDirection, difference);
				if (angle > jointConstraints[i] || angle < -jointConstraints[i]) {
					Debug.Log("Joint " + i + " out of constraint at angle " + angle);
					// Quaternion.AxisAngle()
					Quaternion jointRotation = Quaternion.AngleAxis(jointConstraints[i], Vector3.Cross(previousDirection, difference));
					jointPositions[i+1] = jointRotation * previousDirection.normalized + jointPositions[i];
					// Debug.Log(Vector3.Angle(jointPositions[i] - jointPositions[i-1], jointPositions[i+1] - jointPositions[i]));
					// Debug.Log(jointRotation * Vector3.forward);
				} else {
					jointPositions[i+1] = difference.normalized * jointDistances[i] + jointPositions[i];
				}
			}
			// DEBUG CODE
			iterations++;
			if (iterations > 1000) {
				Debug.LogError("infinite loop");
				yield break;
			}
		}

		Coroutine[] rotationCoroutines = new Coroutine[joints.Length-1];

		for (int i = 0; i < joints.Length - 1; i++) {
			rotationCoroutines[i] = StartCoroutine(RotateJoint(joints[i], jointPositions[i+1], jointPositions[i]));
			// Debug.Log(jointPositions[i]);
		}

		for (int i = 0; i < joints.Length - 1; i++) {
			yield return rotationCoroutines[i];
		}
		// Debug.Log("Coroutine set to null");
		IKCoroutine = null;
	}

	// IEnumerator RotateJoint(Transform rotatingJoint, Transform targetJoint, Vector3 target, float speed = 1f) {
	// 	Vector3 localTarget = target - rotatingJoint.position;
	// 	//TODO make correct rotation
	// 	float rotation = Mathf.Acos(localTarget.x) / Mathf.PI * 180;
	// 	float originalRotation = rotatingJoint.transform.eulerAngles.y;
	// 	float lerpIndex = 0f;
	// 	Vector3 newRotation = rotatingJoint.transform.eulerAngles;
	// 	while (Mathf.Abs(rotatingJoint.transform.eulerAngles.y - rotation) > 0.05f) {
	// 		newRotation.y = Mathf.Lerp(originalRotation, rotation, lerpIndex);
	// 		rotatingJoint.transform.rotation = Quaternion.Euler(newRotation);
	// 		lerpIndex += speed * Time.deltaTime;
	// 		yield return new WaitForEndOfFrame();
	// 	}
	// }

	// IEnumerator RotateJointSync(Transform rotatingJoint, Vector3 target, Vector3 previousTarget, float speed = 1f) {
	// 	Vector3 localTarget = target - previousTarget;
	// 	Quaternion targetRotation = Quaternion.LookRotation(localTarget);
	// 	float originalRotation = rotatingJoint.transform.eulerAngles.y;
	// 	Vector3 destinationRotation = rotatingJoint.transform.eulerAngles;
	// 	destinationRotation.y = rotation;
	// 	float lerpIndex = 0f;
	// 	Vector3 newRotation = rotatingJoint.transform.eulerAngles;
	// 	while (Mathf.DeltaAngle(newRotation.y, rotation) > 0.05f || Mathf.DeltaAngle(newRotation.y, rotation) < -0.05f) {
	// 		newRotation.y = Mathf.Lerp(originalRotation, rotation, lerpIndex);
	// 		rotatingJoint.transform.rotation = Quaternion.Euler(newRotation);
	// 		lerpIndex += speed * Time.deltaTime;
	// 		yield return new WaitForEndOfFrame();
	// 	}
	// 	Debug.Log("Coroutine ended");
	// 	// Debug.Log(Mathf.DeltaAngle(newRotation.y, rotation));

	// }

	IEnumerator RotateJoint(Transform rotatingJoint, Vector3 target, Vector3 previousTarget) {
		Vector3 localTarget = target - previousTarget;
		Quaternion targetRotation = Quaternion.LookRotation(localTarget);
		Quaternion originalRotation = rotatingJoint.rotation;
		// Debug.Log(targetRotation.eulerAngles);
		float lerpIndex = 0f;
		while (Quaternion.Angle(rotatingJoint.rotation, targetRotation) > 0.05f) {
			rotatingJoint.rotation = Quaternion.Lerp(originalRotation, targetRotation, lerpIndex);
			lerpIndex += Time.deltaTime;
			// Debug.Log(rotatingJoint.rotation);
			yield return new WaitForEndOfFrame();
		}
	}

	// static void LerpTransform(Transform transform, QuQuaternion targetRotation, Vector3 targetPosition, float lerpIndex) {
	// 	transform.position = Vector3.Lerp()
	// }

	// IEnumerator RotateJointSync(Transform rotatingJoint, Vector3 target, Vector3 previousTarget, float speed = 1f) {
	// 	Vector3 localTarget = target - previousTarget;
	// 	float rotation = Mathf.Acos(localTarget.x) / Mathf.PI * 180;
	// 	if (localTarget.z > 0) {
	// 		rotation *= -1f;
	// 	}
	// 	float originalRotation = rotatingJoint.transform.eulerAngles.y;
	// 	Vector3 destinationRotation = rotatingJoint.transform.eulerAngles;
	// 	destinationRotation.y = rotation;
	// 	float lerpIndex = 0f;
	// 	Vector3 newRotation = rotatingJoint.transform.eulerAngles;
	// 	while (Mathf.DeltaAngle(newRotation.y, rotation) > 0.05f || Mathf.DeltaAngle(newRotation.y, rotation) < -0.05f) {
	// 		newRotation.y = Mathf.Lerp(originalRotation, rotation, lerpIndex);
	// 		rotatingJoint.transform.rotation = Quaternion.Euler(newRotation);
	// 		lerpIndex += speed * Time.deltaTime;
	// 		yield return new WaitForEndOfFrame();
	// 	}
	// 	Debug.Log("Coroutine ended");
	// 	// Debug.Log(Mathf.DeltaAngle(newRotation.y, rotation));

	// }
}
