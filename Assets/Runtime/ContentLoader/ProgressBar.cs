using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace SupremacyHangar.Runtime.ContentLoader
{
    public class ProgressBar : MonoBehaviour
    {
        private Slider mySlider;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private string newMessage = "Loading...";

        private string originalMessage;
        private SignalBus _bus;
        private bool _subscribed;

        public bool IsTargeted { get; set; } = false;

        public void Start()
        {
            originalMessage = text.text;
            if (TryGetComponent(out mySlider)) return;
            Debug.LogError("Cannot find slider");
            enabled = false;
        }

        [Inject]
        public void Construct(SignalBus bus)
        {
            _bus = bus;
            SubscribeToSignal();
        }

        private void OnEnable()
        {
            SubscribeToSignal();
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            _bus.Unsubscribe<AssetLoadingProgressSignal>(SetSliderValue);

            _subscribed = false;
        }

        private void SubscribeToSignal()
        {
            if (_bus == null || _subscribed) return;
            _bus.Subscribe<AssetLoadingProgressSignal>(SetSliderValue);

            _subscribed = true;
        }

        private void SetSliderValue(AssetLoadingProgressSignal signal)
        {
            if(!mySlider || !IsTargeted)
            {
                text.text = originalMessage;
                return;
            }
            text.text = newMessage;
            mySlider.value = signal.PercentageComplete;
        }
    }
}
