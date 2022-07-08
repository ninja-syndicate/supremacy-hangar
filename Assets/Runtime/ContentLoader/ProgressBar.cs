using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;


namespace SupremacyHangar.Runtime.ContentLoader
{
    public class LoadingProgressContext
    {
        public ProgressSignalHandler ProgressSignalHandler => progressSignalHandler;
        public static ProgressSignalHandler progressSignalHandler;

        [Inject]
        public void Constuct(ProgressSignalHandler signalHandler)
        {
            progressSignalHandler = signalHandler;
        }

        public IEnumerator LoadingAssetProgress(AsyncOperationHandle handle, string message = "")
        {
            do
            {
                progressSignalHandler.ProgressBar(handle, message);
                yield return null;
            } while (!handle.IsDone);
            progressSignalHandler.ProgressBar(handle, message);
        }
    }

    public class ProgressBar : MonoBehaviour
    {
        private Slider mySlider;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField, Tooltip("Optional fixed name for loading indication")]
        private string newMessage = "Loading...";
        [SerializeField] private bool loadingScreen = false;

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
            if(!IsTargeted && !loadingScreen)
            {
                text.text = originalMessage;
                return;
            }
            text.text = signal.Description != "" ? signal.Description : newMessage;
            mySlider.value = signal.PercentageComplete;
        }
    }
}
