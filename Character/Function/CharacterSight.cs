using UnityEngine;
using Photon.Pun;

public class CharacterSight : MonoBehaviour
{
    #region Variables

    [SerializeField] private Transform characterTransform = null;
    [SerializeField] private GameObject sightObject = null;
    [SerializeField] private GameObject circleObject = null;

    [SerializeField] private Vector3 sightOffset = Vector3.zero;
    
    private Mesh sightMesh = null;
    private MeshFilter sightMeshFilter = null;

    private Mesh circleMesh = null;
    private MeshFilter circleMeshFilter = null;

    private float fov = 90f;
    private int rayCount = 90;

    private float viewDistance = 8f;
    private float circleDistance = 1f;

    private LayerMask layerMask;

    #endregion Variables

    #region Unity Events
    
    private void Awake()
    {
        if (!GetComponent<PhotonView>().IsMine)
        {
            Destroy(sightObject);
            Destroy(circleObject);
            Destroy(this);
            return;
        }

        sightMeshFilter = Utilities.GetOrAddComponent<MeshFilter>(sightObject);
        circleMeshFilter = Utilities.GetOrAddComponent<MeshFilter>(circleObject);

        sightMesh = new();
        sightMeshFilter.mesh = sightMesh;
        circleMesh = new();
        circleMeshFilter.mesh = circleMesh;

        layerMask = LayerMask.GetMask(TagAndLayer.Layer.WALL);
    }

    #endregion Unity Events

    #region Methods

    public void SetFOV(float fov)
    {
        this.fov = fov;
        rayCount = (int)fov;
    }

    public void DrawSight(Vector2 direction)
    {
        Vector3 characterPosition = characterTransform.position;
        Vector3 origin = characterPosition + sightOffset;
        float angle = Utilities.GetAngleFromVector(direction) + (fov * 0.5f);
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = sightOffset;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int index = 0; index <= rayCount; index++)
        {
            Vector3 vertex = Vector3.zero;
            Vector3 rayDirection = Utilities.GetVectorFromAngle(angle);

            RaycastHit2D hitObject = Physics2D.Raycast(origin, Utilities.GetVectorFromAngle(angle), viewDistance, layerMask);
            if (hitObject.collider == null) // No hit
            {
                vertex = origin + rayDirection * viewDistance;
            }
            else // Hit the object
            {
                vertex = hitObject.point;
            }

            vertex -= characterPosition;
            vertices[vertexIndex] = vertex;

            if (index > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        sightMesh.vertices = vertices;
        sightMesh.uv = uv;
        sightMesh.triangles = triangles;
    }

    public void DrawCircleSight()
    {
        Vector3 characterPosition = characterTransform.position;
        Vector3 origin = characterPosition + sightOffset;
        float angle = 0f;
        float angleIncrease = 360f / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = sightOffset;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int index = 0; index <= rayCount; index++)
        {
            Vector3 vertex = Vector3.zero;
            Vector3 rayDirection = Utilities.GetVectorFromAngle(angle);

            RaycastHit2D hitObject = Physics2D.Raycast(origin, Utilities.GetVectorFromAngle(angle), circleDistance, layerMask);
            if (hitObject.collider == null) // No hit
            {
                vertex = origin + rayDirection * circleDistance;
            }
            else // Hit the object
            {
                vertex = hitObject.point;
            }

            vertex -= characterPosition;
            vertices[vertexIndex] = vertex;

            if (index > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        circleMesh.vertices = vertices;
        circleMesh.uv = uv;
        circleMesh.triangles = triangles;
    }

    #endregion Methods
}
