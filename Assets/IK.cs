using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Source:  https://www.sciencedirect.com/science/article/pii/S1524070311000178?via%3Dihub
// Author: Sam

/// Inverse Kinomatic Standard Functions
public static class IK {

	/// Returns N transforms for N joint positions passed into the function
	/// the last element of joint positions is the "end effector" or joint with no children.
	public static Vector3[] InverseTransform (Vector3[] jointPositions, float[] jointDistances, Vector3 target) {
		// Check for malformed parameters
		if (jointPositions.Length == 0) {
			Debug.LogError("No Joint Positions passed into inverse kinomatic function.");
			return null;
		}
		if (jointDistances.Length != jointPositions.Length - 1) {
			Debug.LogError("Incorrect number of parameters passed into inverse kinomatic function.");
			return null;
		}

		int jointCount = jointPositions.Length;

		// Find the max span of the model
		float maxSpan = 0f;
		foreach (float jointDistance in jointDistances) {
			maxSpan += jointDistance;
		}

		float targetSpan = Vector3.Distance(jointPositions[0], target);
		
		// If target distance is out of reach of the model
		if (maxSpan < targetSpan) {
			//TODO
			Debug.LogWarning("TODO: find closest point to target that is within attainable span");
			return null;
			// or do this?
			// // Distance from joint positions and the final target
			// float[] targetDistances = new float[jointCount];
			// float[] jointTargetFraction = new float [jointCount];
			// for (int i = 0; i < jointCount-1; i++) {
			// 	targetDistances[i] = Vector3.Distance(jointPositions[i], target);
			// 	jointTargetFraction[i] = jointDistances[i] / targetDistances[i];
			// 	// Estimate joint position by linearly interpolating based on how much of the distance to target can be coverd by this joint
			// 	destinationPositions[i+1] = Vector3.Lerp(jointPositions[i], target, jointTargetFraction[i]);
			// }
		}
		int iterations = 0; // for debuging
		while (Vector3.Distance(jointPositions[jointCount-1], target) > 0.05f) {
			// First, set last joint position at target
			jointPositions[jointCount-1] = target;
			// create a line between the current joint and the previous one, move the previous one along that line until it is at desired distance
			for (int i = jointCount-1; i > 0; i--) {
				Vector3 difference = jointPositions[i-1] - jointPositions[i];
				jointPositions[i-1] = difference.normalized * jointDistances[i-1] + jointPositions[i];
			}
			// Set base joint to original location
			jointPositions[0] = Vector3.zero; //TODO replace this with original joint location
			for (int i = 0; i < jointCount - 1; i++) {
				Vector3 difference = jointPositions[i+1] - jointPositions[i];
				jointPositions[i+1] = difference.normalized * jointDistances[i] + jointPositions[i];
			}
			// DEBUG CODE
			iterations++;
			if (iterations > 1000) {
				Debug.LogError("infinite loop");
				break;
			}
		}
		return jointPositions;
	}

	/// Automatically calculates joint distances
	public static Vector3[] InverseTransform (Vector3[] jointPositions, Vector3 target) {
		float[] jointDistances = new float[jointPositions.Length-1];
		for (int i = 0; i < jointPositions.Length - 1; i++) {
			jointDistances[i] = Vector3.Distance(jointPositions[i], jointPositions[i+1]);
		}
		return InverseTransform(jointPositions, jointDistances, target);
	}
}
