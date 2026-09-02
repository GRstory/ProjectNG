using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace GRstory.Rendering
{
    // 포스트 프로세싱까지 끝난 화면을 Dual Kawase로 흐려서 _UIBlurTexture 전역 텍스처에 올린다.
    // UI 셰이더(GRstory/UI/Blur)는 이 텍스처를 화면 좌표로 샘플만 하므로 블러 비용은 프레임당 한 번이다.
    public class UIBlurFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader _blurShader;
        [SerializeField, Range(1, 6), Tooltip("다운/업샘플 단계 수. 한 단계마다 해상도가 절반이 되어 블러가 넓어진다")]
        private int _iterations = 3;
        [SerializeField, Range(0.5f, 3f), Tooltip("샘플 간격(텍셀). 키우면 더 흐려지지만 너무 크면 격자가 보인다")]
        private float _offset = 1f;

        private Material _material;
        private UIBlurPass _pass;

        public override void Create()
        {
            if (_blurShader == null)
            {
                Debug.LogError("UIBlurFeature: 블러 셰이더가 비어 있음", this);
                return;
            }

            _material = CoreUtils.CreateEngineMaterial(_blurShader);
            _pass = new UIBlurPass(_material)
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing,
                requiresIntermediateTexture = true, // 백버퍼 직행을 막아 화면을 텍스처로 읽을 수 있게 한다
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null) return;

            // 씬 뷰/프리뷰는 제외하고, 카메라 스택의 마지막 카메라에서 한 번만 찍는다
            if (renderingData.cameraData.cameraType != CameraType.Game) return;
            if (!renderingData.cameraData.resolveFinalTarget) return;

            _pass.Setup(_iterations, _offset);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            _pass?.Dispose();
            CoreUtils.Destroy(_material);
        }

        private class UIBlurPass : ScriptableRenderPass
        {
            private static readonly int BlurTextureId = Shader.PropertyToID("_UIBlurTexture");
            private static readonly int OffsetId = Shader.PropertyToID("_Offset");
            private const int DownPass = 0;
            private const int UpPass = 1;
            private const int MaxLevels = 7;

            private readonly Material _material;
            private readonly TextureHandle[] _levels = new TextureHandle[MaxLevels];
            private RTHandle _blurHandle; // 최종 결과. 오버레이 캔버스가 그려질 때까지 살아 있어야 해서 그래프 밖에서 관리한다
            private int _iterations;

            private class PassData
            {
                public TextureHandle Source;
                public Material Material;
                public int Pass;
            }

            public UIBlurPass(Material material)
            {
                _material = material;
                profilingSampler = new ProfilingSampler("UI Blur");
            }

            public void Setup(int iterations, float offset)
            {
                _iterations = Mathf.Clamp(iterations, 1, MaxLevels - 1);
                _material.SetFloat(OffsetId, offset);
            }

            public void Dispose()
            {
                _blurHandle?.Release();
                _blurHandle = null;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                if (resourceData.isActiveTargetBackBuffer) return; // 백버퍼는 텍스처로 읽을 수 없다

                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.bindMS = false;
                desc.useMipMap = false;
                desc.width = Mathf.Max(1, desc.width >> 1);
                desc.height = Mathf.Max(1, desc.height >> 1);

                // 레벨 1(절반 해상도)은 영구 텍스처, 그 아래 레벨은 그래프가 관리하는 임시 텍스처
                if (RenderingUtils.ReAllocateHandleIfNeeded(ref _blurHandle, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_UIBlurTexture"))
                {
                    Shader.SetGlobalTexture(BlurTextureId, _blurHandle.rt);
                }
                _levels[1] = renderGraph.ImportTexture(_blurHandle);

                for (int i = 2; i <= _iterations; i++)
                {
                    desc.width = Mathf.Max(1, desc.width >> 1);
                    desc.height = Mathf.Max(1, desc.height >> 1);
                    _levels[i] = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_UIBlurLevel", false, FilterMode.Bilinear);
                }

                // 다운: 카메라 → 1 → 2 → … → N
                TextureHandle current = resourceData.activeColorTexture;
                for (int i = 1; i <= _iterations; i++)
                {
                    AddBlit(renderGraph, current, _levels[i], DownPass, "UI Blur Down");
                    current = _levels[i];
                }

                // 업: N → … → 2 → 1
                for (int i = _iterations - 1; i >= 1; i--)
                {
                    AddBlit(renderGraph, current, _levels[i], UpPass, "UI Blur Up");
                    current = _levels[i];
                }
            }

            private void AddBlit(RenderGraph renderGraph, TextureHandle source, TextureHandle destination, int pass, string name)
            {
                using IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(name, out PassData data, profilingSampler);
                data.Source = source;
                data.Material = _material;
                data.Pass = pass;

                builder.UseTexture(source);
                builder.SetRenderAttachment(destination, 0);
                builder.AllowPassCulling(false); // 결과를 그래프 밖(UI 캔버스)에서 읽으므로 컬링되면 안 된다
                builder.SetRenderFunc(static (PassData d, RasterGraphContext context) =>
                    Blitter.BlitTexture(context.cmd, d.Source, new Vector4(1f, 1f, 0f, 0f), d.Material, d.Pass));
            }
        }
    }
}
