using NaughtyAttributes;
using Pixelplacement;
using System;
using UnityEngine;

namespace PolyhedraAngles
{
    public abstract class VisibilityChanger : MonoBehaviour
    {
        [OnValueChanged("UpdateVisibility")]
        [Range(0, 1f)]
        [SerializeField]
        protected float visibility = 0.5f;

        public float visibilityChangeDuration = 0.5f;

        protected virtual void Awake()
        {
            UpdateVisibility();
        }

        public virtual bool IsVisible()
        {
            return visibility > Mathf.Epsilon;
        }

        public virtual void SetVisibility(float visibility)
        {
            this.visibility = visibility;
            UpdateVisibility();
        }

        protected abstract void UpdateVisibility();

        [Button(null, EButtonEnableMode.Playmode)]
        public virtual void Show(Action completeCallback = null)
        {
            Tween.Value(visibility, 1, SetVisibility, Mathf.Lerp(visibilityChangeDuration, Mathf.Epsilon, visibility), 0, Tween.EaseLinear, Tween.LoopType.None, null, completeCallback);
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public virtual void Hide(Action completeCallback = null)
        {
            Tween.Value(visibility, 0, SetVisibility, Mathf.Lerp(Mathf.Epsilon, visibilityChangeDuration, visibility), 0, Tween.EaseLinear, Tween.LoopType.None, null, completeCallback);
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public void ToggleVisibility(Action completeCallback = null)
        {
            if (IsVisible())
                Hide(completeCallback);
            else
                Show(completeCallback);
        }
    }
}
