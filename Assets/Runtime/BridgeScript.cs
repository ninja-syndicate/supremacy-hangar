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
    private SupremacyGameObject readData = new();
    
    public override void InstallBindings()
    {
        JSONtesting();
        //Might have to bind again after data is read
        Container.Bind<SupremacyGameObject>().FromInstance(readData).AsSingle();
    }

    public void JSONtesting()
    {
        string obj = "{faction: 'Zaibatsu'," +
            "silos:[" +
            "{type: 'mech',id: 'mech/assetID',chassisId: 'Zaibatsu', skinID: 'CherryBlossom' }, " +
            "{type: 'lootBox', id: 'Rare', expires: '2022-06-09T09:32:38+00:00'}," +
            "{type: 'mech',id: 'mech/assetID',chassisId: 'RedMountain', skinID: 'Evo' }, " +
            "{type: 'mech',id: 'mech/assetID',chassisId: 'BostonCybernetics', skinID: 'Cyber' }, " +
            "{type: 'lootBox', id: 'lootbot/assetID', expires: '2022-06-010T09:32:38+00:00'}" +
            "]}";

        GetPlayerInventoryFromPage(obj);
    }

    public void GetPlayerInventoryFromPage(string message)
    {
        JsonSerializer serializer = new JsonSerializer();
        readData = serializer.Deserialize(new JsonTextReader(new StringReader(message)), typeof(SupremacyGameObject)) as SupremacyGameObject;
    }

    public void SiloReady()
    {
        WebGLPluginJS.SiloReady();
    }
}
