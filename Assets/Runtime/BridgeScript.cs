using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Zenject;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Silo;

/// <summary>
/// Bridge used to communicate with a page
/// </summary>
public class BridgeScript : MonoInstaller
{
    //TODO: this probably needs to be done better later.
    private HangarData hangarData = new();

    [Inject]
    private ContentSignalHandler contentSignalHandler;

    [Inject]
    private CrateSignalHandler _crateSignalHandler;

#if UNITY_EDITOR
    [TextArea(3, 50)]
    [SerializeField] public string jsonTestFragment;

    [SerializeField] public float jsonTestFragmentDelay = 0.2f;
#endif
    
    #if UNITY_EDITOR
    [TextArea(3, 50)]
    [SerializeField] public string jsonCrateText;
#endif
    
    public override void InstallBindings()
    {
        //Might have to bind again after data is read
        Container.Bind<HangarData>().FromInstance(hangarData).AsSingle();
    }

    public void GetPlayerInventoryFromPage(string message)
    {
        var newInventoryData = JsonConvert.DeserializeObject<HangarData>(message, new SiloItemConterter());
        hangarData.CopyFrom(newInventoryData);
        contentSignalHandler.InventoryRecieved();
    }

    private void CrateContent()
    {
        GetCrateContentsFromPage(jsonCrateText);
    }

    public void GetCrateContentsFromPage(string message)
    {
        var crateContent = JsonConvert.DeserializeObject<SiloItem>(message, new SiloItemConterter());
        _crateSignalHandler.FillCrate(crateContent);
    }

#if UNITY_EDITOR
    public void SetPlayerInventoryFromFragment()
    {
        if (!string.IsNullOrWhiteSpace(jsonTestFragment))
        {
            Task.Run(async () =>
            {
                await Task.Delay((int)(jsonTestFragmentDelay * 1000));
                GetPlayerInventoryFromPage(jsonTestFragment);
            });
        }
    }
#endif

    private SignalBus _bus;
    bool _subscribed = false;
    [Inject]
    public void Contruct(SignalBus bus)
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
        _bus.Unsubscribe<CrateContentSignal>(CrateContent);
        _subscribed = false;
    }

    private void SubscribeToSignal()
    {
        if (_bus == null || _subscribed) return;
        _bus.Subscribe<CrateContentSignal>(CrateContent);
        _subscribed = true;
    }
}
