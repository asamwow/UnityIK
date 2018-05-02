using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCloth : MonoBehaviour {

    public float meshWidth = 2f;
    public float meshHeight = 2f;
    public const int verticesOnRow = 5;
    public const int verticesOnColm = 5; 

    public Vector3[] newVertices;

    public Vector3[] newNormals;
    public int[] newTriangles;

    public struct ClothSpring
    {
        public int P1;
        public int P2;

        public float _NaturalLenght;
        public float _InverseLength;

        public float _Stiffness;
        
        public ClothSpring(int PID1, int PID2, float Len, float Stiffness)
        {
            this.P1 = PID1;
            this.P2 = PID2;
            this._NaturalLenght = Len;
            this._InverseLength = 1.0f / Len;
            this._Stiffness = Stiffness;
        }
    }

    public struct ClothParticle
    {
        public Vector3 currentPosition;
        public Vector3 currentVelocity;

        public Vector3 nextPosition;
        public Vector3 nextVelocity;

        public Vector3 tension;
        public float inverseMass;

        public bool pinned;
    }

    public struct ClothCollider
    {
        public Vector3 Position;
        public float Radius;

        public ClothCollider(Vector3 position, float radius)
        {
            this.Position = position;
            this.Radius = radius;
        }
    }

    private const int SimScale = 1;
    private const float minimumPhysicsDelta = 0.01f;

    private const float clothScale = 20.0f;

    private float StretchStiffness;
    private float BendStiffness;

    private float mass = 0.01f * SimScale;

    private float dampFactor = 0.9f;

    public const int gridSize = verticesOnRow;

    private ClothSpring[] _springs;
    private ClothParticle[] _particles;

    private float _timeSinceLastUpdate;

    private Vector3 _gravity;

    private List<ClothCollider> _colliders = new List<ClothCollider>();



    // Use this for initialization
    void Start()
    {
        _gravity = new Vector3(0, -0.98f * SimScale, 0);

        Mesh mesh = new Mesh();

        //Cloth Simulation stuff
        StretchStiffness = 2.5f * meshWidth;
        BendStiffness = 1.0f * meshWidth;

        //sets up the correct number of spots to have based on how many vertices declared
        int totalVertices = verticesOnRow * verticesOnColm;
        //calculate number of springs being connected to the right of each point
        int springCount = (gridSize - 1) * gridSize * 2;
        //adds the diagonal springs to the spring amount amount
        springCount += (gridSize - 1) * (gridSize - 1) * 2;
        //adding one past the neighbor
        springCount += (gridSize - 2) * gridSize * 2;

        _particles = new ClothParticle[totalVertices];
        _springs = new ClothSpring[springCount];

        newVertices = new Vector3[totalVertices];
        /*
        //creates the vertices at the set interval 
        int index = 0;
        for (int i = 0; i < verticesOnColm; i++)
        {
            for (int j = 0; j < verticesOnRow; j++)
            {
                newVertices[index] = new Vector3(i * (meshWidth / (verticesOnRow - 1)), j * (meshHeight / (verticesOnColm - 1)), 0);
                index += 1;
            }
        }
         * UV stuff as 2Dvecs
         * v(0,1)  v(1,1)
         * 6---7---8
         * |\  |\  |
         * | \ | \ |
         * |  \|  \|
         * 3---4---5
         * |\  |\  |
         * | \ | \ |
         * |  \|  \|
         * 0---1---2
         * ^(0,0)  ^(1,0)
         * UV stuff as 2Dvecs
         * 
         * 8 total triangles in 3X3

        //we do not need to do the triangles above the top row
        //we start in the bottom at 0

        int rowIndex = 0;
        int columnIndex = 0;

        float totalTriangles = 2 * (3 * verticesOnRow * verticesOnColm);
        newTriangles = new int[Mathf.RoundToInt(totalTriangles)];

        int triangleCount = 0;

        //need to print the start of the next triangle which is previous + 1
        //then go i row up so + verticesOnRow
        //then +1 to the first spot
        //Uses vertices in a clockwise direction
        while (triangleCount < totalTriangles)
        {
            if (columnIndex != verticesOnColm - 1 && rowIndex != verticesOnRow)
            {
                 * o 
                 * |\
                 * | \
                 * |  \
                 * o---o
                 *
                //spot next in line
                newTriangles[triangleCount] = (rowIndex * verticesOnRow) + (columnIndex);
                triangleCount += 1;
                //spot a row above
                newTriangles[triangleCount] = ((rowIndex + 1) * verticesOnRow) + (columnIndex);
                triangleCount += 1;
                //spot at current + 1
                newTriangles[triangleCount] = (rowIndex * verticesOnRow) + (columnIndex + 1);
                triangleCount += 1;

                 * o---o 
                 *  \  |
                 *   \ |
                 *    \|
                 *     o
                 *
                //current row + 1
                newTriangles[triangleCount] = ((rowIndex + 1) * verticesOnRow) + (columnIndex);
                triangleCount += 1;
                //current row + 1 & current column + 1
                newTriangles[triangleCount] = ((rowIndex + 1) * verticesOnRow) + (columnIndex + 1);
                triangleCount += 1;
                //current spot on row + 1
                newTriangles[triangleCount] = (rowIndex * verticesOnRow) + (columnIndex + 1);
                triangleCount += 1;
            }
            //go row by row to creat the triangles we want
            columnIndex += 1;
            //if we reach the end of the row when incrementing then we reset the columnIndex and start on the next row
            if (columnIndex == verticesOnColm)
            {
                columnIndex = 0;
                if (rowIndex + 1 != verticesOnRow - 1)
                {
                    rowIndex += 1;
                }
            }


        }

        //newTriangles = new int[]
        //{
        //    0,2,1,
        //    2,3,1
        //};

        //Calculating the normals of each vertex
        newNormals = new Vector3[newVertices.Length];

        for (int i = 0; i < newVertices.Length; i++)
        {
            newNormals[i] = Vector3.forward;
        }

        mesh.Clear();

        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.triangles = newTriangles;
        */

        initMesh();
    }

    private void initMesh()
    {
        //sets up an array of ints telling to be used in creating the triangles of the mesh
        int triCount = (gridSize * gridSize) * 2;
        newTriangles = new int[triCount];

        //sets an array of Vector3's that are the vertices of the mesh
        int vertCount = (gridSize * gridSize);
        newVertices = new Vector3[vertCount];

        //loop that goes 
        int k = 0;
        for(int j = 0; j < gridSize - 1; j++)
        {
            for(int i = 0; i < gridSize - 1; i++)
            {
                var i0 = j * gridSize + i;
                var i1 = j * gridSize + i + 1;
                var i2 = (j + 1) * gridSize + i;
                var i3 = (j + 1) * gridSize + i + 1;

                newTriangles[k] = i2;
                newTriangles[k + 1] = i1;
                newTriangles[k + 2] = i0;

                newTriangles[k + 3] = i2;
                newTriangles[k + 4] = i3;
                newTriangles[k + 5] = i1;

                k += 6;
            }
        }
    }

    private void reset()
    {
        //inits the vertices the particles in an even spaced grid
        for(int j = 0; j<gridSize; j++)
        {
            for(int i = 0; i<gridSize; i++)
            {
                float U = (i / (float)(gridSize - 1)) - 0.5f;
                float V = (j / (float)(gridSize - 1)) - 0.5f;

                int BallID = j * gridSize + i;
                _particles[BallID].currentPosition = new Vector3((float)clothScale * U, 8.5f, (float)clothScale * V);
                _particles[BallID].currentVelocity = Vector3.zero;

                _particles[BallID].inverseMass = 1.0f / mass;
                _particles[BallID].pinned = false;

                _particles[BallID].tension = Vector3.zero;
            }
        }

        float naturalLengthVec = Vector3.Distance(_particles[0].currentPosition, _particles[1].currentPosition);

        //Pinned the corners
        _particles[0].pinned = true;
        _particles[gridSize - 1].pinned = true;

        /*
         * _particles[gridSize * (gridSize -1)].pinned = true;
         * _particles[gridSize * gridSize - 1].pinned = true;
         * 
         */

        int currentSpring = 0;

        //Connecting the springs that are next to each other except the points on the edge of row
        for(int j = 0; j<gridSize; j++)
        {
            for(int i=0; i<gridSize; i++)
            {
                _springs[currentSpring] = new ClothSpring(j * gridSize + i, j * gridSize + 1, naturalLengthVec, StretchStiffness);
                currentSpring++;
            }
        }
    }
    //// Update is called once per frame
    //void Update () {
    //       Mesh mesh = GetComponent<MeshFilter>().mesh;
    //       Vector3[] vertices = mesh.vertices;
    //       Vector3[] normals = mesh.normals;
    //       int i = 0;
    //       while(i < vertices.Length)
    //       {
    //           vertices[i] += normals[i] * Mathf.Sin(Time.time);
    //           i++;
    //       }
    //       mesh.vertices = vertices;
    //}

    //Need to create seperate points for out in the game field and then use that in creating the mesh
    //Triangles work with what spot in the array it is using from the other arrays
}
