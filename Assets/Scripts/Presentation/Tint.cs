using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>Per-renderer colour without new material instances: sprite vertex colour or a property block.</summary>
    public static class Tint
    {
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int FlashId = Shader.PropertyToID("_Flash");
        static MaterialPropertyBlock _block;

        public static void Set(Renderer renderer, Color color)
        {
            if (renderer is SpriteRenderer sprite)
            {
                sprite.color = color;
                return;
            }
            Block(renderer).SetColor(ColorId, color);
            renderer.SetPropertyBlock(_block);
        }

        /// <summary>KitLit hit flash: 0 = normal, 1 = white.</summary>
        public static void Flash(Renderer renderer, float amount)
        {
            Block(renderer).SetFloat(FlashId, amount);
            renderer.SetPropertyBlock(_block);
        }

        static MaterialPropertyBlock Block(Renderer renderer)
        {
            // Shared scratch block (main thread only); read back first so colour and flash do not clobber each other.
            _block ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(_block);
            return _block;
        }
    }
}
