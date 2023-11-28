using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


public class ErosionComponent : MonoBehaviour
{
    [SerializeField] ComputeShader erosionShader;

    [Header("Perlin Noise")] 
    [SerializeField] [Range(0.1f, 10f)] float perlinScale = 2;
    [SerializeField][Range(1, 16)] int perlinResolution = 4;
    [SerializeField] Vector2Int outputResolution = new Vector2Int(4096, 4096);
    [Tooltip("in km")][SerializeField] float size;
    [Tooltip("in km")][SerializeField] float height;
    
    [Header("Simulation")] 
    [SerializeField][Range(0.5f, 10f)] float slopePower = 1;

    [Tooltip("Number of droplets per pixel")][SerializeField] [Range(0.1f, 10.0f)] float dropletDensity = 1.0f;
    [SerializeField] [Range(10, 1000)] int maxDropletSteps = 100;
    [SerializeField] [Range(0.0001f, 0.01f)] float dropletWeight = 0.001f;

    [SerializeField] bool initialize = false;
    [SerializeField] bool erode = false;

    private RenderTexture generatedHeight;
    private RenderTexture generatedNormal;
    private RenderTexture generatedAlbedo;
    private RenderTexture generatedSlope;
    private RenderTexture generatedErosion;
    private MeshRenderer meshRenderer;

    private void OnValidate()
    {
        if (initialize)
        {
            Initialize();
            initialize = false;
        }
        if (erode)
        {
            Erode();
            erode = false;
        }
    }

    void Erode()
    {
        int traceKernel = erosionShader.FindKernel("CSTrace");
        erosionShader.GetKernelThreadGroupSizes(traceKernel, out uint x, out uint y, out uint z);
        erosionShader.Dispatch(traceKernel, (int)(outputResolution.x * dropletDensity / x), (int)(outputResolution.x * dropletDensity / y), 1);
    }

    void InitializeErosion()
    {
        if(generatedAlbedo)
            generatedAlbedo.Release();
        if (generatedHeight)
            generatedHeight.Release();
        if (generatedNormal)
            generatedNormal.Release();
        if (generatedSlope)
            generatedSlope.Release();
        if (generatedErosion)
            generatedErosion.Release();
        
        generatedAlbedo = new RenderTexture(outputResolution.x, outputResolution.y, 0);
        generatedHeight = new RenderTexture(outputResolution.x, outputResolution.y, 0, RenderTextureFormat.RFloat);
        generatedNormal = new RenderTexture(outputResolution.x, outputResolution.y, 0);
        generatedSlope = new RenderTexture(outputResolution.x, outputResolution.y, 0, RenderTextureFormat.RFloat);
        generatedErosion = new RenderTexture(outputResolution.x, outputResolution.y, 0, RenderTextureFormat.RFloat);
        generatedHeight.enableRandomWrite = true;
        generatedAlbedo.enableRandomWrite = true;
        generatedNormal.enableRandomWrite = true;
        generatedSlope.enableRandomWrite = true;
        generatedErosion.enableRandomWrite = true;
        
        generatedHeight.Create();
        generatedAlbedo.Create();
        generatedNormal.Create();
        generatedSlope.Create();
        
        erosionShader.SetTexture(0, "Albedo", generatedAlbedo);
        erosionShader.SetTexture(0, "Displacement", generatedHeight);
        erosionShader.SetTexture(0, "Normal", generatedNormal);
        erosionShader.SetTexture(0, "Slope", generatedSlope);
        erosionShader.SetTexture(0, "Erosion", generatedErosion);
        
        erosionShader.SetTexture(1, "Albedo", generatedAlbedo);
        erosionShader.SetTexture(1, "Displacement", generatedHeight);
        erosionShader.SetTexture(1, "Normal", generatedNormal);
        erosionShader.SetTexture(1, "Slope", generatedSlope);
        erosionShader.SetTexture(1, "Erosion", generatedErosion);
        
        erosionShader.SetTexture(2, "Displacement", generatedHeight);
        erosionShader.SetTexture(2, "Normal", generatedNormal);
        erosionShader.SetTexture(2, "Slope", generatedSlope);
        
        erosionShader.SetFloat("PerlinScale", perlinScale);
        erosionShader.SetInt("PerlinDepth", perlinResolution);
        
        erosionShader.SetVector("Resolution", new Vector2(outputResolution.x, outputResolution.y));
        erosionShader.SetFloat("Size", size);
        erosionShader.SetFloat("Height", this.height);
        erosionShader.SetFloat("SlopePower", slopePower);
        erosionShader.SetFloat("DropletDensity", dropletDensity);
        erosionShader.SetInt("MaxDropletSteps", maxDropletSteps);
        
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.SetFloat("_Displacement", height);
        meshRenderer.sharedMaterial.SetFloat("_Size", size);

    }

    void Initialize()
    {
        InitializeErosion();
        int initializeKernel = erosionShader.FindKernel("GenerateNoise");
        erosionShader.GetKernelThreadGroupSizes(initializeKernel, out uint ix, out uint iy, out uint iz);
        erosionShader.Dispatch(initializeKernel, outputResolution.x / (int)ix, outputResolution.y / (int)iy, 1);
        
        int normalsKernel = erosionShader.FindKernel("GenerateNormals");
        erosionShader.GetKernelThreadGroupSizes(initializeKernel, out uint x, out uint y, out uint z);
        erosionShader.Dispatch(normalsKernel, outputResolution.x / (int)x, outputResolution.y / (int)y, 1);
        
        
        meshRenderer.sharedMaterial.SetTexture("_Albedo", generatedAlbedo);
        meshRenderer.sharedMaterial.SetTexture("_Normal", generatedNormal);
        meshRenderer.sharedMaterial.SetTexture("_DisplacementMap", generatedHeight);
        meshRenderer.sharedMaterial.SetTexture("_Slope", generatedSlope);
        meshRenderer.sharedMaterial.SetTexture("_Erosion", generatedErosion);
    }
}
