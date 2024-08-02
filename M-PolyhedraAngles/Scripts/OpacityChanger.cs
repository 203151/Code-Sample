using UnityEngine;

namespace PolyhedraAngles
{
    public class OpacityChanger : VisibilityChanger
    {
        Material material;
        Color visibleColor;
        Color hiddenColor;

        protected override void Awake()
        {
            if (TryGetComponent(out Renderer renderer))
            {
                material = renderer.material;

                visibleColor = material.color;
                hiddenColor = visibleColor;
                hiddenColor.a = 0;

                base.Awake();
            }
            else
            {
                Debug.LogError($"[OpacityChanger] Renderer not found for {nameof(OpacityChanger)} on object {name}. Removing script.");
                DestroyImmediate(this);
            }
        }

        protected override void UpdateVisibility()
        {
            material.color = Color.Lerp(hiddenColor, visibleColor, visibility);
        }
    }
}
