using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Zenject;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Plugins.WebGL;

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

    [TextArea(3, 50)]
    [SerializeField] public string jsonTestFragment;

    [SerializeField] public float jsonTestFragmentDelay = 0.2f;

    [TextArea(3, 50)]
    [SerializeField] public string jsonCrateText;

    private SignalBus _bus;
    bool _subscribed = false;

    public override void InstallBindings()
    {
        //Might have to bind again after data is read
        Container.Bind<HangarData>().FromInstance(hangarData).AsSingle();
        Container.Bind<BridgeScript>().FromInstance(this).AsSingle();
    }

    public void GetPlayerInventoryFromPage(string message)
    {
        var newInventoryData = JsonConvert.DeserializeObject<HangarData>(message, new SiloItemConterter());
        hangarData.CopyFrom(newInventoryData);
        contentSignalHandler.InventoryRecieved();
    }

    public void GetCrateContentsFromPage(string message)
    {
        var crateContent = JsonConvert.DeserializeObject<SiloItem>(message, new SiloItemConterter());
        _crateSignalHandler.FillCrate(crateContent);
    }

    public void RequestCrateContent(string ownership_id)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        SetCrateContent(ownership_id);
#else
        WebGLPluginJS.RequestCrateContent(ownership_id);
#endif
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    private void SetCrateContent(string ownership_id)
    {
        Debug.Log(ownership_id);
        GetCrateContentsFromPage(jsonCrateText);
    }

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
}
