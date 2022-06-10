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
        //SiloReady();
        JSONtesting();
        //Might have to bind again after data is read
        Container.Bind<SupremacyGameObject>().FromInstance(readData).AsSingle();
    }

    public void JSONtesting()
    {
        string obj = "{faction: 'Zaibatsu'," +
            "silos:[" +
            "{type: 'mech', ownership_id: 'mech/assetID', mech_id: 'Zaibatsu', skin_id: 'CherryBlossom' }, " +
            "{type: 'mystery_crate', ownership_id: 'Rare', can_open_on: '2022-06-09T09:32:38+00:00'}," +
            "{type: 'mech', ownership_id: 'mech/assetID', mech_id: 'RedMountain', skin_id: 'Evo' }, " +
            "{type: 'mech', ownership_id: 'mech/assetID', mech_id: 'BostonCybernetics', skin_id: 'Cyber' }, " +
            "{type: 'mystery_crate', ownership_id: 'lootbot/assetID', can_open_on: '2022-06-010T09:32:38+00:00'}" +
            "]}";

        GetPlayerInventoryFromPage(obj);
    }

    public void GetPlayerInventoryFromPage(string message)
    {
        //JsonSerializer serializer = new JsonSerializer();
        //readData = serializer.Deserialize(new JsonTextReader(new StringReader(message)), typeof(SupremacyGameObject)) as SupremacyGameObject;

        readData = JsonConvert.DeserializeObject<SupremacyGameObject>(message, new SiloItemConterter());

        //Debug.Log(readData);
    }


    public void SiloReady()
    {
        WebGLPluginJS.SiloReady();
    }
}
