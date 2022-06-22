using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
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

    [SerializeField] public float jsonTestFragmentDelay = 5f;
#endif
    
    public override void InstallBindings()
    {
#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(jsonTestFragment))
        {
            Task.Run(async () =>
            {
                await Task.Delay((int)(jsonTestFragmentDelay * 1000));
                Debug.Log("Set inventory");
                GetPlayerInventoryFromPage(jsonTestFragment);
            });
        }
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
