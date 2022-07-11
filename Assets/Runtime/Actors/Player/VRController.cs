using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SupremacyHangar.Runtime.Actors.Player
{
    public class VRController : MonoBehaviour, IPlayerController
    {
        public event Action OnInteractionTriggered;
        public float3 PlatformVelocity { get; set; }

        [SerializeField] private Color normalColor;
        [SerializeField] private Color interactibleColor;
        
        [SerializeField] private Renderer[] controllerRenderers;
        [SerializeField] private InputActionProperty[] activationActions;

        private Color currentColor;
        private readonly List<Material> controllerMaterials = new();
        
        private int interactableCount = 0;
        private static readonly int shaderColor = Shader.PropertyToID("_BaseColor");

        public void Awake()
        {
            currentColor = normalColor;
            foreach (var renderer in controllerRenderers)
            {
                foreach (var material in renderer.materials)
                {
                    controllerMaterials.Add(material);
                    material.SetColor(shaderColor, currentColor);
                }
            }

            foreach (var actionproperty in activationActions)
            {
                actionproperty.action.performed += OnInteractionChanged;
                actionproperty.action.canceled += OnInteractionChanged;
            }
        }

        private void OnInteractionChanged(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled) return;
            if (OnInteractionTriggered == null) return;
			
            OnInteractionTriggered.Invoke();
            interactableCount = 0;
        }

        public void Update()
        {
            if (!(math.lengthsq(PlatformVelocity) > 0)) return;

            transform.Translate(new Vector3(PlatformVelocity.x, PlatformVelocity.y, PlatformVelocity.z));
            PlatformVelocity = float3.zero;
        }
        
        public void IncrementInteractionPromptRequests()
        {
            interactableCount++;
            foreach (var material in controllerMaterials)
            {
                material.SetColor(shaderColor, interactibleColor);
            }
        }

        public void DecrementInteractionPromptRequests()
        {
            if (interactableCount > 0)
            {
                interactableCount--;
            }
            else
            {
                interactableCount = 0;
                foreach (var material in controllerMaterials)
                {
                    material.SetColor(shaderColor, normalColor);
                }
            }
        }
    }
}