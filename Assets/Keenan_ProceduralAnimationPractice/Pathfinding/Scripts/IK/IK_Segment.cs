using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Segment {
    public delegate void JointCallback(IK_Joint joint);
    private const float MARGIN_OF_ERROR = 0.1f; // ....... Error margin
    private const int ITERATION_COUNT = 10; // ........... Max iterations of FABRIK solving

    public float jointSpacing = 1.0f;

    public int jointCount = 1;
    public IK_Joint root;
    public IK_Joint end;
    public List<IK_Joint> joints = new List<IK_Joint>();

    public IK_Segment parentSegment;
    public List<IK_Segment> childrenSegments = new List<IK_Segment>();

    public IK_Segment(Vector3 initialPos, int extraInitialJoints = 0) {
        root = new IK_Joint(null);
        root.worldPosition = initialPos;
        end = root;
        joints.Add(root);

        for (int i = 0; i < extraInitialJoints; i++)
            AddJoint();
	}

    public void AddJoint() {
        IK_Joint newJoint = new IK_Joint(joints[joints.Count - 1], jointSpacing);
        //newJoint.worldPosition = root.worldPosition + (Vector3.right * jointCount * jointSpacing); // Initialize position
        end = newJoint;
        joints.Add(newJoint);
        jointCount++;
    }

    // Automatically updates the details of joints according to the list of joints
    public void UpdateJointDetailsInList() {
        IK_Joint parentSegmentJointConnector = joints[0].parentJoint;
        IK_Joint[] childrenSegmentJointConnectors = new IK_Joint[childrenSegments.Count];
        for (int i = 0; i < childrenSegments.Count; i++) {
            childrenSegmentJointConnectors[i] = childrenSegments[i].joints[0];
        }


        // Set up connections with root joint

        for (int i = 0; i < joints.Count; i++) {

        }

    }

    public void AddSegment(IK_Segment existingSegment) {
        end.AddChild(existingSegment.root);
        childrenSegments.Add(existingSegment);

        existingSegment.joints.Insert(0, end);
        existingSegment.joints[1].parentJoint = end; // Old root
        existingSegment.root = end;
        existingSegment.parentSegment = existingSegment;


        end.UpdateJointDetails();
        existingSegment.root.parentJoint.UpdateJointDetails();
    }

    public void ForEachJoint(JointCallback func) {
        for (int i = 0; i < joints.Count; i++) {
            func(joints[i]);
        }
    }
    
}
