using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Light bloom post effect (built-in pipeline, OnRenderImage). Quarter-resolution bright pass, one blur,
    /// additive combine. Not the Post Processing package: it would add code and shaders to the WebGL build.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class BloomEffect : MonoBehaviour
    {
        const int PassPrefilter = 0;
        const int PassBlur = 1;
        const int PassCombine = 2;

        [SerializeField] Shader shader;
        [SerializeField, Range(0f, 1f)] float threshold = 0.92f;
        [SerializeField, Range(0f, 2f)] float intensity = 0.25f;

        static readonly int ThresholdId = Shader.PropertyToID("_Threshold");
        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int BloomTexId = Shader.PropertyToID("_BloomTex");

        Material _material;

        void OnEnable()
        {
            if (shader != null && shader.isSupported) _material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        void OnDisable()
        {
            if (_material != null) Destroy(_material);
            _material = null;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
            _material.SetFloat(ThresholdId, threshold);
            _material.SetFloat(IntensityId, intensity);

            int width = Mathf.Max(1, source.width / 4), height = Mathf.Max(1, source.height / 4);
            var bright = RenderTexture.GetTemporary(width, height, 0, source.format);
            var blurred = RenderTexture.GetTemporary(width, height, 0, source.format);
            Graphics.Blit(source, bright, _material, PassPrefilter);
            Graphics.Blit(bright, blurred, _material, PassBlur);
            Graphics.Blit(blurred, bright, _material, PassBlur);
            _material.SetTexture(BloomTexId, bright);
            Graphics.Blit(source, destination, _material, PassCombine);
            RenderTexture.ReleaseTemporary(bright);
            RenderTexture.ReleaseTemporary(blurred);
        }
    }
}
