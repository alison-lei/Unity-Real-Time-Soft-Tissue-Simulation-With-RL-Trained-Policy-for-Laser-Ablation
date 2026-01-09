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
    // private int stepCounter; do this only if want to access reward every 10 steps
    private float onesLeft;
    private float totalPatternPixels;
    public RenderTexture ovalShaderPattern;
    public Material shaderGraph;
    public Texture2D fullPatternTexture;

    private float gReward;
    private float startTime;
    private Vector3 vecDirection;

    
    private Vector3 startPos;
    // private int campingThresholdSteps = 300;
    // private float perStepPenalty = -0.002f;
    // private float oneTimePenalty = -0.1f;
    // private float campingSteps = 0;
    // private bool oneTimePenaltyApplied = false;
    private float radiusSqr = 16f; //4**2
    private Vector3 vertexVector;
    // public List<int> patternIndices;
    private bool laserOn = false;
    private float scoreIncrement = 1f;
    // private float scoreDecay = 0.05f;
    private float campingPenalty = -0.1f;
    private float campingScore = 0f;
    private float campingThreshold = 250f;
    private int relocationCounter = 0;
    public float relocationDistance = 25f; // Distance to count as relocation
    public int relocationSteps = 60;
    // private float redoTimes = 0f;
    // private float vertDistance = 0f;
    private bool calculateCloseVertex;
    private int Width = 60;
    private int Height = 28;
    private int edgeMargin = 8;
    // private float percentageDone;
    private float lastPercentage;
    // private float fulltotalreward = 0f;
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
        // stepCounter = 0;
        // totalPatternPixels = 0.0f;
        // patternIndices = new List<int>();

        //////////////////////////////////////////////////////////////////////////////////////////
        
        // Generating a specific pattern uploaded from image onto the mesh
        // Texture2D temporaryTexture = ResizeTexture(patternImage, meshHandle.xDimensions, meshHandle.zDimensions);

        // Color[] pixels = temporaryTexture.GetPixels();
        // foreach (Color c in pixels)
        // {
        //     // if (c.r > 0.999f)
        //     if (c.r > 0.1f)
        //     {
        //         totalPatternPixels += 1.0f;
        //         // patternIndices.Add(count);
        //     }
        //     // count++;
        // }

        //////////////////////////////////////////////////////////////////////////////////////////

        // setting the render texture to the shader graph
        // Use RGFloat because want two channels, 32-bit float each, store two values per coordinate
        ovalShaderPattern = new RenderTexture(meshHandle.xDimensions, meshHandle.zDimensions, 0, RenderTextureFormat.RGFloat);
        ovalShaderPattern.enableRandomWrite = true;
        ovalShaderPattern.Create();
        shaderGraph.SetTexture("_ovalShaderPattern", ovalShaderPattern);




        totalPatternPixels = 0f;

        // returns Texture2D data type
        fullPatternTexture = GenerateOval();

        onesLeft = totalPatternPixels;
        // Debug.Log("total " + totalPatternPixels);
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
        // change the filter mode of the render texture so blends neighboring 4 pixel values, good for shrinking image
        rt.filterMode = FilterMode.Bilinear;

        Graphics.Blit(source, rt);

        RenderTexture.active = rt;
        Texture2D tempTex = new Texture2D(width, height, TextureFormat.RGBA32, false); // 8 bits per channel, 4 channels
        // paste at point 0,0 of tempTex, read from point 0,0 (bottom left) of render texture rt
        // read pixels from GPU to CPU
        tempTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tempTex.Apply();
        RenderTexture.active = null;

        // to count the total number of pattern pixels AND change the image from grayscale to purely black and white
        // Color[] pixels = tempTex.GetPixels();
        // for (int i = 0; i < pixels.Length; i++)
        // {
        //     float luminance = pixels[i].r;
        //     pixels[i] = (luminance > 0.1f) ? Color.white : Color.black;
        //     if (luminance > 0.1f)
        //     {
        //         totalPatternPixels += 1.0f;
        //     }
        // }
        // tempTex.SetPixels(pixels);
        // tempTex.Apply();

        return tempTex;
    }

    // function to generate random oval-shapped pattern to mimic appearance of endometriosis lesions
    public Texture2D GenerateOval()
    {
        Texture2D tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);

        // Fill background black
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

                // Rotate point around center
                float rotatedX = dx * Mathf.Cos(rad) - dy * Mathf.Sin(rad);
                float rotatedY = dx * Mathf.Sin(rad) + dy * Mathf.Cos(rad);

                // checking whether inside the ellipse equation
                if ((rotatedX * rotatedX) / (rx * rx) + (rotatedY * rotatedY) / (ry * ry) <= 1f)
                {
                    tex.SetPixel(x, y, Color.white);
                    totalPatternPixels += 1.0f;
                }
            }
        }
        // updates the mirrored copy of tex on GPU in order to then do Graphics.Blit()
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
        // reset agent and environment
        if (doneTrace)
            doneTrace = false;
        lastReward = null;
        onesLeft = totalPatternPixels;
        vecDirection = Vector3.zero;
        calculateCloseVertex = false;

        // Resets all render texture buffers to 0
        meshHandle.ResetRTBuffers();
        playerController.resetBoundsFlag();

        // scalpel.SetActive(true);
        // if (trainingMode)
        gReward = 0f;
        startTime = Time.time;


        // oneTimePenaltyApplied = false;
        // campingSteps = 0;
        startPos = meshHandle.playerPos;
        // redoTimes = 0f;
        // meshHandle.newIndex = 0f;

        campingScore = 0f;
        relocationCounter = 0;
        lastPercentage = 0f;
        // fulltotalreward = 0f;
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

        // Debug.DrawLine(meshHandle.playerPos, meshHandle.playerPos + vecDirection, Color.green);

        // sensor.AddObservation(scalpel.transform.localPosition - meshHandle.quad.transform.localPosition); // 3
        sensor.AddObservation(meshHandle.playerPos); // 3

        // CalculateHeight();

        lastPercentage = PercentageComplete();
        sensor.AddObservation(lastPercentage); // 1
                                               // Debug.Log("% done = " + PercentageComplete());

        sensor.AddObservation(endTimeCounter);
        sensor.AddObservation(withinRadius);

    }


    public override void OnActionReceived(ActionBuffers actions)
    {

        // 1
        // stepCounter++
        // tanh [-1,1]
        // horizontalInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f) * 0.5f;
        // yMoveVec = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f) * 0.001f;
        // verticalInput = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f) * 0.5f;
        horizontalInput = actions.ContinuousActions[0];
        // yMoveVec = actions.ContinuousActions[1];
        // verticalInput = actions.ContinuousActions[2];
        verticalInput = actions.ContinuousActions[1];

        if (horizontalInput != 0f || verticalInput != 0f)
            activeAgain = true;

        // playerController.MoveLaser(horizontalInput, yMoveVec, verticalInput);
        playerController.MoveLaser(horizontalInput, verticalInput);

        // float tempDist = meshHandle.Distance(meshHandle.playerPos, vertexVector);
        // if (tempDist < vertDistance)
        // {
        //     AddReward(0.01f);
        // }
        // vertDistance = tempDist;

        // 2
        // if (float.IsNaN(meshHandle.vertexUnderneath.y))
        // {
        //     AddReward(-1f);
        //     Debug.Log("the vertex directly underneath would freak out");
        //     EndEpisode();
        //     return;
        // }

        // bonus for moving in correct direction
        // if (calculateCloseVertex && vecDirection == Vector3.zero)
        // {
        //     gReward += 0.01f;
        //     // AddReward(0.01f);
        // }
        // else
        // {
        // Debug.Log(vecDirection + "dot" + new Vector3(horizontalInput, 0f, verticalInput));
        // Debug.Log(Vector3.Dot(vecDirection, new Vector3(horizontalInput, 0f, verticalInput)));

        Vector3 lastMovement = new Vector3(horizontalInput, 0f, verticalInput);
        if (vecDirection != Vector3.zero && lastMovement != Vector3.zero)
        {
            // reward for going in the direction of the nearest pettern vertex
            float dirBonus = Mathf.Clamp(0.001f * Vector3.Dot(vecDirection.normalized, lastMovement.normalized), 0f, 0.001f);
            if (dirBonus > 0.0001f)
            {

                // if (trainingMode)
                // {
                //     gReward += dirBonus;
                // }
                gReward += dirBonus;
                // AddReward(dirBonus);
                // Debug.Log(dirBonus + " went nearest vertex !!!!!!!!!!!!!!!!!!!!!!!");
            }
            // Debug.Log(dirBonus);
        }


        // float newIndices = meshHandle.newIndex;
        // if (newIndices != 0f)
        // {
        //     AddReward(newIndices);
        //     meshHandle.newIndex = 0f;
        //     // Debug.Log(newIndices);
        // }

        // actions.DiscretActions[0] is responsible for turning on the laser
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
            // AddReward(campingPenalty);
            // Debug.Log("surpassed camping threshold");

            // resets the starting position to see if staring from this new position it camps again
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
                // AddReward(0.005f);
                // Debug.Log("new start position");
            }
        }
        else
        {
            relocationCounter = 0; // Reset if back within distance, so need to be outside of relocationDistance for relocationSteps consecutive times
        }



        // Vector3 curPos = meshHandle.playerPos;
        // float distSqr = (curPos - lastPos).sqrMagnitude;

        // if (distSqr < radiusSqr)
        // {
        //     campingSteps++;
        //     // AddReward(perStepPenalty);
        //     // if (campingSteps >= campingThresholdSteps && !oneTimePenaltyApplied)
        //     if (campingSteps % campingThresholdSteps == 0)
        //     {
        //         AddReward(oneTimePenalty); // one-time larger penalty
        //         // oneTimePenaltyApplied = true;
        //         // Debug.Log("too long /////////////////////////");
        //     }

        // }
        // else
        // {
        //     campingSteps = 0;
        //     // oneTimePenaltyApplied = false;
        //     lastPos = curPos;
        //     // Debug.Log("no camp");
        // }
        // Debug.DrawLine(lastPos, curPos, Color.red);
        // Debug.Log(meshHandle.Distance(lastPos, curPos));



        // in mesh handle, always run fixed update where the tissue is animated and damaged
        // only calculate reward if traininig here
        // updates rewardbuffer and gets reward
        // decision period is 1
        if (Time.frameCount % 4 == 0 && meshHandle.allInitialized/* && laserOn*/)
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
            // Debug.Log(gReward);
            // fulltotalreward += gReward;
            // Debug.Log(fulltotalreward);
        }
        gReward = 0f;

        // if (stepCounter % 10 == 0)
        // {
        //     percentageComplete = 1.0f - (onesLeft / totalPatternPixels);
        // }
        
        if (doneTrace || PercentageComplete() > 0.9f)
        {
            playerController.homePosition();
            // Debug.Log("doneeeeeeeeeeeeeeeeee");
            // flag
            // gReward += 5.0f;
            AddReward(5.0f);
            // Time.timeScale = 0;
            // massSpringSystem.ReleaseBuffers(); CANNOT do this, only when gameplay ends as then start referencing nonexistant buffers by cpu and gpu
            ConsiderTime();
            EndEpisode();
        }


        if (playerController.outofBounds)
        {
            // gReward += -2.0f;
            // AddReward(-2.0f);
            // if (scalpel.activeSelf == true)
            //     scalpel.SetActive(false);
            playerController.homePosition();
            // massSpringSystem.ReleaseBuffers();
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
        // if go up and down
        // continuous[1] = Input.GetKey(KeyCode.Q) ? 1.0f : (Input.GetKey(KeyCode.E) ? -1.0f : 0.0f);
        // continuous[2] = Input.GetAxis("Vertical");
        continuous[1] = Input.GetAxis("Vertical");

        discrete[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        // Debug.Log(discrete[0]);
    }


    // Huge neg reward if overtime, only penalize
    public void ConsiderTime()
    {
        float timePunish = Mathf.Clamp(-Mathf.Pow(2f, 0.02f * (Time.time - startTime)) + 1f, -4f, 0f);
        gReward += timePunish;
    }


    // private void CalculateHeight()
    // {
    //     // return height of scalpel above closest vertex point
    //     scalpelHeightAboveTissue = scalpel.transform.position.y - meshHandle.vertexUnderneath.y;
    //     tipHeightAboveTissue = meshHandle.playerPos.y - meshHandle.vertexUnderneath.y;
    //     // Debug.DrawLine(new Vector3(0f, 1f, 0f), meshHandle.vertexUnderneath, Color.red);
    // }



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
        vertexVector = meshHandle.verts[result[3]]; // had index outside of bounds of array error
        float currentPercentage = PercentageComplete();

        if (lastReward == null)
        {
            float clampReward = Mathf.Clamp(rewardValue, -4f, 2f);
            gReward += clampReward;
            lastReward = rewardValue;
            // AddReward(clampReward);
            // if (trainingMode)
            // {
            //     gReward = clampReward;
            //     Debug.Log("gReward from normal damage " + gReward);
            // }
        }
        else if (lastReward != rewardValue)
        {
            float temp = (float)(rewardValue - lastReward);

            float clampReward = Mathf.Clamp(temp, -4f, 2f);
            gReward += clampReward;
            // fulltotalreward += temp;
            // Debug.Log(fulltotalreward);
            // Debug.Log("gReward from normal damage " + gReward);
            // AddReward(clampReward);
            lastReward = rewardValue;
            // if (trainingMode)
            // {
            //     gReward += clampReward;
            //     Debug.Log("gReward from normal damage " + gReward);
            // }

        }

        // Hit again logic
        // no damage on healthy tissue and didn't hit the closest vertex
        else if (laserOn && lastReward == rewardValue)
        {
            // no change in the % of pattern hit but laser was on
            if (currentPercentage == lastPercentage)
            {
                hitAgainCounter++;
                if (hitAgainCounter % 10 == 0)
                {
                    gReward += -0.007f;
                    // Debug.Log("Hit again here ?//////////////////////////////");
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
            // Debug.Log(currentPercentage + "  and " + percentageDone);
            if (delta > 0f)
            {
                gReward += delta * 11f;
                lastPercentage = currentPercentage;
                // fulltotalreward += delta * 10f;
                // Debug.Log("gReward here " + gReward);
                // Debug.Log("helooo ////////////////////////////////");
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
        // Debug.Log("pos off vector "+vertexVector);
        // Debug.Log("tool pos "+meshHandle.playerPos);
        // Debug.Log("this is vecDirection "+vecDirection);
        vecDirection.y = 0f;
        
        if (vecDirection.magnitude < 2.5f && withinRadius == 0f)
        {
            withinRadius = 1f;
            // Debug.Log("within radius is now true");
        }
        if (!calculateCloseVertex)
            calculateCloseVertex = true;
        // Debug.Log(fulltotalreward);
    }


    void OnDestroy()
    {
        if (fullPatternTexture != null)
            Destroy(fullPatternTexture);
        if (ovalShaderPattern != null)
            ovalShaderPattern.Release();
    }

}


    // public void HitAgain(float value)
    // {
    //     AddReward(value);
    //     if (trainingMode)
    //     {
    //         gReward += value;
    //         Debug.Log("from damage again new gReward" + gReward);
    //     }

    // }
    

    // public void DeterCamping(float value)
    // {
    //     AddReward(value);
    //     if (trainingMode)
    //     {
    //         gReward += value;
    //         Debug.Log("from damage again new gReward" + gReward);
    //     }
            
    // }

