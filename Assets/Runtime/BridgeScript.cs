using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Zenject;
using SupremacyHangar.Runtime.Types;
using SupremacyHangar.Runtime.ContentLoader;

/// <summary>
/// Bridge used to communicate with a page
/// </summary>
public class BridgeScript : MonoInstaller
{
    //TODO: this probably needs to be done better later.
    private SupremacyGameObject inventoryData = new();

    [Inject]
    private ContentSignalHandler _contentSignalHandler;

#if UNITY_EDITOR
    [TextArea(3, 50)]
    [SerializeField] public string jsonTestFragment;

    [SerializeField] public float jsonTestFragmentDelay = 0.2f;
#endif
    
    public override void InstallBindings()
    {
        //Might have to bind again after data is read
        Container.Bind<SupremacyGameObject>().FromInstance(inventoryData).AsSingle();
    }

    public void GetPlayerInventoryFromPage(string message)
    {
        var newInventoryData = JsonConvert.DeserializeObject<SupremacyGameObject>(message, new SiloItemConterter());
        inventoryData.CopyFrom(newInventoryData);
        _contentSignalHandler.InventoryRecieved();
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
}
