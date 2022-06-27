using System;
using UnityEngine;

namespace SupremacyHangar.Runtime
{
    public class AnimatorBoolController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string property;
        [SerializeField] private bool defaultValue;

        private int propertyHash;
        private bool currentValue;
        
        public void Awake()
        {
            SetupAnimator();
            ValidateProperty();

            if (!enabled) return;
            
            propertyHash = Animator.StringToHash(property);
        }
        
        public void Start()
        {
            animator.SetBool(propertyHash, defaultValue);
            currentValue = defaultValue;
        }

        public void SetTrue()
        {
            if (!enabled)
            {
                Debug.LogError("Invalid Setup, cannot control animator", this);
            }
            if (currentValue) return;
            currentValue = true;
            animator.SetBool(propertyHash, currentValue);
        }

        public void SetFalse()
        {
            if (!enabled)
            {
                Debug.LogError("Invalid Setup, cannot control animator", this);
            }
            if (!currentValue) return;
            currentValue = false;
            animator.SetBool(propertyHash, currentValue);
        }

        public void Toggle()
        {
            if (!enabled)
            {
                Debug.LogError("Invalid Setup, cannot control animator", this);
            }
            currentValue = !currentValue;
            animator.SetBool(propertyHash, currentValue);
        }

        public void Set(bool newValue)
        {
            if (!enabled)
            {
                Debug.LogError("Invalid Setup, cannot control animator", this);
            }
            if (currentValue == newValue) return;
            currentValue = newValue;
            animator.SetBool(propertyHash, currentValue);
        }
        
        private void SetupAnimator()
        {
            if (animator != null) return;
            if (TryGetComponent(out animator)) return;

            Debug.LogError("No Animator on this GameObject and none set!", this);
            enabled = false;
        }
        
        private void ValidateProperty()
        {
            if (!string.IsNullOrWhiteSpace(property)) return;

            Debug.LogError("Animation property cannot be blank or whitespace", this);
            enabled = false;
        }
    }
}