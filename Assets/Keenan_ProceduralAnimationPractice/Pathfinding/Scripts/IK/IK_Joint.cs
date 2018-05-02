using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Joint {
    public delegate void JointCallback(IK_Joint joint);
    public enum JointTypeEnum { Joint, Fork, Root, End }
    public static int joint_id = 0;

    public int id = 0;
    public JointTypeEnum jointType = JointTypeEnum.Joint;

    public float distanceToParent = 0.0f;
    public int distanceToRoot = 0;
    public int distanceFromLastFork = 0;

    public IK_Joint parentJoint;
    public List<IK_Joint> childrenJoints = new List<IK_Joint>();

    public Vector3 worldPosition = Vector3.zero;
    public Vector3 target = Vector3.zero;
    public bool seekingGoal = false;

    public int childCount = 0;
    public int childIndex = 0;
    public int limbIndex = 0;

    public IK_Joint(IK_Joint _parentJoint, float _distanceToParent = 0) {
        id = joint_id;
        joint_id++;

        parentJoint = _parentJoint;
        if (_parentJoint == null) // If there is no parent (null), return early
            return;

        distanceToParent = _distanceToParent;
        distanceToRoot = _parentJoint.distanceToRoot + 1;

        _parentJoint.AddChild(this);
        childIndex = _parentJoint.childrenJoints.Count - 1;
        limbIndex = _parentJoint.limbIndex + 1;

        ForAllChildren(_parentJoint, (IK_Joint joint) => {
            joint.UpdateJointDetails();
        });
    }


    // -------------------------------------
    // SET THE TARGET FOR THE JOINT         \
    // ---------------------------------------
    public void SetTarget(Vector3 _target) {
        target = _target;
        seekingGoal = true;
    }

    // -------------------------------------
    // REMOVE THE TARGET FOR THE JOINT      \
    // ---------------------------------------
    public void RemoveTarget() {
        target = Vector3.zero;
        seekingGoal = false;
    }

    public void UpdateJointDetails() {
        if (parentJoint == null) {
            jointType = JointTypeEnum.Root;
            distanceFromLastFork = 0;
        }
        else {
            distanceFromLastFork = (parentJoint.jointType == JointTypeEnum.Fork) ? 1 : parentJoint.distanceFromLastFork + 1;
            if (childCount == 0) {
                jointType = JointTypeEnum.End;
            }
            else if (childCount == 1) {
                jointType = JointTypeEnum.Joint;
            }
            else {
                jointType = JointTypeEnum.Fork;
            }
        }
    }

    // -------------------------------------------------------------
    // UPDATE POSITION OF CHILD JOINTS ...                          \
    // ---------------------------------------------------------------
    public void Update() {

    }

    // -------------------------------------
    // ADDS A CHILD TO THE JOINT            \
    // ---------------------------------------
    public void AddChild(IK_Joint newJoint) {
        childrenJoints.Add(newJoint);
        childCount++;
    }

    // -------------------------------------------------------------
    // GET AN ARRAY OF JOINTS FROM THE ROOT TO THE TARGET JOINT     \
    // ---------------------------------------------------------------
    public List<IK_Joint> GetFullPath() {
        List<IK_Joint> path = new List<IK_Joint>(new IK_Joint[limbIndex + 1]); // Size
        path[limbIndex] = this;

        IK_Joint currentJoint = parentJoint;
        while (currentJoint.parentJoint != null) {
            path[currentJoint.limbIndex] = currentJoint;
            currentJoint = currentJoint.parentJoint;
        }
        path[0] = currentJoint; // Add the root to the array
        return path;
    }

    // -------------------------------------------------------------
    // GET AN ARRAY OF JOINTS FROM A FORK TO THE TARGET JOINT       \
    // ---------------------------------------------------------------
    public List<IK_Joint> GetPathFromFork() {
        List<IK_Joint> path = new List<IK_Joint>(new IK_Joint[distanceFromLastFork + 1]);

        path[distanceFromLastFork] = this;
        if (this.jointType == JointTypeEnum.Root)
            return path;

        IK_Joint currentJoint = parentJoint;
        while (currentJoint.jointType != JointTypeEnum.Fork && currentJoint.jointType != JointTypeEnum.Root) {
            path[currentJoint.distanceFromLastFork] = currentJoint;
            currentJoint = currentJoint.parentJoint;
        }
        path[0] = currentJoint; // Add the fork/root to the array
        return path;
    }

    public static void PrintPath(List<IK_Joint> path) {
        string pathStr = "[";
        for (int i = 0; i < path.Count; i++) {
            pathStr += path[i].id + ((i != path.Count - 1) ? " -> " : "");
        } pathStr += "]";
        Debug.Log(pathStr);
    }

    public static void CopyOverJointList(List<IK_Joint> targetJoints, List<IK_Joint> otherJoints) {
        foreach (IK_Joint joint in otherJoints) {
            targetJoints.Add(joint);
        }
    }

    // A way to call a function for all joints branching off of a starting joint (including the starting joint)
    public static void ForAllChildren(IK_Joint startingJoint, JointCallback func) {
        if (startingJoint == null)
            return;

        func(startingJoint); // Call function for first joint
        foreach (IK_Joint joint in startingJoint.childrenJoints) {
            func(joint);
            if (joint.childCount > 0) // If has children, call function on them too
                ForAllChildren(joint, func);
        }
    }

    // Calls a function for all end joints from a starting joint
    public static void ForAllEnds(IK_Joint startingJoint, JointCallback func) {
        if (startingJoint == null)
            return;

        if (startingJoint.jointType == JointTypeEnum.End) {
            func(startingJoint); // Call function for first joint
        }
        else { 
            foreach (IK_Joint joint in startingJoint.childrenJoints) {
                if (joint.jointType == JointTypeEnum.End) {
                    func(joint);
                }
                if (joint.childCount > 0) // If has children, call function on them too
                    ForAllEnds(joint, func);
            }
        }
    }

}
