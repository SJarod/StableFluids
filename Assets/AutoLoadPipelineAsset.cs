using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// https://www.youtube.com/watch?v=3CpEn_mmr3o

[ExecuteAlways]
public class AutoLoadPipelineAsset : MonoBehaviour
{
    public UniversalRenderPipelineAsset pipelineAsset;

    private void OnEnable()
    {
        if (pipelineAsset)
        {
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
        }
    }
}
