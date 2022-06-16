using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Zenject;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.Plugins.WebGL;

/// <summary>
/// Bridge used to communicate with a page
/// </summary>
public class BridgeScript : MonoInstaller
{
    //TODO: this probably needs to be done better later.
    private SupremacyGameObject inventoryData = new();

#if UNITY_EDITOR
    [TextArea(3, 50)]
    [SerializeField] public string jsonTestFragment;
#endif
    
    public override void InstallBindings()
    {
#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(jsonTestFragment)) GetPlayerInventoryFromPage(jsonTestFragment);
#elif UNITY_WEBGL
        SiloReady();
#endif
        //Might have to bind again after data is read
        Container.Bind<SupremacyGameObject>().FromInstance(inventoryData).AsSingle();
    }

    public void GetPlayerInventoryFromPage(string message)
    {
        var newInventoryData = JsonConvert.DeserializeObject<SupremacyGameObject>(message, new SiloItemConterter());
        inventoryData.CopyFrom(newInventoryData);
    }

    public void SiloReady()
    {
        WebGLPluginJS.SiloReady();
    }
}
