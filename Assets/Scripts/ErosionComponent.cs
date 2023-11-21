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

    [FormerlySerializedAs("powerBySlope")]
    [Header("Simulation")] 
    [SerializeField] [Range(0.5f, 10f)] float slopePower = 1;

    [SerializeField] bool runErosion = false;

    private RenderTexture generatedHeight;
    private RenderTexture generatedNormal;
    private RenderTexture generatedAlbedo;
    private MeshRenderer meshRenderer;

    private void OnValidate()
    {
        if (runErosion)
        {
            double currentTime = Time.timeAsDouble;
            Erode();
            Debug.Log(1000 * (Time.timeAsDouble - currentTime) + "ms");
            runErosion = false;
        }
    }

    void Erode()
    {
        InitializeErosion();
        int kernel = erosionShader.FindKernel("CSMain");
        uint x, y, z;
        erosionShader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);

        erosionShader.Dispatch(kernel, outputResolution.x / (int)x, outputResolution.y / (int)y, 1);

        meshRenderer.sharedMaterial.SetTexture("_Albedo", generatedAlbedo);
        meshRenderer.sharedMaterial.SetTexture("_Normal", generatedNormal);
        meshRenderer.sharedMaterial.SetTexture("_DisplacementMap", generatedHeight);
        
    }

    void InitializeErosion()
    {
        generatedAlbedo.Release();
        generatedHeight.Release();
        generatedNormal.Release();
        
        generatedAlbedo = new RenderTexture(outputResolution.x, outputResolution.y, 0);
        generatedHeight = new RenderTexture(outputResolution.x, outputResolution.y, 0, RenderTextureFormat.RFloat);
        generatedNormal = new RenderTexture(outputResolution.x, outputResolution.y, 0);
        generatedHeight.enableRandomWrite = true;
        generatedAlbedo.enableRandomWrite = true;
        generatedNormal.enableRandomWrite = true;
        //generatedNormal.
        
        generatedHeight.Create();
        generatedAlbedo.Create();
        generatedNormal.Create();
        
        erosionShader.SetTexture(0, "Albedo", generatedAlbedo);
        erosionShader.SetTexture(0, "Displacement", generatedHeight);
        erosionShader.SetTexture(0, "Normal", generatedNormal);
        
        erosionShader.SetFloat("PerlinScale", perlinScale);
        erosionShader.SetInt("PerlinDepth", perlinResolution);
        
        erosionShader.SetVector("Resolution", new Vector2(outputResolution.x, outputResolution.y));
        erosionShader.SetFloat("Size", size);
        erosionShader.SetFloat("Height", this.height);
        erosionShader.SetFloat("SlopePower", slopePower);
        
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.SetFloat("_Displacement", height);
        meshRenderer.sharedMaterial.SetFloat("_Size", size);

    }
}
