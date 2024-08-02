using NaughtyAttributes;
using Pixelplacement;
using System.Linq;
using UnityEngine;

namespace PolyhedraAngles
{
    public class ExamplePolyhedronObjectModel : MonoBehaviour
    {
        private float opacity = 1f;

        public float transparentModeOpacityLevel = 0.1f;

        Material[] materials;

        private void Awake()
        {
            materials = GetComponentsInChildren<MeshRenderer>().Select(mr => mr.material).Distinct().ToArray();
        }

        public void SetOpacity(float opacity)
        {
            this.opacity = opacity;
            foreach (var m in materials)
            {
                Color c = m.color;
                c.a = opacity;
                m.color = c;
            }
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public void BecomeTransparent()
        {
            Tween.Value(opacity, transparentModeOpacityLevel, v => SetOpacity(v), 0.5f, 0, Tween.EaseInOut);
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public void ResetToNormal()
        {
            Tween.Value(opacity, 1f, v => SetOpacity(v), 0.5f, 0, Tween.EaseInOut);
        }
    }
}