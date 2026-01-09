using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class DynamicDamageManager : MonoBehaviour
{
    // annotation, not statement
    [Header("Damage Texture")]
    public Material tissueMaterial;

    public Texture2D damageSprite;

    [Header("Render Texture Settings")]
    public int maskResolutionX = 549;
    public int maskResolutionY = 256;

    // intensity is determined as opacity
    [Range(0.0f, 1.0f)]
    public float damageIntensity = 0.2f;

    private Material blitMaterial;

    private RenderTexture damageMaskTexture;
    private RenderTexture tempA;
    private RenderTexture tempB;
    private RenderTexture source;
    private RenderTexture destination;
    private bool sourceTempA;


    void Awake()
    {
        sourceTempA = true;
        tempA = new RenderTexture(maskResolutionX, maskResolutionY, 0, RenderTextureFormat.R8);
        tempB = new RenderTexture(maskResolutionX, maskResolutionY, 0, RenderTextureFormat.R8);

        tempA.enableRandomWrite = true;
        tempB.enableRandomWrite = true;

        tempA.Create();
        tempB.Create();

        RenderTexture.active = tempA;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = tempB;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        blitMaterial = new Material(Shader.Find("Hidden/DamageBlitShader"));

    }

    public void ApplyDamage(Vector2 uvHitPoint, float radius)
    {
        blitMaterial.SetTexture("_DamageDecal", damageSprite);
        blitMaterial.SetVector("_HitUV", uvHitPoint);
        blitMaterial.SetFloat("_Radius", radius);
        blitMaterial.SetFloat("_Intensity", damageIntensity);

        source = sourceTempA ? tempA : tempB;
        destination = sourceTempA ? tempB : tempA;

        Graphics.Blit(source, destination, blitMaterial);

        damageMaskTexture = sourceTempA ? tempB : tempA;

        if (tissueMaterial != null)
        {
            tissueMaterial.SetTexture("_DamageMask", damageMaskTexture);
        }

        sourceTempA = !sourceTempA;

    }

    // called once per event, many times per frame
    // Graphical User Interface
    // create and renders GUI elements during runtime
    void OnGUI()
    {
        if (damageMaskTexture != null)
        {
            // use immediate mode GUI to draw texture onto the screen
            // uses GUI class to call method inside it
            // draws texture in top left corner of screen and the texture it draws is the damageMaskTexture RenderTexture
            // it is red because the damageMaskTexture formate is RenderFormat.R8, uses only the red channel, for grayscale purposes,
            // defaults to using those values for red chanels and leaves green and blue valeus as 0
            GUI.DrawTexture(new Rect(10, 10, 128, 128), damageMaskTexture);
        }
    }

    void OnDestroy()
    {
        if (tempA != null && tempB != null && source != null && destination != null && damageMaskTexture != null)
        {
            tempA.Release(); // deallocates GPU memory
            tempB.Release();
        }

        if (blitMaterial != null)
        {
            Destroy(blitMaterial);
        }
    }



}
