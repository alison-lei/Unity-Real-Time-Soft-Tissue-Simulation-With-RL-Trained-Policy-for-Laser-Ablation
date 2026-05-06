using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class MeshHandle : MonoBehaviour
{
    public Vector3[] verts;
    protected Mesh mesh;
    private MeshFilter meshFilter;
    public int xDimensions = 60;
    public int zDimensions = 28;
    public GameObject mainCamera;
    public AgentScript agentScript;
    public MassSpringSystem massSpringSystem;
    private Vector3[] positions;
    float cutDepthScalpel, cutDepth;
    public Vector3 playerPos;
    private Dictionary<int, float> cutList;


    private int spread = 10;
    public float heightEffectiveRadius = 2f;
    public float heightCurveSteepness = 2f;


    private RenderTexture copyBlackRenderTexture;
    private int updateBlack_kernelID;
    public ComputeBuffer rewardBuffer;

    public ComputeShader uvComputeShader;
    public Material shader_graph_material;
    public float maxDistance = 3f;

    private ComputeBuffer positionBuffer;
    private RenderTexture uvRenderTexture;
    private int uv_kernelID;
    private int threadGroupSizeX = 4;
    private int threadGroupSizeY = 4;
    private int vertCount;
    public GameObject Scalpel;
    private int x;
    private int y;
    public bool spaceBar = false;
    public bool allInitialized = false;
    private bool dealedDamage = false;
    public PlayerController playerController;
    public GameObject quad;
    public Vector3[] initialpositions;


    void Start()
    {
        initialpositions = new Vector3[massSpringSystem.VertCount];
        initialpositions = massSpringSystem.GetPositions();
        for (int i = 0; i < initialpositions.Length; i++)
        {
            initialpositions[i] = new Vector3(initialpositions[i].x, initialpositions[i].z, initialpositions[i].y);
        }

        mesh = new Mesh();
        mesh.name = gameObject.name;
        mesh.vertices = GenerateVerts();
        verts = mesh.vertices;
        mesh.triangles = GenerateTriangles();
        mesh.RecalculateBounds();
        mesh.uv = SetTextureUVs();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;


        cutDepthScalpel = -0.1f;
        FixPlayerPosition();
        cutList = new Dictionary<int, float>();


        // Setting up compute shader, its kernels, and the necessary compute buffers
        vertCount = verts.Length;
        x = Mathf.CeilToInt((float)xDimensions / threadGroupSizeX);
        y = Mathf.CeilToInt((float)zDimensions / threadGroupSizeY);

        positionBuffer = new ComputeBuffer(vertCount, sizeof(float) * 3);
        positionBuffer.SetData(verts);

        uvRenderTexture = new RenderTexture(xDimensions, zDimensions, 0, RenderTextureFormat.RGFloat);
        uvRenderTexture.enableRandomWrite = true;
        uvRenderTexture.Create();
        Graphics.SetRenderTarget(uvRenderTexture);
        GL.Clear(true, true, Color.clear);
        Graphics.SetRenderTarget(null);

        uv_kernelID = uvComputeShader.FindKernel("CSMain");


        uvComputeShader.SetBuffer(uv_kernelID, "Positions", positionBuffer);
        uvComputeShader.SetFloat("maxDistance", maxDistance);
        uvComputeShader.SetInt("tot_vertCount", vertCount);
        uvComputeShader.SetTexture(uv_kernelID, "UVMap", uvRenderTexture);
        uvComputeShader.SetInt("xDimensions", xDimensions);

        
        shader_graph_material.SetTexture("_UVMap", uvRenderTexture);


        copyBlackRenderTexture = new RenderTexture(xDimensions, zDimensions, 0, RenderTextureFormat.RGFloat);
        copyBlackRenderTexture.enableRandomWrite = true;
        copyBlackRenderTexture.Create();
        Graphics.Blit(agentScript.fullPatternTexture, copyBlackRenderTexture);


        // in start, everything is sequential, but doesn't apply to fixed update 

        updateBlack_kernelID = uvComputeShader.FindKernel("updateCopyBlack");
        rewardBuffer = new ComputeBuffer(4, sizeof(int));
        rewardBuffer.SetData(new int[4] { 0, 0, 0, 0 });

        uvComputeShader.SetBuffer(updateBlack_kernelID, "Positions", positionBuffer);

        uvComputeShader.SetBuffer(uv_kernelID, "rewardBuffer", rewardBuffer);

        uvComputeShader.SetBuffer(updateBlack_kernelID, "rewardBuffer", rewardBuffer);
        uvComputeShader.SetTexture(updateBlack_kernelID, "UVMap", uvRenderTexture);
        uvComputeShader.SetTexture(updateBlack_kernelID, "copyBlackRenderTexture", copyBlackRenderTexture);

        allInitialized = true;

    }

    public void ResetRTBuffers()
    {
        cutList.Clear();
        rewardBuffer.SetData(new int[4] { 0, 0, 0, 0 });
        Graphics.SetRenderTarget(uvRenderTexture);
        GL.Clear(true, true, Color.clear);
        Graphics.SetRenderTarget(null);
        Graphics.Blit(agentScript.fullPatternTexture, copyBlackRenderTexture);
        allInitialized = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (mesh != null)
            mesh.RecalculateNormals();


        FixPlayerPosition();

        Vector3 fixedPlayerPos = new Vector3(playerPos.x + xDimensions / 2, playerPos.y, playerPos.z + zDimensions / 2);
        uvComputeShader.SetVector("playerPosition", playerPos);

        verts = mesh.vertices;

        positions = massSpringSystem.GetPositions();
        int count = 0;
        cutDepth = cutDepthScalpel;


        
        for (int i = 0; i < zDimensions; i++)
        {
            for (int j = 0; j < xDimensions; j++)
            {
                int idx = GetIndex(j, i);
                verts[idx] = new Vector3(positions[count].x, positions[count].z, positions[count].y);
                if (j < xDimensions - 1 && i < zDimensions - 1)
                {
                    float posY = (positions[count].z + positions[count + 1].z + positions[count + xDimensions].z + positions[count + xDimensions + 1].z) / 4.0f;
                    verts[idx + xDimensions] = new Vector3(positions[count].x + 0.5f, posY, positions[count].y + 0.5f);
                }
                if (cutDepth > playerPos.y && j == (int)(fixedPlayerPos.x) && i == (int)(fixedPlayerPos.z))
                {
                    int num = 0;
                    for (int k = idx - (xDimensions * spread); k <= idx + (xDimensions * spread); k += xDimensions)
                    {
                        for (int l = num * -1; l < spread * 2 - num; l++)
                        {
                            if (k + l < vertCount && 0 <= k + l)
                            {
                                if (spaceBar)
                                {
                                    if (!dealedDamage)
                                        DealDamage();
                                    CalcVertHeight(verts[k + l], k + l);

                                }

                            }

                        }
                        num++;
                    }
                    
                }
                count++;
            }
        }

        foreach (var pair in cutList)
        {
            verts[pair.Key].y += pair.Value;
        }

        mesh.vertices = verts;
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        spaceBar = false;
        dealedDamage = false;
    }


    private void DealDamage()
    {
        uvComputeShader.Dispatch(uv_kernelID, x, y, 1);
        dealedDamage = true;
    }


    public void CalculateReward()
    {
        uvComputeShader.Dispatch(updateBlack_kernelID, x, y, 1);
    }


    void CalcVertHeight(Vector3 vertex, int index)
    {
        
        float gaussValue = Gaussian(vertex, playerPos);
        float depth = gaussValue * playerPos.y;
        if (cutList.ContainsKey(index))
        {
            if (depth < cutList[index])
            {
                cutList[index] = depth;
            }
        }
        else
        {
            cutList.Add(index, depth);
        }
    }

    // relative to center of mesh, get the position of tip of scalpel
    public void FixPlayerPosition()
    {
        playerPos = Scalpel.transform.localPosition - quad.transform.localPosition;
        playerPos.y = playerPos.y - 7.0f;
    }


    private Vector3[] GenerateVerts()
    {
        positions = massSpringSystem.GetPositions();
        Vector3[] vertices = new Vector3[(2 * xDimensions - 1) * zDimensions];
        int count = 0;
        for (int i = 0; i < zDimensions; i++)
        {
            for (int j = 0; j < xDimensions; j++)
            {
                vertices[GetIndex(j, i)] = new Vector3(positions[count].x, positions[count].z, positions[count].y);
                if (j < xDimensions - 1 && i < zDimensions - 1)
                    vertices[GetIndex(j + xDimensions, i)] = new Vector3(positions[count].x + 0.5f, positions[count].z, positions[count].y + 0.5f);
                count++;

            }
        }

        return vertices;
    }

    public int GetIndex(int x, int z)
    {
        return z * (2 * xDimensions - 1) + x;
    }

    protected int[] GenerateTriangles()
    {
        int[] triangles = new int[mesh.vertices.Length * 12];

        for (int i = 0; i < zDimensions - 1; i++)
        {
            for (int j = 0; j < xDimensions - 1; j++)
            {
                int index = GetIndex(j, i);
                triangles[index * 12] = GetIndex(j + 1, i);
                triangles[index * 12 + 1] = index;
                triangles[index * 12 + 2] = GetIndex(j + xDimensions, i);
                triangles[index * 12 + 3] = GetIndex(j, i + 1);
                triangles[index * 12 + 4] = GetIndex(j + 1, i + 1);
                triangles[index * 12 + 5] = GetIndex(j + xDimensions, i);
                triangles[index * 12 + 6] = index;
                triangles[index * 12 + 7] = GetIndex(j, i + 1);
                triangles[index * 12 + 8] = GetIndex(j + xDimensions, i);
                triangles[index * 12 + 9] = GetIndex(j + 1, i + 1);
                triangles[index * 12 + 10] = GetIndex(j + 1, i);
                triangles[index * 12 + 11] = GetIndex(j + xDimensions, i);
            }
        }

        return triangles;
    }

    private Vector2[] SetTextureUVs()
    {
        Vector2[] uvs = new Vector2[mesh.vertices.Length];

        float xStep = 1.0f / (float)(xDimensions - 1);
        float zStep = 1.0f / (float)(zDimensions - 1);
        int idx = 0;
        for (int i = 0; i < zDimensions; i++)
        {
            for (int j = 0; j < xDimensions; j++)
            {
                idx = GetIndex(j, i);
                uvs[idx] = new Vector2(j * xStep, i * zStep);
                if (j < xDimensions - 1 && i < zDimensions - 1)
                {
                    uvs[idx + xDimensions] = new Vector2((j + 0.5f) * xStep, (i + 0.5f) * zStep);
                }
            }
        }
        return uvs;
    }

    public float Distance(Vector3 playerPosition, Vector3 point)
    {
        float xDist = playerPosition.x - point.x;
        float zDist = playerPosition.z - point.z;
        float dist = Mathf.Sqrt(xDist * xDist + zDist * zDist);
        return dist;
    }

    private float Gaussian(Vector3 vertex, Vector3 playerPos)
    {
        float distance = Distance(playerPos, vertex);
        float sigma = heightEffectiveRadius / heightCurveSteepness;
        if (sigma < 0.001f)
        {
            sigma = 0.001f;
        }
        float gaussianValue = Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(sigma, 2)));

        return gaussianValue;
    }

    private void OnDestroy()
    {
        if (uvRenderTexture != null)
        {
            uvRenderTexture.Release();
        }
        if (positionBuffer != null)
        {
            positionBuffer.Dispose();
        }
        if (copyBlackRenderTexture != null)
        {
            copyBlackRenderTexture.Release();
        }
        if (rewardBuffer != null)
        {
            rewardBuffer.Dispose();
        }
    }
}

