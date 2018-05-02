using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Body : MonoBehaviour {

    public delegate void JointCallback(IK_Joint joint);
    public Transform target;
    public Transform sub_target_1;
    public Transform sub_target_2;
    public Grid grid;

    public int extraJoints = 4;
    public float segmentLength = 1.0f;
    

    private const float MARGIN_OF_ERROR = 0.1f; // ....... Error margin
    private const int ITERATION_COUNT = 10; // ........... Max iterations of FABRIK solving

    private IK_Joint root_1;
    private IK_Joint root_2;
    private IK_Joint fork_1;

    private IK_Joint left_1;
    private IK_Joint left_2;
    private IK_Joint left_3;
    private IK_Joint left_end;

    private IK_Joint right_1;
    private IK_Joint right_end;

    // +=================================
    // | EXAMPLE DIAGRAM                 \
    // +===================================
    //      left_end  +
    //              |
    //      left_3  +
    //              |
    //      left_2  +   +  right_end
    //              |   |
    //      left_1  +   +  right_1
    //               \ /
    //      fork_1    +
    //                |
    //                +  root_2
    //                |
    //                +  root_1
    // 
    // +===================================

    private List<IK_Segment> segments = new List<IK_Segment>();

    private List<IK_Joint> joints = new List<IK_Joint>();
    private IK_Joint endJoint;
    private IK_Joint rootJoint;

    void Awake() {

    }

    // Use this for initialization
    void Start () {
        //// Create Segments
        //segments.Add(new IK_Segment(Vector3.zero, 2));
        //segments.Add(new IK_Segment(Vector3.zero, 2));
        //segments.Add(new IK_Segment(Vector3.zero, 2));
        //// Connect
        //segments[0].AddSegment(segments[1]);
        //segments[0].AddSegment(segments[2]);

        //root_1 = new IK_Joint(null, segmentLength);
        //root_2 = new IK_Joint(root_1, segmentLength);
        //fork_1 = new IK_Joint(root_2, segmentLength);

        //left_1 = new IK_Joint(fork_1, segmentLength);
        //left_2 = new IK_Joint(left_1, segmentLength);
        //left_3 = new IK_Joint(left_2, segmentLength);
        //left_end = new IK_Joint(left_3, segmentLength);

        //right_1 = new IK_Joint(fork_1, segmentLength);
        //right_end = new IK_Joint(right_1, segmentLength);

        // Generated Joints
        joints.Add(new IK_Joint(null, segmentLength));
        for (int i = 0; i < extraJoints; i++) {
            joints.Add(new IK_Joint(joints[joints.Count - 1], segmentLength));
        }
        endJoint = joints[joints.Count - 1];
        rootJoint = joints[0];
    }

    void JointPathfinding() {
        int turnsFound = 0;
        IK_Joint previousTargetedJoint = null;
        Vector3 previousTurnPos = transform.position;

        Pathfinding.ForEachTurn(grid.path, (Vector2 turnPos, Vector2 previousDir, Vector2 currentDir) => {
            float distanceToTurn = 0;
            float distanceToJoint = 0;
            int targetJointIndex = 0;

            //if (turnsFound == 0) {
            distanceToTurn = ((Vector3)turnPos - previousTurnPos).magnitude;

            // Find joint that can reach turn
            for (int i = 0; i < joints.Count; i++) {
                distanceToJoint = i * segmentLength;
                if (distanceToJoint >= distanceToTurn) {
                    targetJointIndex = i;
                    break;
                }
            }

            print("Turn_" + turnsFound + " -> Dst: " + distanceToTurn + ", DstJt: " + distanceToJoint + ", Joint#: " + targetJointIndex);
            joints[targetJointIndex].SetTarget(turnPos);

            //}

            Solve(joints[targetJointIndex], false);
            turnsFound++;
            previousTurnPos = (Vector3)turnPos;
        });
    }

    // Update is called once per frame
    void Update () {
        //if (Input.GetKeyDown(KeyCode.Space)) 
        // ...

        //MultiJointSolve();
        //endJoint.SetTarget(target.position);
        //left_2.SetTarget(target2.position);

        //Solve(left_2, false);
        //Solve(endJoint, false);

        //endJoint.SetTarget(target.position);
        //Solve(endJoint, false);

        //JointPathfinding();
    }


    void OnDrawGizmos() {
        //IK_Joint.PrintPath(endJoint.GetFullPath()); // joints[joints.Count - 1]
        ForAllJoints(rootJoint, (IK_Joint joint) => {
            //print("JOINT_" + joint.id + ": " + joint.worldPosition);
            Gizmos.color = Color.yellow;
            if (joint.parentJoint != null) {
                Gizmos.DrawLine(joint.worldPosition, joint.parentJoint.worldPosition);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(joint.worldPosition, 0.1f);
        });

        //foreach (IK_Segment seg in segments) {
        //    seg.ForEachJoint((IK_Joint joint) => {
        //        Gizmos.color = Color.yellow;
        //        if (joint.parentJoint != null) {
        //            Gizmos.DrawLine(joint.worldPosition, joint.parentJoint.worldPosition);
        //        }

        //        Gizmos.color = Color.blue;
        //        Gizmos.DrawSphere(joint.worldPosition, 0.1f);
        //    });
        //}
    }

    // A way to call a function for all joints branching off of a starting joint (including the starting joint)
    public void ForAllJoints(IK_Joint startingJoint, JointCallback func) {
        if (startingJoint == null)
            return;

        func(startingJoint); // Call function for first joint
        foreach (IK_Joint joint in startingJoint.childrenJoints) {
            func(joint);
            if (joint.childCount > 0) // If has children, call function on them too
                ForAllJoints(joint, func);
        }
    }

    public void MultiJointSolve_NEW() {
        List<IK_Joint> chainRoot = segments[0].joints;
        List<IK_Joint> chain1 = segments[1].joints;
        List<IK_Joint> chain2 = segments[2].joints;

        Vector3 chainRoot_origin = chainRoot[0].worldPosition;
        //left_end.target = target1.position;
        //right_end.target = target2.position;

        // Work backwards to fork
        //BackwardsAdjust(chain1, target1.position);
        //BackwardsAdjust(chain2, target2.position);

        Vector3 center = (chain1[1].worldPosition + chain2[1].worldPosition) / 2;
        fork_1.target = center;
        Solve(fork_1, false);

        Vector3 chain1_origin = chain1[0].worldPosition;
        Vector3 chain2_origin = chain2[0].worldPosition;

        // Work forwards from fork
        ForwardsAdjust(chain1, chain1_origin);
        ForwardsAdjust(chain2, chain2_origin);
    }

    public void MultiJointSolve() {
        List<IK_Joint> chainRoot = fork_1.GetPathFromFork();
        List<IK_Joint> chain1 = left_end.GetPathFromFork();
        List<IK_Joint> chain2 = right_end.GetPathFromFork();

        Vector3 chainRoot_origin = chainRoot[0].worldPosition;
        //left_end.target = target1.position;
        //right_end.target = target2.position;

        // Work backwards to fork
        //BackwardsAdjust(chain1, target1.position);
        //BackwardsAdjust(chain2, target2.position);

        Vector3 center = (chain1[1].worldPosition + chain2[1].worldPosition) / 2;
        fork_1.target = center;
        Solve(fork_1, false);

        Vector3 chain1_origin = chain1[0].worldPosition;
        Vector3 chain2_origin = chain2[0].worldPosition;

        // Work forwards from fork
        ForwardsAdjust(chain1, chain1_origin);
        ForwardsAdjust(chain2, chain2_origin);
    }

    
    public void Solve(IK_Joint endPoint, bool requiredInRange) {
        Vector3 target = endPoint.target;
        int iterationNum = 0;
        float error = 0;

        List<IK_Joint> path = endPoint.GetPathFromFork();

        // Check if can reach
        if (requiredInRange) { 
            float totalLimbDistance = 0;
            float requiredDistance = 0;
            foreach (IK_Joint joint in path) {
                totalLimbDistance += joint.distanceToParent;
                if (joint.jointType == IK_Joint.JointTypeEnum.Root)
                    requiredDistance = (joint.worldPosition - target).magnitude;
            }
            if (totalLimbDistance < requiredDistance)
                return;
        }

        // Start adjusting the positions of joints
        do { 
            Vector3 originPos = path[0].worldPosition; // Used later

            BackwardsAdjust(path, target);      // Backwards through the joints
            ForwardsAdjust(path, originPos);    // Forwards through the joints

            error = (path[path.Count - 1].worldPosition - target).magnitude;
            iterationNum++;
        } while (iterationNum < ITERATION_COUNT && (error > MARGIN_OF_ERROR && iterationNum != 0));
        // While iteration num is below ITERATION_COUNT and the MARGIN_OF_ERROR hasn't been reached yet...

    }

    public void BackwardsAdjust(List<IK_Joint> path, Vector3 target) {
        path[path.Count - 1].worldPosition = target;
        Vector3 anchor = target;
        for (int i = path.Count - 2; i >= 0; i--) {
            path[i].worldPosition = anchor + (path[i].worldPosition - anchor).normalized * path[i + 1].distanceToParent;
            anchor = path[i].worldPosition;
        }
    }

    public void ForwardsAdjust(List<IK_Joint> path, Vector3 initialStart) {
        path[0].worldPosition = initialStart;
        Vector3 anchor = initialStart;
        for (int i = 1; i < path.Count; i++) {
            path[i].worldPosition = anchor + (path[i].worldPosition - anchor).normalized * path[i].distanceToParent;
            anchor = path[i].worldPosition;
        }
    }





}
