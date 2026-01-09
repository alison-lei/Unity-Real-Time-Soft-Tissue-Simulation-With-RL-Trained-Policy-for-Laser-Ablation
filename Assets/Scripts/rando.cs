// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering;
// using Unity.Collections;

// public class MeshHandle : MonoBehaviour
// {
//     protected Mesh mesh;
//     private MeshFilter meshFilter;
//     private int xDimensions, zDimensions;
//     public GameObject mainCamera;
//     private MassSpringSystem massSpringSystem;
//     private Vector3[] positions;
//     float cutDepthStick, cutDepthScalpel, cutDepth, currentDepth;
//     private Vector3 playerPos;
//     private Dictionary<int, float> cutList;
//     public Gradient gradient;
//     private float minHeight, maxHeight;
//     public bool trainingMode = false;


//     private int spread = 20;
//     public float heightEffectiveRadius = 5f;
//     public float heightCurveSteepness = 2f;


//     //////////////////////////////////////////////////////////////////////

//     public Texture2D png_BlackMarker;
//     private RenderTexture copyBlackRenderTexture;
//     private int black_kernelID;
//     private int updateBlack_kernelID;
//     private ComputeBuffer rewardBuffer;

//     //////////////////////////////////////////////////////////////////////

//     public ComputeShader uvComputeShader;
//     public Material shader_graph_material;
//     public float maxDistance = 20f;

//     private ComputeBuffer positionBuffer;
//     private RenderTexture uvRenderTexture;
//     private int uv_kernelID;
//     private int threadGroupSizeX = 8;
//     private int threadGroupSizeY = 8;
//     private int vertCount;


//     public bool green_icon = false;
//     public GameObject Scalpel;
//     private int x;
//     private int y;
//     // private float? lastReward = null;
//     // public float reward = 0;
//     // private float? finalReward = null;


//     void Start()
//     {
//         mainCamera = GameObject.Find("Main Camera");
//         massSpringSystem = mainCamera.GetComponent<MassSpringSystem>();
//         positions = new Vector3[massSpringSystem.VertCount];
//         positions = massSpringSystem.GetPositions();
//         xDimensions = 120; // 59 quads
//         zDimensions = 56; // 27 quads

//         // xDimensions = 60;
//         // zDimensions = 28;
//         mesh = new Mesh();
//         mesh.name = gameObject.name;
//         mesh.vertices = GenerateVerts();
//         mesh.triangles = GenerateTriangles();
//         mesh.RecalculateBounds();
//         mesh.uv = SetTextureUVs();

//         // uv2 = MakeUV2();
//         // mesh.uv2 = uv2;

//         //colors = SetInitialColor();
//         //mesh.colors = colors;
//         meshFilter = gameObject.AddComponent<MeshFilter>();
//         meshFilter.mesh = mesh;
//         minHeight = -16.0f;
//         maxHeight = 0.0f;
//         cutDepthScalpel = -0.1f;
//         cutDepthStick = -16.0f;
//         playerPos = new Vector3();
//         cutList = new Dictionary<int, float>();

//         //cutRange = 0.5f;

//         // units of radius are in terms of UV coordinates
//         // radius = 0.04f;
//         // dynamicDamageManager = gameObject.GetComponent<DynamicDamageManager>();

//         // black_uv = new List<Vector2>();

//         // if (black_marker != null)
//         // {
//         //     for (int i = 0; i < black_markerY; i++)
//         //     {
//         //         for (int j = 0; j < black_markerX; j++)
//         //         {
//         //             pixel = black_marker.GetPixel(j, i);
//         //             if (pixel.r != 0)
//         //             {
//         //                 temp_uv = new Vector2((float)j / black_markerX, (float)i / black_markerY);
//         //                 black_uv.Add(temp_uv);
//         //             }
//         //         }
//         //     }
//         // }
//         // for (int i = 0; i < black_uv.Count; i++)
//         // {
//         //     Debug.Log(black_uv[i]);
//         // }


//         ////////////////////////////////////////////////////////////////////

//         vertCount = mesh.vertices.Length;
//         x = Mathf.CeilToInt(xDimensions / threadGroupSizeX);
//         y = Mathf.CeilToInt(zDimensions / threadGroupSizeY);

//         positionBuffer = new ComputeBuffer(vertCount, sizeof(float) * 3);
//         positionBuffer.SetData(mesh.vertices);

//         // if (uvRenderTexture != null)
//         // {
//         //     Debug.Log("hi");
//         //     if (uvRenderTexture.IsCreated())
//         //         uvRenderTexture.Release();
//         //     DestroyImmediate(uvRenderTexture);
//         //     uvRenderTexture = null;

//         // }
        
//         uvRenderTexture = new RenderTexture(xDimensions, zDimensions, 0, RenderTextureFormat.RGFloat);
//         uvRenderTexture.enableRandomWrite = true;
//         uvRenderTexture.Create();
//         Graphics.SetRenderTarget(uvRenderTexture);
//         GL.Clear(true, true, Color.clear);

//         copyBlackRenderTexture = new RenderTexture(xDimensions, zDimensions, 0, RenderTextureFormat.RGFloat);
//         copyBlackRenderTexture.enableRandomWrite = true;
//         copyBlackRenderTexture.Create();
//         Graphics.SetRenderTarget(copyBlackRenderTexture);
//         GL.Clear(true, true, Color.clear);
//         Graphics.Blit(png_BlackMarker, copyBlackRenderTexture);

//         Graphics.SetRenderTarget(null);
        
            

//         uv_kernelID = uvComputeShader.FindKernel("CSMain");
//         uvComputeShader.SetBuffer(uv_kernelID, "Positions", positionBuffer);
//         uvComputeShader.SetFloat("maxDistance", maxDistance);
//         uvComputeShader.SetInt("tot_vertCount", vertCount);
//         uvComputeShader.SetTexture(uv_kernelID, "UVMap", uvRenderTexture);
//         uvComputeShader.SetInt("xDimensions", xDimensions);

//         shader_graph_material.SetTexture("_UVMap", uvRenderTexture);
//         // Vector2[] uv2 = new Vector2[vertCount];
//         // for (int i = 0; i < vertCount; i++)
//         // {
//         //     uv2[i] = new Vector2((float)i / (vertCount - 1), 0.0f);
//         // }
//         // mesh.uv2 = uv2;


//         //lack_kernelID = uvComputeShader.FindKernel("copyTexture");
//         // uvComputeShader.SetTexture(black_kernelID, "png_BlackMarker", png_BlackMarker);
//         // uvComputeShader.SetTexture(black_kernelID, "copyBlackRenderTexture", copyBlackRenderTexture);
//         // uvComputeShader.Dispatch(black_kernelID, x, y, 1);

//         // in start, everything is sequential, but doesn't apply to fixed update 

//         updateBlack_kernelID = uvComputeShader.FindKernel("updateCopyBlack");
//         rewardBuffer = new ComputeBuffer(1, sizeof(int));
//         rewardBuffer.SetData(new int[1] { 0 });
//         uvComputeShader.SetBuffer(updateBlack_kernelID, "rewardBuffer", rewardBuffer);
//         uvComputeShader.SetTexture(updateBlack_kernelID, "UVMap", uvRenderTexture);
//         uvComputeShader.SetTexture(updateBlack_kernelID, "copyBlackRenderTexture", copyBlackRenderTexture);
//         /////////////////////////////////////////////////////////////////////////

//     }

//     // Update is called once per frame
//     void FixedUpdate()
//     {
//         mesh.RecalculateNormals();
//         FixPlayerPosition();
//         //Debug.Log(Scalpel.transform.position);
//         if (-5.5 <= playerPos.y && playerPos.y <= -3.5f)
//         {
//             green_icon = true;
//         }
//         else
//         {
//             green_icon = false;
//         }

//         if (green_icon && Input.GetKey(KeyCode.Space))
//         {
//             positions = massSpringSystem.GetPositions();
//             var verts = mesh.vertices;
//             int count = 0;
//             cutDepth = GetCutDepth();
//             FixPlayerPosition();

//             //////////////////////////////////////////////////////////////////////
//             //uvComputeShader.SetVector("playerPosition", playerPos);
//             //uvComputeShader.Dispatch(uv_kernelID, x, y, 1);
//             //uvComputeShader.Dispatch(updateBlack_kernelID, x, y, 1);
//             /////////////////////////////////////////////////////////////////////////



//             Vector3 fixedPlayerPos = new Vector3(playerPos.x + xDimensions / 2, playerPos.y, playerPos.z + zDimensions / 2);

//             for (int i = 0; i < zDimensions; i++)
//             {
//                 for (int j = 0; j < xDimensions; j++)
//                 {
//                     int idx = GetIndex(j, i);
//                     verts[idx] = new Vector3(positions[count].x, positions[count].z, positions[count].y);


//                     if (j < xDimensions - 1 && i < zDimensions - 1)
//                     {
//                         float posY = (positions[count].z + positions[count + 1].z + positions[count + xDimensions].z + positions[count + xDimensions + 1].z) / 4.0f;
//                         verts[idx + xDimensions] = new Vector3(positions[count].x + 0.5f, posY, positions[count].y + 0.5f);
//                     }
//                     if (cutDepth > playerPos.y && j == (int)(fixedPlayerPos.x) && i == (int)(fixedPlayerPos.z))
//                     {
//                         int num = 0;
//                         for (int k = idx - (xDimensions * spread); k <= idx + (xDimensions * spread); k += xDimensions)
//                         {
//                             for (int l = num * -1; l < spread * 2 - num; l++)
//                             {
//                                 //Debug.Log(k + l);
//                                 if (k + l < vertCount && 0 <= k + l)
//                                 {
//                                     CalcVertHeight(verts[k + l], k + l, playerPos);
//                                 }


//                                 // if (k + l != idx + xDimensions)
//                                 // {
//                                 //     CalcVertHeight(verts[k + l], k + l, playerPos);
//                                 // }
//                                 // else
//                                 // {
//                                 //     CalcVertHeightCent(verts[k + l], k + l, playerPos, verts);
//                                 // }
//                             }
//                             num++;
//                         }


//                     }
//                     count++;
//                 }
//             }
//             // Debug.Log("this is player position" + playerPos.y);

//             foreach (var pair in cutList)
//             {
//                 verts[pair.Key].y += pair.Value;
//                 //colors[pair.Key] = gradient.Evaluate(Mathf.InverseLerp(minHeight, maxHeight, pair.Value));
//                 //colors[pair.Key] = new Color(0.0f, 1.0f, 0.0f, 1.0f);
//                 // List<int> adjVerts = GetAdjacentVertices(pair.Key);
//                 // foreach (int vert in adjVerts)
//                 // {
//                 //     colors[vert] = colors[pair.Key];
//                 // } 
//                 //dynamicDamageManager.ApplyDamage(mesh.uv[pair.Key], radius);


//             }

//             // sends array to GPU, transforms it into UV1 or TEXCOORD1 in shader
//             // each 3 vertices form a triangle
//             // only adds extra info to each vertex 
//             //mesh.uv2 = uv2;
//             mesh.vertices = verts;
//             //mesh.colors = colors;

//             mesh.RecalculateBounds();
//             meshFilter.mesh = mesh;

//             // int[] rewardResult = new int[1];
//             // rewardBuffer.GetData(rewardResult);
//             // Debug.Log("Reward: " + rewardResult[0]);

//             // AsyncGPUReadback.Request(rewardBuffer, OnReadbackComplete);

//         }

//     }

//     void Update()
//     {
//         if (green_icon && Input.GetKey(KeyCode.Space))
//         {
//             uvComputeShader.SetVector("playerPosition", playerPos);
//             uvComputeShader.Dispatch(uv_kernelID, x, y, 1);
//             uvComputeShader.Dispatch(updateBlack_kernelID, x, y, 1);
//             AsyncGPUReadback.Request(rewardBuffer, OnReadbackComplete);
//         }

//         if (finalReward == null)
//         {
//             Debug.Log(reward);
//             finalReward = reward;
//         }
//         else if (finalReward != reward)
//         {
//             Debug.Log(reward);
//             finalReward = reward;
//         }
        
        
//     }

//     void OnReadbackComplete(AsyncGPUReadbackRequest request)
//     {
//         NativeArray<int> result = request.GetData<int>();
//         int rewardValue = result[0];

//         Debug.Log(rewardValue);

//         if (lastReward == null)
//         {
//             reward += rewardValue;
//             lastReward = rewardValue;
//         }

//         else if (lastReward.HasValue && rewardValue != lastReward)
//         {
//             //Debug.Log(rewardValue);
//             reward += (float)(rewardValue - lastReward);
//             lastReward = rewardValue;
//         }

//     }

//     void CalcVertHeight(Vector3 vertex, int index, Vector3 playerPos)
//     {
//         //float vertDist = Distance(playerPos, vertex);
//         //cutRange = Mathf.Abs(Mathf.Tan(-playerPos.y / 10.0f) * 4f);



//         // if (vertDist < cutRange)
//         // {
//         //findDepth(cutRange, vertDist, playerPos.y);
//         float depth = Gaussian(vertex, playerPos);
//         if (cutList.ContainsKey(index))
//         {
//             if (depth < cutList[index])
//             {
//                 cutList[index] = depth;
//             }
//         }
//         else
//         {
//             cutList.Add(index, depth);
//         }

//         //}
//     }


//     float GetCutDepth()
//     {
//         if (GameObject.FindGameObjectWithTag("Stick").GetComponent<CharacterController>().enabled == true)
//         {
//             playerPos = GameObject.FindGameObjectWithTag("Stick").transform.position;
//             return cutDepthStick;
//         }
//         playerPos = GameObject.FindGameObjectWithTag("Scalpel").transform.position;
//         return cutDepthScalpel;
//     }

//     void FixPlayerPosition()
//     {
//         //playerPos.y -= 12f;
//         playerPos.y = Scalpel.transform.position.y - 13.0f;
//     }


//     private Vector3[] GenerateVerts()
//     {
//         positions = massSpringSystem.GetPositions();
//         Vector3[] verts = new Vector3[(2 * xDimensions - 1) * zDimensions];
//         int count = 0;
//         for (int i = 0; i < zDimensions; i++)
//         {
//             for (int j = 0; j < xDimensions; j++)
//             {
//                 verts[GetIndex(j, i)] = new Vector3(positions[count].x, positions[count].z, positions[count].y);
//                 if (j < xDimensions - 1 && i < zDimensions - 1)
//                     verts[GetIndex(j + xDimensions, i)] = new Vector3(positions[count].x + 0.5f, positions[count].z, positions[count].y + 0.5f);
//                 count++;

//             }
//         }

//         return verts;
//     }

//     public int GetIndex(int x, int z)
//     {
//         return z * (2 * xDimensions - 1) + x;
//     }

//     protected int[] GenerateTriangles()
//     {
//         int[] triangles = new int[mesh.vertices.Length * 12];

//         for (int i = 0; i < zDimensions - 1; i++)
//         {
//             for (int j = 0; j < xDimensions - 1; j++)
//             {
//                 int index = GetIndex(j, i);
//                 triangles[index * 12] = GetIndex(j + 1, i);
//                 triangles[index * 12 + 1] = index;
//                 triangles[index * 12 + 2] = GetIndex(j + xDimensions, i);
//                 triangles[index * 12 + 3] = GetIndex(j, i + 1);
//                 triangles[index * 12 + 4] = GetIndex(j + 1, i + 1);
//                 triangles[index * 12 + 5] = GetIndex(j + xDimensions, i);
//                 triangles[index * 12 + 6] = index;
//                 triangles[index * 12 + 7] = GetIndex(j, i + 1);
//                 triangles[index * 12 + 8] = GetIndex(j + xDimensions, i);
//                 triangles[index * 12 + 9] = GetIndex(j + 1, i + 1);
//                 triangles[index * 12 + 10] = GetIndex(j + 1, i);
//                 triangles[index * 12 + 11] = GetIndex(j + xDimensions, i);
//             }
//         }

//         return triangles;
//     }

//     private Vector2[] SetTextureUVs()
//     {
//         Vector2[] uvs = new Vector2[mesh.vertices.Length];

//         float xStep = 1.0f / (float)(xDimensions - 1);
//         float zStep = 1.0f / (float)(zDimensions - 1);
//         int idx = 0;
//         for (int i = 0; i < zDimensions; i++)
//         {
//             for (int j = 0; j < xDimensions; j++)
//             {
//                 idx = GetIndex(j, i);
//                 uvs[idx] = new Vector2(j * xStep, i * zStep);
//                 if (j < xDimensions - 1 && i < zDimensions - 1)
//                 {
//                     uvs[idx + xDimensions] = new Vector2((j + 0.5f) * xStep, (i + 0.5f) * zStep);
//                 }
//             }
//         }
//         return uvs;
//     }

//     private Color[] SetInitialColor()
//     {
//         Color[] clrs = new Color[mesh.vertices.Length];
//         for (int i = 0; i < clrs.Length; i++)
//         {
//             // sets the surface skin color
//             clrs[i] = new Color(0.73f, 0.15f, 0.08f, 1.0f);
//             //clrs[i] = new Color(0f, 0f, 0f, 1.0f);
//         }
//         return clrs;
//     }

//     private List<int> GetAdjacentVertices(int vertIndex)
//     {
//         // mesh is upside down, bottom is up
//         List<int> adjVertices = new List<int>();
//         int count = 0;

//         for (int i = vertIndex - (xDimensions * spread); i <= vertIndex + (xDimensions * spread); i += xDimensions)
//         {
//             for (int j = count * -1; j < 2 * spread - count; j++)
//             {
//                 adjVertices.Add(i + j);
//             }

//             count++;
//         }

//         return adjVertices;

//     }

//     private List<int> GetSquareVertices(int vertIndex)
//     {
//         List<int> adjVertices = new List<int>();

//         adjVertices.Add(vertIndex);
//         adjVertices.Add(vertIndex + 1);
//         adjVertices.Add(vertIndex + xDimensions);
//         adjVertices.Add(vertIndex + xDimensions + 1);

//         return adjVertices;

//     }

//     // private Vector2[] MakeUV2()
//     // {
//     //     Vector2[] temp_uv2 = new Vector2[mesh.vertices.Length];
//     //     for (int i = 0; i < mesh.vertices.Length; i++)
//     //     {
//     //         temp_uv2[i] = Vector2.zero;
//     //     }
//     //     return temp_uv2;
//     // }

//     private float Distance(Vector3 playerPosition, Vector3 vertex)
//     {
//         float xDist = playerPosition.x - vertex.x;
//         float zDist = playerPosition.z - vertex.z;
//         float dist = Mathf.Sqrt(xDist * xDist + zDist * zDist);
//         return dist;
//     }

//     private float Gaussian(Vector3 vertex, Vector3 playerPos)
//     {
//         float distance = Distance(playerPos, vertex);
//         float sigma = heightEffectiveRadius / heightCurveSteepness;
//         if (sigma < 0.001f)
//         {
//             sigma = 0.001f;
//         }
//         float gaussianValue = Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(sigma, 2)));

//         return gaussianValue * playerPos.y;
//     }
//     void OnGUI()
//     {
//         if (copyBlackRenderTexture != null)
//         {
//             GUI.DrawTexture(new Rect(10, 90, 128, 128), copyBlackRenderTexture, ScaleMode.ScaleToFit);
//         }
//         if (uvRenderTexture != null)
//         {
//             GUI.DrawTexture(new Rect(10, 200, 128, 128), uvRenderTexture, ScaleMode.ScaleToFit);
//         }
//     }

//     private void OnDestroy()
//     {
//         // Graphics.SetRenderTarget(uvRenderTexture);
//         // GL.Clear(true, true, Color.clear);
//         // Graphics.SetRenderTarget(null);

//         if (uvRenderTexture != null)
//         {
//             uvRenderTexture.Release();
//             //Destroy(uvRenderTexture);
//             //uvRenderTexture = null;
//         }
//         if (positionBuffer != null)
//         {
//             positionBuffer.Dispose();
//         }
//         if (copyBlackRenderTexture != null)
//         {
//             copyBlackRenderTexture.Release();
//         }
//         if (rewardBuffer != null)
//         {
//             rewardBuffer.Dispose();
//         }
//     }
// }













// // Each #kernel tells which function to compile; you can have many kernels
// #pragma kernel CSMain
// //#pragma kernel copyTexture
// #pragma kernel updateCopyBlack

// // Create a RenderTexture with enableRandomWrite flag and set it
// // with cs.SetTexture

// // indexed in terms of pixels not UV space, so dont need to normalie final coordinates
// RWTexture2D<float2> UVMap;

// // Texture2D<float4> png_BlackMarker;
// RWTexture2D<float2> copyBlackRenderTexture;
// RWStructuredBuffer<int> rewardBuffer;
// //RWStructuredBuffer<float> temporary;
// StructuredBuffer<float3> Positions;

// uint tot_vertCount;
// float maxDistance;
// float3 playerPosition;
// int xDimensions;


// // [numthreads(8,8,1)]
// // void copyTexture (uint3 id : SV_DispatchThreadID)
// // {
// //     int2 coord = int2(id.xy);
// //     copyBlackRenderTexture[coord] = float2(png_BlackMarker[coord].x, 0.0);
// //     //temporary[(2 * xDimensions - 1) * id.y + id.x] = copyBlackRenderTexture[coord].x;
// // }

// [numthreads(8,8,1)]
// // uint3 is a Vector3, basically this parameter is the index of the vertex
// void CSMain (uint3 id : SV_DispatchThreadID)
// {
//     // TODO: insert actual code here!
//     // y is height and x is width, y is the outer forloop, x is the inner forloop
//     uint xIndex = id.x;
//     uint yIndex = id.y;
//     // skips the middle vertices, so just quads, goes one by one
//     uint index = (2 * xDimensions - 1) * yIndex + xIndex;

//     if (index >= tot_vertCount) return;

//     float3 position = Positions[index];
//     float xDist = playerPosition.x - position.x;
//     float zDist = playerPosition.z - position.z;
//     float dist = sqrt(xDist * xDist + zDist * zDist);    
    
//     // blend factor from 1-0
//     float blend = pow(saturate(1.0 - dist / maxDistance), 1.0 / 4.0);

//     // write result to output 2D texture
//     UVMap[int2(xIndex, yIndex)] = float2(max(UVMap[int2(xIndex, yIndex)].x, blend), 0.0f);
    
// }


// [numthreads(8,8,1)]
// void updateCopyBlack (uint3 id : SV_DispatchThreadID)
// {
//     int2 coord = int2(id.xy);
//     // bool done = true;

//     // if (id.x == 0 && id.y == 0 && id.z == 0)
//     // {
//     //     for (int i = 0; i < 56; i++)
//     //     {
//     //         for (int j = 0; j < 120; j++)
//     //         {
//     //             if (copyBlackRenderTexture[int2(j, i)].x == 1.0f)
//     //             {
//     //                 done = false;
//     //                 break;
//     //             }
//     //         }
//     //         if (done == false)
//     //         {
//     //             break;
//     //         }
//     //     }
//     //     if (done == true)
//     //     {
//     //         InterlockedAdd(rewardBuffer[0], 100);
//     //     }
//     // }
        
//     // else
//     // {
//         if (UVMap[coord].x != 0)
//     {
//         if (copyBlackRenderTexture[coord].x != 0.5f)
//         {
//             if (abs(copyBlackRenderTexture[coord].x - 1.0f) < 0.01f)
//             {
//                 InterlockedAdd(rewardBuffer[0], 1); 
//             }
//             else
//             {
//                 InterlockedAdd(rewardBuffer[0], -1);
//             }
//             copyBlackRenderTexture[coord] = float2(0.5, 0.0);
//         }
        
//     }
//     //}
        
// }























// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine;
// // // // using UnityEngine.Rendering; // required for AsyncGPUReadback
// // // // using Unity.Collections; // required for NativeArray

// // public class MeshHandle : MonoBehaviour
// // {
// //     //private int resolutionFactor;
// //     protected Mesh mesh;
// //     private MeshFilter meshFilter;
// //     private int xDimensions, zDimensions;
// //     public GameObject mainCamera;
// //     private MassSpringSystem massSpringSystem;
// //     private Vector3[] positions;
// //     float cutDepthStick, cutDepthScalpel, cutDepth, currentDepth;
// //     private Vector3 playerPos;
// //     private Dictionary<int, float> cutList;
// //     public Gradient gradient;
// //     private float minHeight, maxHeight;
// //     //private Color[] colors;

// //     //private float cutRange;



// //     // public float radius;
// //     // public DynamicDamageManager dynamicDamageManager;
// //     //private Vector2[] uv2;

// //     //private List<Vector2> black_uv;
// //     // public Texture2D black_marker;
// //     // public int black_markerX = 120;
// //     // public int black_markerY = 56;

// //     // currently false
// //     public bool trainingMode = false;
// //     private Color pixel;
// //     private Vector2 temp_uv;

// //     private int spread = 20;
// //     // public float minVertexHeightAtPlayer = 0.1f;
// //     // public float maxVertexHeightFarAway = 5f;
// //     public float heightEffectiveRadius = 5f;
// //     public float heightCurveSteepness = 2f;


// // //     //////////////////////////////////////////////////////////////////////

// //     public ComputeShader uvComputeShader;
// // //     public Texture2D BlackMarker;
// //     public Material shader_graph_material;
// //     public float maxDistance = 20f;

// //     private ComputeBuffer positionBuffer;
// // //     // private ComputeBuffer rewardBuffer;
// // //     // private ComputeBuffer temporary;
// // //     // private ComputeBuffer temp_RWBuffer;

// // //     //private RenderTexture copyBlackTexture;
// //     private RenderTexture uvRenderTexture;
// //     private int uv_kernelID;
// // //     //private int copyTex_kernelID;
// // //     //private int damageUpdate_kernelID;
// //     private int threadGroupSizeX = 8;
// //     private int threadGroupSizeY = 8;
// //     private int vertCount;

// //     public bool green_icon = false;
// //     public GameObject Scalpel;



// // //     //////////////////////////////////////////////////////////////////////


// //     void Start()
// //     {
// //         mainCamera = GameObject.Find("Main Camera");
// //         massSpringSystem = mainCamera.GetComponent<MassSpringSystem>();
// //         positions = new Vector3[massSpringSystem.VertCount];
// //         positions = massSpringSystem.GetPositions();
// //         xDimensions = 120; // 59 quads
// //         zDimensions = 56; // 27 quads
// //         // xDimensions = 60 * 2 - 1;
// //         // zDimensions = 28 * 2 - 1;

// //         // xDimensions = 60;
// //         // zDimensions = 28;
// //         mesh = new Mesh();
// //         mesh.name = gameObject.name;
// //         mesh.vertices = GenerateVerts();
// //         mesh.triangles = GenerateTriangles();
// //         mesh.RecalculateBounds();
// //         mesh.uv = SetTextureUVs();

// //         // uv2 = MakeUV2();
// //         // mesh.uv2 = uv2;

// //         //colors = SetInitialColor();
// //         //mesh.colors = colors;
// //         meshFilter = gameObject.AddComponent<MeshFilter>();
// //         meshFilter.mesh = mesh;
// //         minHeight = -16.0f;
// //         maxHeight = 0.0f;
// //         cutDepthScalpel = -0.1f;
// //         cutDepthStick = -16.0f;
// //         playerPos = new Vector3();
// //         cutList = new Dictionary<int, float>();

// //         //cutRange = 0.5f;

// //         // units of radius are in terms of UV coordinates
// //         // radius = 0.04f;
// //         // dynamicDamageManager = gameObject.GetComponent<DynamicDamageManager>();

// //         // black_uv = new List<Vector2>();

// //         // if (black_marker != null)
// //         // {
// //         //     for (int i = 0; i < black_markerY; i++)
// //         //     {
// //         //         for (int j = 0; j < black_markerX; j++)
// //         //         {
// //         //             pixel = black_marker.GetPixel(j, i);
// //         //             if (pixel.r != 0)
// //         //             {
// //         //                 temp_uv = new Vector2((float)j / black_markerX, (float)i / black_markerY);
// //         //                 black_uv.Add(temp_uv);
// //         //             }
// //         //         }
// //         //     }
// //         // }
// //         // for (int i = 0; i < black_uv.Count; i++)
// //         // {
// //         //     Debug.Log(black_uv[i]);
// //         // }


// //         //////////////////////////////////////////////////////////////////////

// //         vertCount = mesh.vertices.Length;

// //         positionBuffer = new ComputeBuffer(vertCount, sizeof(float) * 3);
// //         positionBuffer.SetData(mesh.vertices);

// //         uvRenderTexture = new RenderTexture(xDimensions, zDimensions, 0, RenderTextureFormat.RGFloat);
// //         uvRenderTexture.enableRandomWrite = true;
// //         uvRenderTexture.Create();

// //         uv_kernelID = uvComputeShader.FindKernel("CSMain");
// //         uvComputeShader.SetBuffer(uv_kernelID, "Positions", positionBuffer);
// //         uvComputeShader.SetFloat("maxDistance", maxDistance);
// //         uvComputeShader.SetInt("tot_vertCount", vertCount);
// //         uvComputeShader.SetTexture(uv_kernelID, "UVMap", uvRenderTexture);
// //         uvComputeShader.SetInt("xDimensions", xDimensions);



// //         shader_graph_material.SetTexture("_UVMap", uvRenderTexture);


// // //         // rewardBuffer = new ComputeBuffer(1, sizeof(int));
// // //         // rewardBuffer.SetData(new int[] { 0 });

// // //         // temp_RWBuffer = new ComputeBuffer(xDimensions * zDimensions, sizeof(float));
// // //         // uvComputeShader.SetBuffer(uv_kernelID, "temp_RWBuffer", temp_RWBuffer);

// // //         // temporary = new ComputeBuffer(vertCount, sizeof(float));
// // //         // float[] temp = new float[vertCount];
// // //         // for (int i = 0; i < vertCount; i++)
// // //         // {
// // //         //     temp[i] = 1.0f;
// // //         // }
// // //         // temporary.SetData(temp);

// // //         // copyBlackTexture = new RenderTexture(xDimensions, zDimensions, 0, RenderTextureFormat.RGFloat);
// // //         // copyBlackTexture.enableRandomWrite = true;
// // //         // copyBlackTexture.Create();

// // //         // copyTex_kernelID = uvComputeShader.FindKernel("copyTexture");
// // //         // uvComputeShader.SetTexture(copyTex_kernelID, "BlackMarker", BlackMarker);
// // //         // uvComputeShader.SetTexture(copyTex_kernelID, "copyBlackTexture", copyBlackTexture);
// // //         // uvComputeShader.SetBuffer(copyTex_kernelID, "temporary", temporary);
// // //         // uvComputeShader.SetBuffer(copyTex_kernelID, "temp_RWBuffer", temp_RWBuffer);

// // //         //uvComputeShader.SetBuffer(uv_kernelID, "rewardBuffer", rewardBuffer);
// // //         //uvComputeShader.SetTexture(uv_kernelID, "copyBlackTexture", copyBlackTexture);
// // //         //uvComputeShader.Dispatch(copyTex_kernelID, x, y, 1);

// // //         // damageUpdate_kernelID = uvComputeShader.FindKernel("damageUpdate");
// // //         // uvComputeShader.SetTexture(damageUpdate_kernelID, "UVMap", uvRenderTexture);
// // //         // uvComputeShader.SetTexture(damageUpdate_kernelID, "copyBlackTexture", copyBlackTexture);


// // //         // Vector2[] uv2 = new Vector2[vertCount];
// // //         // for (int i = 0; i < vertCount; i++)
// // //         // {
// // //         //     uv2[i] = new Vector2((float)i / (vertCount - 1), 0.0f);
// // //         // }
// // //         // mesh.uv2 = uv2;

// // //         Debug.Log($"Dispatching kernel ID: {uv_kernelID}");
// // //         // Debug.Log($"Dispatching kernel ID: {copyTex_kernelID}");
// // //         //Debug.Log($"kernel id: {damageUpdate_kernelID}");
// // //         // float[] temp2 = new float[vertCount];
// // //         // temporary.GetData(temp2);
// // //         // for (int i = 0; i < vertCount; i++)
// // //         // {
// // //         //     Debug.Log(temp2[i]);

// // //         // }
// // //         /////////////////////////////////////////////////////////////////////////

// //     }

// // //     // Update is called once per frame
// //     void FixedUpdate()
// //     {
// //         mesh.RecalculateNormals();
// //         FixPlayerPosition();
// //         //Debug.Log(Scalpel.transform.position);
// //         if (-5.5 <= playerPos.y && playerPos.y <= -3.5f)
// //         {
// //             green_icon = true;
// //         }
// //         else
// //         {
// //             green_icon = false;
// //         }

// //         if (green_icon && Input.GetKey(KeyCode.Space))
// //         {
// //             //////////////////////////////////////////////////////////////////////

// //             uvComputeShader.SetVector("playerPosition", playerPos);
// //             int x = Mathf.CeilToInt(xDimensions / threadGroupSizeX);
// //             int y = Mathf.CeilToInt(zDimensions / threadGroupSizeY);
// //             uvComputeShader.Dispatch(uv_kernelID, x, y, 1);
// //             // uvComputeShader.Dispatch(damageUpdate_kernelID, x, y, 1);


// //             // if (trainingMode)
// //             // {
// //                 // float[] buffer_reward = new float[1];
// //                 // rewardBuffer.GetData(buffer_reward);
// //                 // AddReward(buffer_reward[0]);

// //                 // AsyncGPUReadback is a class that belongs in the Unity.Rendering namespace, .Request is one of the modules in the class
// //                 // OnReadbackComplete take exactly one argument of type AsyncGPUReadbackRequest and return void (nothing).

// //                 // AsyncGPUReadback.Request(ComputeBuffer src, Action<AsyncGPUReadbackRequest> callback = null); format
// //                 // The callback parameter expects an Action<AsyncGPUReadbackRequest>. This means it needs a reference to a method that:

// //                 // Takes one argument of type AsyncGPUReadbackRequest.
// //                 // 
// //                 // Returns nothing (void).
// //                 // 
// //                 // Your OnReadbackComplete method fits this signature perfectly:
// //                 // OnReadbackComplete itself is a method, but the AsyncGPUReadback.Request requires 2 parameters, the GPU buffer to read from
// //                 // and a callback function that it immediatelly runs when done copying data from reward buffer to staging buffer
// //                 // by passing in OnReadbackComplete, you pass in a reference to a method that returns void
// //                 // This reference can be stored in an Action<AsyncGPUReadbackRequest> delegate variable.
// //                 // When you pass OnReadbackComplete as an argument
// //                 // you are implicitly creating an Action<AsyncGPUReadbackRequest>
// //                 // delegate instance that points to your OnReadbackComplete method.

// //                 //AsyncGPUReadback.Request(rewardBuffer, OnReadbackComplete);
// //                 // float[] temp2 = new float[vertCount];
// //                 // temporary.GetData(temp2);
// //                 // Debug.Log(temp2[0]);
// //             // }


// //             /////////////////////////////////////////////////////////////////////////


// //             positions = massSpringSystem.GetPositions();
// //             var verts = mesh.vertices;

// //             int count = 0;
// //             cutDepth = GetCutDepth();
// //             FixPlayerPosition();
// //             Vector3 fixedPlayerPos = new Vector3(playerPos.x + xDimensions / 2, playerPos.y, playerPos.z + zDimensions / 2);

// //             for (int i = 0; i < zDimensions; i++)
// //             {
// //                 for (int j = 0; j < xDimensions; j++)
// //                 {
// //                     int idx = GetIndex(j, i);
// //                     verts[idx] = new Vector3(positions[count].x, positions[count].z, positions[count].y);


// //                     if (j < xDimensions - 1 && i < zDimensions - 1)
// //                     {
// //                         float posY = (positions[count].z + positions[count + 1].z + positions[count + xDimensions].z + positions[count + xDimensions + 1].z) / 4.0f;
// //                         verts[idx + xDimensions] = new Vector3(positions[count].x + 0.5f, posY, positions[count].y + 0.5f);
// //                     }
// //                     if (cutDepth > playerPos.y && j == (int)(fixedPlayerPos.x) && i == (int)(fixedPlayerPos.z))
// //                     {
// //                         int num = 0;
// //                         for (int k = idx - (xDimensions * spread); k <= idx + (xDimensions * spread); k += xDimensions)
// //                         {
// //                             for (int l = num * -1; l < spread * 2 - num; l++)
// //                             {
// //                                 CalcVertHeight(verts[k + l], k + l, playerPos);

// //                                 // if (k + l != idx + xDimensions)
// //                                 // {
// //                                 //     CalcVertHeight(verts[k + l], k + l, playerPos);
// //                                 // }
// //                                 // else
// //                                 // {
// //                                 //     CalcVertHeightCent(verts[k + l], k + l, playerPos, verts);
// //                                 // }
// //                             }
// //                             num++;
// //                         }


// //                     }
// //                     count++;
// //                 }
// //             }
// //             // Debug.Log("this is player position" + playerPos.y);

// //             foreach (var pair in cutList)
// //             {
// //                 verts[pair.Key].y += pair.Value;
// //                 //colors[pair.Key] = gradient.Evaluate(Mathf.InverseLerp(minHeight, maxHeight, pair.Value));
// //                 //colors[pair.Key] = new Color(0.0f, 1.0f, 0.0f, 1.0f);
// //                 // List<int> adjVerts = GetAdjacentVertices(pair.Key);
// //                 // foreach (int vert in adjVerts)
// //                 // {
// //                 //     colors[vert] = colors[pair.Key];
// //                 // } 
// //                 //dynamicDamageManager.ApplyDamage(mesh.uv[pair.Key], radius);


// //             }

// //             // sends array to GPU, transforms it into UV1 or TEXCOORD1 in shader
// //             // each 3 vertices form a triangle
// //             // only adds extra info to each vertex 
// //             //mesh.uv2 = uv2;
// //             mesh.vertices = verts;
// //             //mesh.colors = colors;

// //             mesh.RecalculateBounds();
// //             meshFilter.mesh = mesh;
// //             //uvComputeShader.Dispatch(damageUpdate_kernelID, x, y, 1);
// //         }



// //     }


// //     // this callback method is invoked by unity once gpu readback is complete, so no need for polling, more efficient
// //     // void OnReadbackComplete(AsyncGPUReadbackRequest request)
// //     // {
// //     //     if (request.hasError)
// //     //     {
// //     //         Debug.LogError("GPU Readback Error!");
// //     //         return;
// //     //     }

// //     //     // Get the data once it's ready, it is returned as a NativeArray<T>
// //     //     // so we expect NativeArray<float> with only 1 element
// //     //     NativeArray<int> result = request.GetData<int>();
// //     //     int rewardValue = result[0];
// //     //     // outputs the most current reward value
// //     //     Debug.Log(rewardValue);

// //     //     //AddReward(rewardValue);
// //     // }

// //     void CalcVertHeight(Vector3 vertex, int index, Vector3 playerPos)
// //     {
// //         float vertDist = Distance(playerPos, vertex);
// //         //cutRange = Mathf.Abs(Mathf.Tan(-playerPos.y / 10.0f) * 4f);



// //         // if (vertDist < cutRange)
// //         // {
// //         //findDepth(cutRange, vertDist, playerPos.y);
// //         float depth = Gaussian(vertex, playerPos);
// //         if (cutList.ContainsKey(index))
// //         {
// //             if (depth < cutList[index])
// //             {
// //                 cutList[index] = depth;
// //             }                
// //         }
// //         else
// //         {
// //             cutList.Add(index, depth);
// //         }

// //         //}
// //     }

// //     // void CalcVertHeightCent(Vector3 vertex, int index, Vector3 playerPos, Vector3[] verts)
// //     // {
// //     //     float vertDist = Distance(playerPos, vertex);
// //     //     //cutRange = Mathf.Abs(Mathf.Tan(-playerPos.y / 10.0f) * 10.0f);

// //     //     // if (vertDist < cutRange)
// //     //     // {
// //     //     List<int> adjVerts = GetAdjacentVertices(index);
// //     //     foreach (int vert in adjVerts) // loops through each neighboring vertex
// //     //     {
// //     //         if (trainingMode)
// //     //         {
// //     //             if (uv2[vert].x == 0)
// //     //             {
// //     //                 if (black_uv.Contains(mesh.uv[vert]))
// //     //                 {
// //     //                     // need to import ML Agents package
// //     //                     //AddReward(1.0f);
// //     //                     //reward += 1.0f;
// //     //                 }
// //     //                 else
// //     //                 {
// //     //                     //AddReward(-0.5f);
// //     //                     //reward += -0.5f;
// //     //                 }
// //     //             }
// //     //         }
// //     //         float dist = Distance(playerPos, verts[vert]);
// //     //         float damage = Mathf.InverseLerp(0.0f, 0.5f, dist);
// //     //         if (uv2[vert].x != 1f)
// //     //         {
// //     //             uv2[vert].x = damage;
// //     //         }
// //     //     }
// //     //     // if (uv2[index].x == 0)
// //     //     uv2[index].x = 1.0f;
// //     //     //float depth = findDepth(cutRange, vertDist, playerPos.y);
// //     //     float depth = Gaussian(vertex, playerPos);
// //     //     if (cutList.ContainsKey(index))
// //     //     {
// //     //         if (depth < cutList[index])
// //     //             cutList[index] = depth;
// //     //     }
// //     //     else
// //     //     {
// //     //         cutList.Add(index, depth);
// //     //     }
// //     //     //}
// //     // }

// //     // float findDepth(float cutRange, float vertDist, float playerDepth)
// //     // {
// //     //     return (cutRange - vertDist) * playerDepth / cutRange;


// //     // }

// //     float GetCutDepth()
// //     {
// //         if (GameObject.FindGameObjectWithTag("Stick").GetComponent<CharacterController>().enabled == true)
// //         {
// //             playerPos = GameObject.FindGameObjectWithTag("Stick").transform.position;
// //             return cutDepthStick;
// //         }
// //         playerPos = GameObject.FindGameObjectWithTag("Scalpel").transform.position;
// //         return cutDepthScalpel;
// //     }

// //     void FixPlayerPosition()
// //     {
// //         //playerPos.y -= 12f;
// //         playerPos.y = Scalpel.transform.position.y - 13.0f;
// //     }


// //     private Vector3[] GenerateVerts()
// //     {
// //         positions = massSpringSystem.GetPositions();
// //         Vector3[] verts = new Vector3[(2 * xDimensions - 1) * zDimensions];
// //         int count = 0;
// //         for (int i = 0; i < zDimensions; i++)
// //         {
// //             for (int j = 0; j < xDimensions; j++)
// //             {
// //                 verts[GetIndex(j, i)] = new Vector3(positions[count].x, positions[count].z, positions[count].y);
// //                 if (j < xDimensions - 1 && i < zDimensions - 1)
// //                     verts[GetIndex(j + xDimensions, i)] = new Vector3(positions[count].x + 0.5f, positions[count].z, positions[count].y + 0.5f);
// //                 count++;

// //             }
// //         }

// //         return verts;
// //     }

// //     public int GetIndex(int x, int z)
// //     {
// //         return z * (2 * xDimensions - 1) + x;
// //     }

// //     protected int[] GenerateTriangles()
// //     {
// //         int[] triangles = new int[mesh.vertices.Length * 12];

// //         for (int i = 0; i < zDimensions - 1; i++)
// //         {
// //             for (int j = 0; j < xDimensions - 1; j++)
// //             {
// //                 triangles[GetIndex(j, i) * 12] = GetIndex(j + 1, i);
// //                 triangles[GetIndex(j, i) * 12 + 1] = GetIndex(j, i);
// //                 triangles[GetIndex(j, i) * 12 + 2] = GetIndex(j + xDimensions, i);
// //                 triangles[GetIndex(j, i) * 12 + 3] = GetIndex(j, i + 1);
// //                 triangles[GetIndex(j, i) * 12 + 4] = GetIndex(j + 1, i + 1);
// //                 triangles[GetIndex(j, i) * 12 + 5] = GetIndex(j + xDimensions, i);
// //                 triangles[GetIndex(j, i) * 12 + 6] = GetIndex(j, i);
// //                 triangles[GetIndex(j, i) * 12 + 7] = GetIndex(j, i + 1);
// //                 triangles[GetIndex(j, i) * 12 + 8] = GetIndex(j + xDimensions, i);
// //                 triangles[GetIndex(j, i) * 12 + 9] = GetIndex(j + 1, i + 1);
// //                 triangles[GetIndex(j, i) * 12 + 10] = GetIndex(j + 1, i);
// //                 triangles[GetIndex(j, i) * 12 + 11] = GetIndex(j + xDimensions, i);
// //             }
// //         }

// //         return triangles;
// //     }

// //     private Vector2[] SetTextureUVs()
// //     {
// //         Vector2[] uvs = new Vector2[mesh.vertices.Length];

// //         float xStep = 1.0f / (float)(xDimensions - 1);
// //         float zStep = 1.0f / (float)(zDimensions - 1);
// //         int idx = 0;
// //         for (int i = 0; i < zDimensions; i++)
// //         {
// //             for (int j = 0; j < xDimensions; j++)
// //             {
// //                 idx = GetIndex(j, i);
// //                 uvs[idx] = new Vector2(j * xStep, i * zStep);
// //                 if (j < xDimensions - 1 && i < zDimensions - 1)
// //                 {
// //                     uvs[idx + xDimensions] = new Vector2((j + 0.5f) * xStep, (i + 0.5f) * zStep);
// //                 }
// //             }
// //         }
// //         return uvs;
// //     }

// //     private Color[] SetInitialColor()
// //     {
// //         Color[] clrs = new Color[mesh.vertices.Length];
// //         for (int i = 0; i < clrs.Length; i++)
// //         {
// //             // sets the surface skin color
// //             clrs[i] = new Color(0.73f, 0.15f, 0.08f, 1.0f);
// //             //clrs[i] = new Color(0f, 0f, 0f, 1.0f);
// //         }
// //         return clrs;
// //     }

// //     private List<int> GetAdjacentVertices(int vertIndex)
// //     {
// //         // mesh is upside down, bottom is up
// //         List<int> adjVertices = new List<int>();
// //         int count = 0;

// //         for (int i = vertIndex - (xDimensions * spread); i <= vertIndex + (xDimensions * spread); i += xDimensions)
// //         {
// //             for (int j = count * -1; j < 2 * spread - count; j++)
// //             {
// //                 adjVertices.Add(i + j);
// //             }

// //             count++;
// //         }

// //         return adjVertices;

// //     }

// //     private List<int> GetSquareVertices(int vertIndex)
// //     {
// //         List<int> adjVertices = new List<int>();

// //         adjVertices.Add(vertIndex);
// //         adjVertices.Add(vertIndex + 1);
// //         adjVertices.Add(vertIndex + xDimensions);
// //         adjVertices.Add(vertIndex + xDimensions + 1);

// //         return adjVertices;

// //     }

// //     // private Vector2[] MakeUV2()
// //     // {
// //     //     Vector2[] temp_uv2 = new Vector2[mesh.vertices.Length];
// //     //     for (int i = 0; i < mesh.vertices.Length; i++)
// //     //     {
// //     //         temp_uv2[i] = Vector2.zero;
// //     //     }
// //     //     return temp_uv2;
// //     // }

// //     private float Distance(Vector3 playerPosition, Vector3 vertex)
// //     {
// //         float xDist = playerPosition.x - vertex.x;
// //         float zDist = playerPosition.z - vertex.z;
// //         float dist = Mathf.Sqrt(xDist * xDist + zDist * zDist);
// //         return dist;
// //     }

// //     private float Gaussian(Vector3 vertex, Vector3 playerPos)
// //     {
// //         float distance = Distance(playerPos, vertex);
// //         float sigma = heightEffectiveRadius / heightCurveSteepness;
// //         if (sigma < 0.001f)
// //         {
// //             sigma = 0.001f;
// //         }
// //         float gaussianValue = Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(sigma, 2)));

// //         return gaussianValue * playerPos.y;
// //     }

// //     private void OnDestroy()
// //     {
// //         if (uvRenderTexture != null)
// //         {
// //             uvRenderTexture.Release();
// //         }
// //         if (positionBuffer != null)
// //         {
// //             positionBuffer.Dispose();
// //         }
// //         // if (rewardBuffer != null)
// //         // {
// //         //     rewardBuffer.Dispose();
// //         // }
// //         // if (copyBlackTexture != null)
// //         // {
// //         //     copyBlackTexture.Release();
// //         // }
// //         // if (temporary != null)
// //         // {
// //         //     temporary.Dispose();
// //         // }
// //         // if (temp_RWBuffer != null)
// //         // {
// //         //     temp_RWBuffer.Release();
// //         // }
// //     }
// // }

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class MeshHandle : MonoBehaviour
// {
//     //private int resolutionFactor;
//     protected Mesh mesh;
//     private MeshFilter meshFilter;
//     private int xDimensions, zDimensions;
//     public GameObject mainCamera;
//     private MassSpringSystem massSpringSystem;
//     private Vector3[] positions;
//     float cutDepthStick, cutDepthScalpel, cutDepth, currentDepth;
//     private Vector3 playerPos;
//     private Dictionary<int, float> cutList;
//     public Gradient gradient;
//     private float minHeight, maxHeight;
//     //private Color[] colors;
    
//     //private float cutRange;


//     private float temporary;

//     // public float radius;
//     // public DynamicDamageManager dynamicDamageManager;
//     //private Vector2[] uv2;

//     // private List<Vector2> black_uv;
//     // public Texture2D black_marker;
//     // public int black_markerX = 120;
//     // public int black_markerY = 56;

//     // currently false
//     public bool trainingMode = false;
//     private Color pixel;
//     private Vector2 temp_uv;


//     private int spread = 20;
//     // public float minVertexHeightAtPlayer = 0.1f;
//     // public float maxVertexHeightFarAway = 5f;
//     public float heightEffectiveRadius = 5f;
//     public float heightCurveSteepness = 2f;


//     //////////////////////////////////////////////////////////////////////

//     public ComputeShader uvComputeShader;
//     public Material shader_graph_material;
//     public float maxDistance = 20f;

//     private ComputeBuffer positionBuffer;
//     private RenderTexture uvRenderTexture;
//     private int kernelID;
//     private int threadGroupSizeX = 8;
//     private int threadGroupSizeY = 8;
//     private int vertCount;


//     public bool green_icon = false;
//     public GameObject Scalpel;


//     //////////////////////////////////////////////////////////////////////


//     void Start()
//     {
//         mainCamera = GameObject.Find("Main Camera");
//         massSpringSystem = mainCamera.GetComponent<MassSpringSystem>();
//         positions = new Vector3[massSpringSystem.VertCount];
//         positions = massSpringSystem.GetPositions();
//         xDimensions = 120; // 59 quads
//         zDimensions = 56; // 27 quads
//         // xDimensions = 60 * 2 - 1;
//         // zDimensions = 28 * 2 - 1;

//         // xDimensions = 60;
//         // zDimensions = 28;
//         mesh = new Mesh();
//         mesh.name = gameObject.name;
//         mesh.vertices = GenerateVerts();
//         mesh.triangles = GenerateTriangles();
//         mesh.RecalculateBounds();
//         mesh.uv = SetTextureUVs();

//         // uv2 = MakeUV2();
//         // mesh.uv2 = uv2;

//         //colors = SetInitialColor();
//         //mesh.colors = colors;
//         meshFilter = gameObject.AddComponent<MeshFilter>();
//         meshFilter.mesh = mesh;
//         minHeight = -16.0f;
//         maxHeight = 0.0f;
//         cutDepthScalpel = -0.1f;
//         cutDepthStick = -16.0f;
//         playerPos = new Vector3();
//         cutList = new Dictionary<int, float>();

//         //cutRange = 0.5f;

//         // units of radius are in terms of UV coordinates
//         // radius = 0.04f;
//         // dynamicDamageManager = gameObject.GetComponent<DynamicDamageManager>();

//         // black_uv = new List<Vector2>();

//         // if (black_marker != null)
//         // {
//         //     for (int i = 0; i < black_markerY; i++)
//         //     {
//         //         for (int j = 0; j < black_markerX; j++)
//         //         {
//         //             pixel = black_marker.GetPixel(j, i);
//         //             if (pixel.r != 0)
//         //             {
//         //                 temp_uv = new Vector2((float)j / black_markerX, (float)i / black_markerY);
//         //                 black_uv.Add(temp_uv);
//         //             }
//         //         }
//         //     }
//         // }
//         // for (int i = 0; i < black_uv.Count; i++)
//         // {
//         //     Debug.Log(black_uv[i]);
//         // }


//         //////////////////////////////////////////////////////////////////////

//         vertCount = mesh.vertices.Length;

//         positionBuffer = new ComputeBuffer(vertCount, sizeof(float) * 3);
//         positionBuffer.SetData(mesh.vertices);

//         uvRenderTexture = new RenderTexture(120, 56, 0, RenderTextureFormat.RGFloat);
//         uvRenderTexture.enableRandomWrite = true;
//         uvRenderTexture.Create();


//         kernelID = uvComputeShader.FindKernel("CSMain");
//         uvComputeShader.SetBuffer(kernelID, "Positions", positionBuffer);
//         uvComputeShader.SetFloat("maxDistance", maxDistance);
//         uvComputeShader.SetInt("tot_vertCount", vertCount);
//         uvComputeShader.SetTexture(kernelID, "UVMap", uvRenderTexture);
//         uvComputeShader.SetInt("xDimensions", xDimensions);

//         shader_graph_material.SetTexture("_UVMap", uvRenderTexture);

//         // Vector2[] uv2 = new Vector2[vertCount];
//         // for (int i = 0; i < vertCount; i++)
//         // {
//         //     uv2[i] = new Vector2((float)i / (vertCount - 1), 0.0f);
//         // }
//         // mesh.uv2 = uv2;


//         /////////////////////////////////////////////////////////////////////////

//     }

//     // Update is called once per frame
//     void FixedUpdate()
//     {
//         mesh.RecalculateNormals();
//         FixPlayerPosition();
//         //Debug.Log(Scalpel.transform.position);
//         if (-5.5 <= playerPos.y && playerPos.y <= -3.5f)
//         {
//             green_icon = true;
//         }
//         else
//         {
//             green_icon = false;
//         }

//         if (green_icon && Input.GetKey(KeyCode.Space))
//         {
//             // if (trainingMode)
//             // {
//             //     AddReward(1.0f);
//             // }
//             positions = massSpringSystem.GetPositions();
//             var verts = mesh.vertices;

//             int count = 0;
//             cutDepth = GetCutDepth();
//             FixPlayerPosition();
//             Vector3 fixedPlayerPos = new Vector3(playerPos.x + xDimensions / 2, playerPos.y, playerPos.z + zDimensions / 2);

//             for (int i = 0; i < zDimensions; i++)
//             {
//                 for (int j = 0; j < xDimensions; j++)
//                 {
//                     int idx = GetIndex(j, i);
//                     verts[idx] = new Vector3(positions[count].x, positions[count].z, positions[count].y);


//                     if (j < xDimensions - 1 && i < zDimensions - 1)
//                     {
//                         float posY = (positions[count].z + positions[count + 1].z + positions[count + xDimensions].z + positions[count + xDimensions + 1].z) / 4.0f;
//                         verts[idx + xDimensions] = new Vector3(positions[count].x + 0.5f, posY, positions[count].y + 0.5f);
//                     }
//                     if (cutDepth > playerPos.y && j == (int)(fixedPlayerPos.x) && i == (int)(fixedPlayerPos.z))
//                     {
//                         int num = 0;
//                         for (int k = idx - (xDimensions * spread); k <= idx + (xDimensions * spread); k += xDimensions)
//                         {
//                             for (int l = num * -1; l < spread * 2 - num; l++)
//                             {
//                                 CalcVertHeight(verts[k + l], k + l, playerPos);

//                                 // if (k + l != idx + xDimensions)
//                                 // {
//                                 //     CalcVertHeight(verts[k + l], k + l, playerPos);
//                                 // }
//                                 // else
//                                 // {
//                                 //     CalcVertHeightCent(verts[k + l], k + l, playerPos, verts);
//                                 // }
//                             }
//                             num++;
//                         }


//                     }
//                     count++;
//                 }
//             }
//             // Debug.Log("this is player position" + playerPos.y);

//             foreach (var pair in cutList)
//             {
//                 verts[pair.Key].y += pair.Value;
//                 //colors[pair.Key] = gradient.Evaluate(Mathf.InverseLerp(minHeight, maxHeight, pair.Value));
//                 //colors[pair.Key] = new Color(0.0f, 1.0f, 0.0f, 1.0f);
//                 // List<int> adjVerts = GetAdjacentVertices(pair.Key);
//                 // foreach (int vert in adjVerts)
//                 // {
//                 //     colors[vert] = colors[pair.Key];
//                 // } 
//                 //dynamicDamageManager.ApplyDamage(mesh.uv[pair.Key], radius);


//             }

//             // sends array to GPU, transforms it into UV1 or TEXCOORD1 in shader
//             // each 3 vertices form a triangle
//             // only adds extra info to each vertex 
//             //mesh.uv2 = uv2;
//             mesh.vertices = verts;
//             //mesh.colors = colors;

//             mesh.RecalculateBounds();
//             meshFilter.mesh = mesh;

//             //////////////////////////////////////////////////////////////////////

//             uvComputeShader.SetVector("playerPosition", playerPos);
//             int x = Mathf.CeilToInt(xDimensions / threadGroupSizeX);
//             int y = Mathf.CeilToInt(zDimensions / threadGroupSizeY);
//             uvComputeShader.Dispatch(kernelID, x, y, 1);

//             /////////////////////////////////////////////////////////////////////////

//         }
            
//     }

//     void CalcVertHeight(Vector3 vertex, int index, Vector3 playerPos)
//     {
//         float vertDist = Distance(playerPos, vertex);
//         //cutRange = Mathf.Abs(Mathf.Tan(-playerPos.y / 10.0f) * 4f);



//         // if (vertDist < cutRange)
//         // {
//         //findDepth(cutRange, vertDist, playerPos.y);
//         float depth = Gaussian(vertex, playerPos);
//         if (cutList.ContainsKey(index))
//         {
//             if (depth < cutList[index])
//             {
//                 cutList[index] = depth;
//             }                
//         }
//         else
//         {
//             cutList.Add(index, depth);
//         }

//         //}
//     }


//     float GetCutDepth()
//     {
//         if (GameObject.FindGameObjectWithTag("Stick").GetComponent<CharacterController>().enabled == true)
//         {
//             playerPos = GameObject.FindGameObjectWithTag("Stick").transform.position;
//             return cutDepthStick;
//         }
//         playerPos = GameObject.FindGameObjectWithTag("Scalpel").transform.position;
//         return cutDepthScalpel;
//     }

//     void FixPlayerPosition()
//     {
//         //playerPos.y -= 12f;
//         playerPos.y = Scalpel.transform.position.y - 13.0f;
//     }


//     private Vector3[] GenerateVerts()
//     {
//         positions = massSpringSystem.GetPositions();
//         Vector3[] verts = new Vector3[(2 * xDimensions - 1) * zDimensions];
//         int count = 0;
//         for (int i = 0; i < zDimensions; i++)
//         {
//             for (int j = 0; j < xDimensions; j++)
//             {
//                 verts[GetIndex(j, i)] = new Vector3(positions[count].x, positions[count].z, positions[count].y);
//                 if (j < xDimensions - 1 && i < zDimensions - 1)
//                     verts[GetIndex(j + xDimensions, i)] = new Vector3(positions[count].x + 0.5f, positions[count].z, positions[count].y + 0.5f);
//                 count++;

//             }
//         }

//         return verts;
//     }

//     public int GetIndex(int x, int z)
//     {
//         return z * (2 * xDimensions - 1) + x;
//     }

//     protected int[] GenerateTriangles()
//     {
//         int[] triangles = new int[mesh.vertices.Length * 12];

//         for (int i = 0; i < zDimensions - 1; i++)
//         {
//             for (int j = 0; j < xDimensions - 1; j++)
//             {
//                 triangles[GetIndex(j, i) * 12] = GetIndex(j + 1, i);
//                 triangles[GetIndex(j, i) * 12 + 1] = GetIndex(j, i);
//                 triangles[GetIndex(j, i) * 12 + 2] = GetIndex(j + xDimensions, i);
//                 triangles[GetIndex(j, i) * 12 + 3] = GetIndex(j, i + 1);
//                 triangles[GetIndex(j, i) * 12 + 4] = GetIndex(j + 1, i + 1);
//                 triangles[GetIndex(j, i) * 12 + 5] = GetIndex(j + xDimensions, i);
//                 triangles[GetIndex(j, i) * 12 + 6] = GetIndex(j, i);
//                 triangles[GetIndex(j, i) * 12 + 7] = GetIndex(j, i + 1);
//                 triangles[GetIndex(j, i) * 12 + 8] = GetIndex(j + xDimensions, i);
//                 triangles[GetIndex(j, i) * 12 + 9] = GetIndex(j + 1, i + 1);
//                 triangles[GetIndex(j, i) * 12 + 10] = GetIndex(j + 1, i);
//                 triangles[GetIndex(j, i) * 12 + 11] = GetIndex(j + xDimensions, i);
//             }
//         }

//         return triangles;
//     }

//     private Vector2[] SetTextureUVs()
//     {
//         Vector2[] uvs = new Vector2[mesh.vertices.Length];

//         float xStep = 1.0f / (float)(xDimensions - 1);
//         float zStep = 1.0f / (float)(zDimensions - 1);
//         int idx = 0;
//         for (int i = 0; i < zDimensions; i++)
//         {
//             for (int j = 0; j < xDimensions; j++)
//             {
//                 idx = GetIndex(j, i);
//                 uvs[idx] = new Vector2(j * xStep, i * zStep);
//                 if (j < xDimensions - 1 && i < zDimensions - 1)
//                 {
//                     uvs[idx + xDimensions] = new Vector2((j + 0.5f) * xStep, (i + 0.5f) * zStep);
//                 }
//             }
//         }
//         return uvs;
//     }

//     private Color[] SetInitialColor()
//     {
//         Color[] clrs = new Color[mesh.vertices.Length];
//         for (int i = 0; i < clrs.Length; i++)
//         {
//             // sets the surface skin color
//             clrs[i] = new Color(0.73f, 0.15f, 0.08f, 1.0f);
//             //clrs[i] = new Color(0f, 0f, 0f, 1.0f);
//         }
//         return clrs;
//     }

//     private List<int> GetAdjacentVertices(int vertIndex)
//     {
//         // mesh is upside down, bottom is up
//         List<int> adjVertices = new List<int>();
//         int count = 0;

//         for (int i = vertIndex - (xDimensions * spread); i <= vertIndex + (xDimensions * spread); i += xDimensions)
//         {
//             for (int j = count * -1; j < 2 * spread - count; j++)
//             {
//                 adjVertices.Add(i + j);
//             }

//             count++;
//         }

//         return adjVertices;

//     }

//     private List<int> GetSquareVertices(int vertIndex)
//     {
//         List<int> adjVertices = new List<int>();

//         adjVertices.Add(vertIndex);
//         adjVertices.Add(vertIndex + 1);
//         adjVertices.Add(vertIndex + xDimensions);
//         adjVertices.Add(vertIndex + xDimensions + 1);

//         return adjVertices;

//     }

//     // private Vector2[] MakeUV2()
//     // {
//     //     Vector2[] temp_uv2 = new Vector2[mesh.vertices.Length];
//     //     for (int i = 0; i < mesh.vertices.Length; i++)
//     //     {
//     //         temp_uv2[i] = Vector2.zero;
//     //     }
//     //     return temp_uv2;
//     // }

//     private float Distance(Vector3 playerPosition, Vector3 vertex)
//     {
//         float xDist = playerPosition.x - vertex.x;
//         float zDist = playerPosition.z - vertex.z;
//         float dist = Mathf.Sqrt(xDist * xDist + zDist * zDist);
//         return dist;
//     }

//     private float Gaussian(Vector3 vertex, Vector3 playerPos)
//     {
//         float distance = Distance(playerPos, vertex);
//         float sigma = heightEffectiveRadius / heightCurveSteepness;
//         if (sigma < 0.001f)
//         {
//             sigma = 0.001f;
//         }
//         float gaussianValue = Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(sigma, 2)));

//         return gaussianValue * playerPos.y;
//     }

//     private void OnDestroy()
//     {
//         if (uvRenderTexture != null)
//         {
//             uvRenderTexture.Release();
//         }
//         if (positionBuffer != null)
//         {
//             positionBuffer.Dispose();
//         }
//     }
// }

















// // Each #kernel tells which function to compile; you can have many kernels
// #pragma kernel CSMain
// // #pragma kernel copyTexture
// // #pragma kernel damageUpdate
// // Create a RenderTexture with enableRandomWrite flag and set it
// // with cs.SetTexture

// // indexed in terms of pixels not UV space, so dont need to normalie final coordinates
// // RW is read write, without it just read
// RWTexture2D<float2> UVMap;
// // RWTexture2D<float2> copyBlackTexture;
// // Texture2D<float4> BlackMarker;
// // RWStructuredBuffer<int> rewardBuffer;
// StructuredBuffer<float3> Positions;

// // RWStructuredBuffer<float> temp_RWBuffer;
// // RWStructuredBuffer<float> temporary;

// uint tot_vertCount;
// float maxDistance;
// float3 playerPosition;
// int xDimensions;


// // [numthreads(8,8,1)]
// // void copyTexture (uint3 id : SV_DispatchThreadID)
// // {
// //     uint2 coordinate = id.xy;
// //     copyBlackTexture[coordinate] = float2(BlackMarker[coordinate].r, 0.0f);
// //     //temporary[(2 * xDimensions - 1) * id.y + id.x] = copyBlackTexture[coordinate].r;

// //     //temp_RWBuffer[xDimensions * id.y + id.x] = copyBlackTexture[coordinate].r;
    
// // }


// [numthreads(8,8,1)]
// // uint3 is a Vector3, basically this parameter is the index of the vertex
// void CSMain (uint3 id : SV_DispatchThreadID)
// {
//     // TODO: insert actual code here!
//     // y is height and x is width, y is the outer forloop, x is the inner forloop
//     uint xIndex = id.x;
//     uint yIndex = id.y;

//     uint index = (2 * xDimensions - 1) * yIndex + xIndex;

//     if (index >= tot_vertCount) return;

//     float3 position = Positions[index];
//     float xDist = playerPosition.x - position.x;
//     float zDist = playerPosition.z - position.z;
//     float dist = sqrt(xDist * xDist + zDist * zDist);
//     // float dist = abs(distance(position, playerPosition));    
    
//     // blend factor from 1-0
//     float blend = pow(saturate(1.0 - dist / maxDistance), 1.0 / 4.0);

//     // write result to output 2D texture
//     UVMap[int2(xIndex, yIndex)] = float2(max(UVMap[int2(xIndex, yIndex)].x, blend), 0.0f);
    
//     // if (UVMap[int2(xIndex, yIndex)].x != 0)
//     // {
//     //     InterlockedAdd(rewardBuffer[0], 1);
//     // }
    
    
//     // if (UVMap[int2(xIndex, yIndex)].x != 0) // must have been damage
//     // {
//     //     if (temp_RWBuffer[xDimensions * id.y + id.x] != 0.5f) // new damage
//     //     {
//     //         if (temp_RWBuffer[xDimensions * id.y + id.x] == 1.0f) // black marker that hasn't been damaged yet
//     //         {
//     //             InterlockedAdd(rewardBuffer[0], 5);
//     //         }
//     //         else // must mean it is 0, hitting good skin
//     //         {
//     //             InterlockedAdd(rewardBuffer[0], -2);
//     //         }
//     //     }

//     //     copyBlackTexture[id.xy] = float2(0.5f, 0.0f);
//     // }
 

//     // if (UVMap[int2(xIndex, yIndex)].x != 0) // must have been damaged
//     // {
//     //     if (copyBlackTexture[int2(xIndex, yIndex)].x != 0.5f) // new damage
//     //     {
//     //         if (abs(copyBlackTexture[int2(xIndex, yIndex)].x - 1) < 0.1) // black marker that hasn't been damaged yet
//     //         {
//     //             InterlockedAdd(rewardBuffer[0], 1);
//     //         }
//     //         else // must mean it is 0, hitting good skin
//     //         {
//     //             if (rewardBuffer[0] == 0)
//     //             {
//     //                 InterlockedAdd(rewardBuffer[0], -1);
//     //             }
//     //         }
//     //     }
//     // }
//     /// include consideration of boundaries
//     // copyblacktexture not copying black texture right?
//     /// saying that all of copyBlackTexture is 0 even though temporary shows that there are 1
// }


// // [numthreads(8,8,1)]
// // void damageUpdate (uint3 id : SV_DispatchThreadID)
// // {
// //     uint2 coord = id.xy;
    
// //     if (UVMap[coord].x != 0)
// //     {
// //         copyBlackTexture[coord] = float2(0.5f, 0.0f);
// //     }
// // }