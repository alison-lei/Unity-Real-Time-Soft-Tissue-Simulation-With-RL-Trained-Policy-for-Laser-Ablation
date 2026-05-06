using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class AgentScript : Agent
{
    // initializing variables
    public bool trainingMode;
    public PlayerController playerController;
    public MassSpringSystem massSpringSystem;
    public MeshHandle meshHandle;
    public GameObject scalpel;


    public float horizontalInput = 0.0f;
    public float yMoveVec = 0.0f;
    public float verticalInput = 0.0f;

    public bool doneTrace = false;
    public float? lastReward = null;
    private float onesLeft;
    private float totalPatternPixels;
    public RenderTexture ovalShaderPattern;
    public Material shaderGraph;
    public Texture2D fullPatternTexture;

    private float gReward;
    private float startTime;
    private Vector3 vecDirection;

    
    private Vector3 startPos;
    private float radiusSqr = 16f;
    private Vector3 vertexVector;
    private bool laserOn = false;
    private float scoreIncrement = 1f;
    private float campingPenalty = -0.1f;
    private float campingScore = 0f;
    private float campingThreshold = 250f;
    private int relocationCounter = 0;
    public float relocationDistance = 25f;
    public int relocationSteps = 60;
    private bool calculateCloseVertex;
    private int Width = 60;
    private int Height = 28;
    private int edgeMargin = 8;
    private float lastPercentage;
    private float endTimeCounter = 0f;
    private int hitAgainCounter = 0;
    private bool activeAgain = false;

    /// <summary>
    /// Extremely important to include the withinRadius bool
    /// Acts as an internal flag to promote incremental learning and keep reward shaping positive
    /// Before incorporating this, agent would not go to the lesion and start ablating immediately before reaching the lesion
    /// Added it as an observation so agent learns to associate it with lesion area where the highest concentration of positive reward is
    /// </summary>
    
    private float withinRadius;


    void Start()
    {
        // setting the render texture to the shader graph
        // Use RGFloat because want two channels, 32-bit float each, store two values per coordinate
        ovalShaderPattern = new RenderTexture(meshHandle.xDimensions, meshHandle.zDimensions, 0, RenderTextureFormat.RGFloat);
        ovalShaderPattern.enableRandomWrite = true;
        ovalShaderPattern.Create();
        shaderGraph.SetTexture("_ovalShaderPattern", ovalShaderPattern);

        totalPatternPixels = 0f;

        fullPatternTexture = GenerateOval();

        onesLeft = totalPatternPixels;
        vecDirection = Vector3.zero;
        gReward = 0f;
        calculateCloseVertex = false;
        lastPercentage = 0f;
        endTimeCounter = 0f;
        hitAgainCounter = 0;
        withinRadius = 0f;

    }

    // Downsizing the texture using bilinear filtering, averages neighboring pixels
    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 0);
        rt.filterMode = FilterMode.Bilinear;

        Graphics.Blit(source, rt);

        RenderTexture.active = rt;
        Texture2D tempTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tempTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tempTex.Apply();
        RenderTexture.active = null;

        return tempTex;
    }

    // function to generate random oval-shapped pattern to mimic appearance of endometriosis lesions
    public Texture2D GenerateOval()
    {
        Texture2D tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[Width * Height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;
        tex.SetPixels(pixels);

        float targetArea = Random.Range(60f, 120f);

        float aspectRatio = Random.Range(0.5f, 2.0f);
        float rx = Mathf.Sqrt((targetArea / Mathf.PI) * aspectRatio);
        float ry = rx / aspectRatio;


        float centerX = Random.Range(edgeMargin + rx, Width - edgeMargin - rx);
        float centerY = Random.Range(edgeMargin + ry, Height - edgeMargin - ry);

        float rad = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;

                float rotatedX = dx * Mathf.Cos(rad) - dy * Mathf.Sin(rad);
                float rotatedY = dx * Mathf.Sin(rad) + dy * Mathf.Cos(rad);

                if ((rotatedX * rotatedX) / (rx * rx) + (rotatedY * rotatedY) / (ry * ry) <= 1f)
                {
                    tex.SetPixel(x, y, Color.white);
                    totalPatternPixels += 1.0f;
                }
            }
        }
        
        tex.Apply();

        Graphics.Blit(tex, ovalShaderPattern);

        return tex;
    }


    public override void OnEpisodeBegin()
    {
        playerController.homePosition();
        totalPatternPixels = 0f;
        fullPatternTexture = GenerateOval();
        meshHandle.spaceBar = false;
        laserOn = false;
        activeAgain = false;
        withinRadius = 0f;
        meshHandle.allInitialized = false;
        massSpringSystem.ResetBuffers();
        if (doneTrace)
            doneTrace = false;
        lastReward = null;
        onesLeft = totalPatternPixels;
        vecDirection = Vector3.zero;
        calculateCloseVertex = false;

        // Resets all render texture buffers to 0
        meshHandle.ResetRTBuffers();
        playerController.resetBoundsFlag();

        gReward = 0f;
        startTime = Time.time;

        startPos = meshHandle.playerPos;

        campingScore = 0f;
        relocationCounter = 0;
        lastPercentage = 0f;
        hitAgainCounter = 0;

        if (meshHandle.allInitialized)
        {
            meshHandle.CalculateReward();
            AsyncGPUReadback.Request(meshHandle.rewardBuffer, OnReadbackComplete);
        }

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (vecDirection == Vector3.zero)
            sensor.AddObservation(vecDirection);
        else
            sensor.AddObservation(vecDirection.normalized); // 3

        sensor.AddObservation(vecDirection.magnitude); // 1

        if (vecDirection == Vector3.zero)
        {
            sensor.AddObservation(0f); // 1
        }
        else
        {
            sensor.AddObservation(Vector3.Dot(vecDirection.normalized, new Vector3(horizontalInput, 0f, verticalInput).normalized)); // 1
        }

        sensor.AddObservation(meshHandle.playerPos); // 3

        lastPercentage = PercentageComplete();
        sensor.AddObservation(lastPercentage); // 1

        sensor.AddObservation(endTimeCounter);
        sensor.AddObservation(withinRadius);

    }


    public override void OnActionReceived(ActionBuffers actions)
    {

        horizontalInput = actions.ContinuousActions[0];
        verticalInput = actions.ContinuousActions[1];

        if (horizontalInput != 0f || verticalInput != 0f)
            activeAgain = true;

        playerController.MoveLaser(horizontalInput, verticalInput);

        Vector3 lastMovement = new Vector3(horizontalInput, 0f, verticalInput);
        if (vecDirection != Vector3.zero && lastMovement != Vector3.zero)
        {
            // reward for going in the direction of the nearest pettern vertex
            float dirBonus = Mathf.Clamp(0.001f * Vector3.Dot(vecDirection.normalized, lastMovement.normalized), 0f, 0.001f);
            if (dirBonus > 0.0001f)
            {
                gReward += dirBonus;
            }
        }

        int activation = actions.DiscreteActions[0] == 1 ? 1 : 0;
        if (activation == 1 && activeAgain)
        {
            laserOn = true;
        }
        else
        {
            laserOn = false;
        }
        if (meshHandle.playerPos.y < -1.0f)
            meshHandle.spaceBar = laserOn;


        // camping reward system
        Vector3 curPos = meshHandle.playerPos;
        float distSqr = (curPos - startPos).sqrMagnitude;
        bool inside = distSqr <= radiusSqr;

        if (inside)
        {
            campingScore += scoreIncrement;
        }

        if (campingScore >= campingThreshold)
        {
            gReward += campingPenalty;
            startPos = meshHandle.playerPos;
            campingScore = 0f;
        }

        if (distSqr >= relocationDistance)
        {
            relocationCounter++;
            if (relocationCounter >= relocationSteps)
            {
                startPos = meshHandle.playerPos;
                campingScore = 0f;           
                relocationCounter = 0;
                gReward += 0.005f;
            }
        }
        else
        {
            relocationCounter = 0;
        }


        if (Time.frameCount % 4 == 0 && meshHandle.allInitialized)
        {
            meshHandle.CalculateReward();
            AsyncGPUReadback.Request(meshHandle.rewardBuffer, OnReadbackComplete);
        }


        endTimeCounter += 1f;
        if (endTimeCounter % 2999f == 0f)
            ConsiderTime();


        if (gReward != 0.0f)
        {
            AddReward(gReward);
        }
        gReward = 0f;
        
        if (doneTrace || PercentageComplete() > 0.9f)
        {
            playerController.homePosition();
            AddReward(5.0f);
            ConsiderTime();
            EndEpisode();
        }


        if (playerController.outofBounds)
        {
            playerController.homePosition();
            ConsiderTime();
            EndEpisode();
        }

    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // of type NativeArray<float>
        var continuous = actionsOut.ContinuousActions;
        var discrete = actionsOut.DiscreteActions;

        continuous[0] = Input.GetAxis("Horizontal");
        continuous[1] = Input.GetAxis("Vertical");

        discrete[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }


    public void ConsiderTime()
    {
        float timePunish = Mathf.Clamp(-Mathf.Pow(2f, 0.02f * (Time.time - startTime)) + 1f, -4f, 0f);
        gReward += timePunish;
    }


    private float PercentageComplete()
    {
        return 1.0f - (onesLeft / totalPatternPixels);
    }


    private void OnReadbackComplete(AsyncGPUReadbackRequest request)
    {
        NativeArray<int> result = request.GetData<int>();
        float rewardValue = (float)result[0] / 40.0f;
        int done = result[1];
        onesLeft = (float)result[2];
        vertexVector = meshHandle.verts[result[3]];
        float currentPercentage = PercentageComplete();

        if (lastReward == null)
        {
            float clampReward = Mathf.Clamp(rewardValue, -4f, 2f);
            gReward += clampReward;
            lastReward = rewardValue;
        }
        else if (lastReward != rewardValue)
        {
            float temp = (float)(rewardValue - lastReward);

            float clampReward = Mathf.Clamp(temp, -4f, 2f);
            gReward += clampReward;
            lastReward = rewardValue;

        }

        // Hit again logic
        // no damage on healthy tissue and didn't hit the closest vertex
        else if (laserOn && lastReward == rewardValue)
        {
            if (currentPercentage == lastPercentage)
            {
                hitAgainCounter++;
                if (hitAgainCounter % 10 == 0)
                {
                    gReward += -0.007f;
                    hitAgainCounter = 0;
                }
            }
            else
            {
                hitAgainCounter = 0;
            }
        }

        // more of the pattern was ablated
        if (laserOn && currentPercentage != lastPercentage && withinRadius == 1f)
        {
            float delta = currentPercentage - lastPercentage;
            if (delta > 0f)
            {
                gReward += delta * 11f;
                lastPercentage = currentPercentage;
            }

        }
        
        if (done == 1)
        {
            doneTrace = true;
        }
        else
        {
            doneTrace = false;
        }


        vecDirection = vertexVector - meshHandle.playerPos;
        vecDirection.y = 0f;
        
        if (vecDirection.magnitude < 2.5f && withinRadius == 0f)
        {
            withinRadius = 1f;
        }
        if (!calculateCloseVertex)
            calculateCloseVertex = true;
    }


    void OnDestroy()
    {
        if (fullPatternTexture != null)
            Destroy(fullPatternTexture);
        if (ovalShaderPattern != null)
            ovalShaderPattern.Release();
    }

}
