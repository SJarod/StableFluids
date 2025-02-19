using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

// https://www.youtube.com/watch?v=3CpEn_mmr3o
public class FluidFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle source;

        private RTHandle tempTexture;

        public CustomRenderPass(Material material) : base()
        {
            this.material = material;
            tempTexture = RTHandles.Alloc("_TempFluidFeature", name: "_TempFluidFeature");

            //Blitter.Initialize(material.shader, material.shader);
        }

        public void SetSource(RTHandle source)
        {
            this.source = source;
        }

        private class PassData
        {
            public TextureHandle cameraSourceTexture;
            public Material material;
        }
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            // https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@13.1/api/UnityEngine.Rendering.Blitter.html
            Blitter.BlitTexture(context.cmd, data.cameraSourceTexture, new Vector4(1, 1, 0, 0), data.material, 1);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph-write-render-pass.html

            const string passName = "Render Custom Pass";

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                passData.cameraSourceTexture = resourceData.activeColorTexture;
                passData.material = material;

                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;

                builder.UseTexture(passData.cameraSourceTexture);
                TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "Camera Dest", false);

                builder.SetRenderAttachment(destination, 0);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material material;
    }

    [SerializeField]
    private Settings settings = new Settings();

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(settings.material);

        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
