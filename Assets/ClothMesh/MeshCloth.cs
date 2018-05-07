using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    public int A;
    public int B;
    public int C;
    public Vector3 normal;
}

public struct Vertex
{
    public Vector3 position;
    public Vector3 normal;
}

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


//Cloth class
public class MeshCloth : MonoBehaviour {

    GameObject sphereCol;
    Transform sphereTrans;

    private const int SimScale = 1;
    //original .01
    private const float minimumPhysicsDelta = 0.0075f;

    private const float clothScale = 15.0f;

    //original 2.5
    private float StretchStiffness = 2.5f * clothScale;
    //original 1.0
    private float BendStiffness = .1f * clothScale;

    //original .01
    private float mass = 0.01f * SimScale;

    //original .9
    private float dampFactor = .9f;

    //original 20
    public int gridSize = 20 * SimScale;

    private ClothSpring[] _springs;
    private ClothParticle[] _particles;

    private float _timeSinceLastUpdate;

    private Vector3 _gravity;

    private List<ClothCollider> _colliders = new List<ClothCollider>();

    private Vertex[] _vertices;
    public Vertex[] vertices { get { return _vertices; } }

    private Triangle[] _triangle;
    public Triangle[] triangles { get { return _triangle; } }

    private int[] newTri;
    private Vector3[] newVert;
    private Vector3[] newNorm;

    // Use this for initialization
    void Start()
    {
        Mesh myCloth = GetComponent<MeshFilter>().mesh;

        myCloth.Clear();

        sphereCol = GameObject.Find("Sphere");
        sphereTrans = sphereCol.transform;

        _gravity = new Vector3(0, -0.98f * SimScale, 0);

        //Mesh mesh = new Mesh();

        //sets up the correct number of spots to have based on how many vertices declared
        int particleCount = gridSize * gridSize;

        //calculate number of springs being connected to the right of each point
        int springCount = (gridSize - 1) * gridSize * 2;
        //adds the diagonal springs to the spring amount amount
        springCount += (gridSize - 1) * (gridSize - 1) * 2;
        //adding one past the neighbor
        springCount += (gridSize - 2) * gridSize * 2;

        _particles = new ClothParticle[particleCount];
        _springs = new ClothSpring[springCount];

        int countTris = gridSize * gridSize * 2 * 3;

        newTri = new int[countTris];
        newVert = new Vector3[particleCount];
        newNorm = new Vector3[particleCount];

        //newVertices = new Vector3[totalVertices];
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
        this.initMesh();
        this.reset();
    }

    private void initMesh()
    {
        Mesh myCloth = GetComponent<MeshFilter>().mesh;
        //sets up an array of ints telling to be used in creating the triangles of the mesh
        int triCount = (gridSize * gridSize) * 2;
        _triangle = new Triangle[triCount];


        //sets an array of Vector3's that are the vertices of the mesh
        int vertCount = (gridSize * gridSize);
        _vertices = new Vertex[vertCount];

        //inits the vertices the particles in an even spaced grid
        for (int j = 0; j < gridSize; j++)
        {
            for (int i = 0; i < gridSize; i++)
            {
                float U = (i / (float)(gridSize - 1)) - 0.5f;
                float V = (j / (float)(gridSize - 1)) - 0.5f;

                int BallID = j * gridSize + i;
                _particles[BallID].currentPosition = new Vector3((float)clothScale * U, 8.5f, (float)clothScale * V);
                newVert[BallID] = _particles[BallID].currentPosition;
                _particles[BallID].currentVelocity = Vector3.zero;

                _particles[BallID].inverseMass = 1.0f / mass;
                _particles[BallID].pinned = false;

                _particles[BallID].tension = Vector3.zero;
            }
        }

        myCloth.vertices = newVert;

        //loop that goes to create each point of the triangles
        int triangleCount = 0;
        int k = 0;
        for(int j = 0; j < gridSize - 1; j++)
        {
            for(int i = 0; i < gridSize - 1; i++)
            {
                var i0 = j * gridSize + i;
                var i1 = j * gridSize + i + 1;
                var i2 = (j + 1) * gridSize + i;
                var i3 = (j + 1) * gridSize + i + 1;

                _triangle[k].A = i2;
                _triangle[k].B = i1;
                _triangle[k].C = i0;

                newTri[triangleCount] = _triangle[k].A;
                triangleCount += 1;
                newTri[triangleCount] = _triangle[k].B;
                triangleCount += 1;
                newTri[triangleCount] = _triangle[k].C;
                triangleCount += 1;

                k ++;

                

                _triangle[k].A = i2;
                _triangle[k].B = i3;
                _triangle[k].C = i1;

                newTri[triangleCount] = _triangle[k].A;
                triangleCount += 1;
                newTri[triangleCount] = _triangle[k].B;
                triangleCount += 1;
                newTri[triangleCount] = _triangle[k].C;
                triangleCount += 1;

                k++;
            }
        }
        myCloth.triangles = newTri;
    }

    private void reset()
    {
        Mesh myCloth = GetComponent<MeshFilter>().mesh;
        float naturalLengthVec = (_particles[0].currentPosition - _particles[1].currentPosition).magnitude;

        //Pinned the corners
        _particles[0].pinned = true;
        _particles[gridSize - 1].pinned = true;

         _particles[gridSize * (gridSize -1)].pinned = true;
         _particles[gridSize * gridSize - 1].pinned = true;

        //Initialise the springs
        int currentSpring = 0;

        //The first (gridSize-1)*gridSize springs go from one ball to the next, excluding those on the right hand edge
        for (int j = 0; j < gridSize; j++)
            for (int i = 0; i < gridSize - 1; i++)
            {
                _springs[currentSpring] = new ClothSpring(j * gridSize + i, j * gridSize + i + 1, naturalLengthVec, StretchStiffness);
                currentSpring++;
            }

        //The next (gridSize-1)*gridSize springs go from one ball to the one below, excluding those on the bottom edge
        for (int j = 0; j < gridSize - 1; j++)
            for (int i = 0; i < gridSize; i++)
            {
                _springs[currentSpring] = new ClothSpring(j * gridSize + i, (j + 1) * gridSize + i, naturalLengthVec, StretchStiffness);
                currentSpring++;
            }

        //The next (gridSize-1)*(gridSize-1) go from a ball to the one below and right, excluding those on the bottom or right
        for (int j = 0; j < gridSize - 1; j++)
            for (int i = 0; i < gridSize - 1; i++)
            {
                _springs[currentSpring] = new ClothSpring(j * gridSize + i, (j + 1) * gridSize + i + 1, naturalLengthVec * (float)Mathf.Sqrt(2.0f), BendStiffness);
                currentSpring++;
            }

        //The next (gridSize-1)*(gridSize-1) go from a ball to the one below and left, excluding those on the bottom or right
        for (int j = 0; j < gridSize - 1; j++)
            for (int i = 1; i < gridSize; i++)
            {
                _springs[currentSpring] = new ClothSpring(j * gridSize + i, (j + 1) * gridSize + i - 1, naturalLengthVec * (float)Mathf.Sqrt(2.0f), BendStiffness);
                currentSpring++;
            }

        //The first (gridSize-2)*gridSize springs go from one ball to the next but one, excluding those on or next to the right hand edge
        for (int j = 0; j < gridSize; j++)
            for (int i = 0; i < gridSize - 2; i++)
            {
                _springs[currentSpring] = new ClothSpring(j * gridSize + i, j * gridSize + i + 2, naturalLengthVec * 2, BendStiffness);
                currentSpring++;
            }


        //The next (gridSize-2)*gridSize springs go from one ball to the next but one below, excluding those on or next to the bottom edge
        for (int j = 0; j < gridSize - 2; j++)
            for (int i = 0; i < gridSize; i++)
            {
                _springs[currentSpring] = new ClothSpring(j * gridSize + i, (j + 2) * gridSize + i, naturalLengthVec * 2, BendStiffness);
                currentSpring++;
            }

        UpdateMesh();
    }

    private static Vector3 CalculateNormal(Vector3 VA, Vector3 VB, Vector3 VC)
    {
        Vector3 a, b;

        a = VB - VA;
        b = VC - VA;

        var normal = Vector3.Cross(a, b);
        normal.Normalize();
        return normal;
    }

    private void UpdateMesh()
    {
        
        Mesh myCloth = GetComponent<MeshFilter>().mesh;

        //calculate triangle normals
        for (int i = 0; i<_triangle.Length; i++)
        {
            var t = _triangle[i];
            var A = vertices[t.A].position;
            var B = vertices[t.B].position;
            var C = vertices[t.C].position;
            _triangle[i].normal = CalculateNormal(A, B, C);
        }

        //Calculate the normals on the current particles
        for(int j = 0; j< gridSize; j++)
        {
            for(int i = 0; i < gridSize; i++)
            {
                int BallID = j * gridSize + i;
                Vector3 normal = Vector3.zero;
                int count = 0;
                for(int Y = 0; Y <= 1; Y++)
                {
                    for(int X = 0; X<=1; X++)
                    {
                        if(X + i <gridSize && Y + j< gridSize)
                        {
                            int Index = (j + Y) * gridSize + (i + X) * 2;
                            normal += _triangle[Index].normal;

                            Index++;
                            normal += _triangle[Index].normal;

                            count += 2;
                        }
                    }
                }

                normal /= (float)count;
                _vertices[BallID].normal = normal;
                newNorm[BallID] = _vertices[BallID].normal;
            }
        }

        for(int j = 0; j < gridSize; j++)
        {
            for(int i = 0; i<gridSize; i++)
            {
                int BallID = j * gridSize + i;
                _vertices[BallID].position = _particles[BallID].currentPosition;
                newVert[BallID] = _vertices[BallID].position;
            }
        }
        myCloth.vertices = newVert;

        myCloth.RecalculateBounds();
        myCloth.RecalculateNormals();
    }


    // Update is called once per frame
    public void Simulate()
    {
        Mesh myCloth = GetComponent<MeshFilter>().mesh;

        float theTime = Time.deltaTime;

        _timeSinceLastUpdate += theTime;

        bool updateMade = false;

        float timePassedInSeconds = minimumPhysicsDelta;

        while(_timeSinceLastUpdate > minimumPhysicsDelta)
        {
            _timeSinceLastUpdate -= minimumPhysicsDelta;
            updateMade = true;

            //Calculate tension in the springs
            for(int i = 0; i<_springs.Length; i++)
            {
                Vector3 tensionDirection = (_particles[_springs[i].P1].currentPosition - _particles[_springs[i].P2].currentPosition);

                float springLength = tensionDirection.magnitude;
                float extension = springLength - _springs[i]._NaturalLenght;

                float tension = _springs[i]._Stiffness * (extension * _springs[i]._InverseLength);


                tensionDirection *= tension / springLength;

                _particles[_springs[i].P2].tension += tensionDirection;
                _particles[_springs[i].P1].tension -= tensionDirection;
            }

            var sfhere = _colliders[0];
            sfhere.Position = sphereTrans.position;
            sfhere.Radius = sphereTrans.localScale.x / 2;
            _colliders[0] = sfhere;

            

            //calculate the nextParticles from the current one
            for(int i = 0; i < _particles.Length; i++)
            {
                //if pinned have changes be zero so they don't move
                if(_particles[i].pinned)
                {
                    _particles[i].nextPosition = _particles[i].currentPosition;
                    _particles[i].nextVelocity = Vector3.zero;

                    continue;
                }

                //calculate force and acceleration
                Vector3 force = _gravity + _particles[i].tension;

                Vector3 acceleration = force * (float)_particles[i].inverseMass;

                //update velocity
                _particles[i].nextVelocity = _particles[i].currentVelocity + (acceleration * timePassedInSeconds);
                //damp velocit
                _particles[i].nextVelocity *= dampFactor;

                force = _particles[i].nextVelocity * timePassedInSeconds;

                _particles[i].nextPosition = _particles[i].currentPosition + force;

                //check against colliders
                for(int j = 0; j<_colliders.Count; j++)
                {
                    Vector3 P = _particles[i].nextPosition - _colliders[j].Position;
                    float cR = _colliders[j].Radius * 1.07f;

                    if(P.sqrMagnitude < cR * cR)
                    {
                        P.Normalize();
                        P *= cR;
                        _particles[i].nextPosition = P + _colliders[j].Position;
                        _particles[i].nextVelocity = Vector3.zero;
                        break;

                    }
                }
            }
           
            //swap current and new particle pointers
            for(int i = 0; i<_particles.Length; i++)
            {
                _particles[i].currentPosition = _particles[i].nextPosition;
                newVert[i] = _particles[i].currentPosition;
                _particles[i].currentVelocity = _particles[i].nextVelocity;
                _particles[i].tension = Vector3.zero;
            }
            myCloth.vertices = newVert;
        }
        if(updateMade)
        {
            UpdateMesh();
        }
    }

    public void addCollider(Vector3 position, float radius)
    {
        var col = new ClothCollider(position, radius);
        _colliders.Add(col);
    }

    public void UnpinParticle(int index)
    {
        _particles[index].pinned = false;
    }

    //Need to create seperate points for out in the game field and then use that in creating the mesh
    //Triangles work with what spot in the array it is using from the other arrays
}
