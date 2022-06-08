using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Zenject;
using SupremacyHangar.Runtime.Scriptable;

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
        string obj = "{faction: 'factionID'," +
            "silos:[" +
            "{type: 'mech',id: 'mech/assetID',chassisId: 'Zaibatsu', skinID: 'CherryBlossom' }, " +
            "{type: 'lootBox', id: 'Rare', expires: 'iso8601timestamp'}," +
            "{type: 'mech',id: 'mech/assetID',chassisId: 'RedMountain', skinID: 'Evo' }, " +
            "{type: 'mech',id: 'mech/assetID',chassisId: 'BostonCybernetics', skinID: 'Cyber' }, " +
            "{type: 'lootBox', id: 'lootbot/assetID', expires: 'iso8601timestamp'}" +
            "]}"; 

        AnnotatedDeserialize(obj);
    }

    private void AnnotatedDeserialize(string message)
    {
        Debug.Log("AnnotatedDeserialize--------------------");
        //Debug.Log("Got this \n --" + message);
        JsonSerializer serializer = new JsonSerializer();
        readData = serializer.Deserialize(new JsonTextReader(new StringReader(message)), typeof(SupremacyGameObject)) as SupremacyGameObject;
        //Debug.Log("faction " + readData.factionID);

        //foreach (var item in readData.silos)
        //{
        //    Debug.Log("Type " + item.type);
        //    Debug.Log("id " + item.id);
        //    if (item.expires != null)
        //    {
        //        Debug.Log("Reading loot box");
        //        Debug.Log("Expires " + item.expires);
        //    }
        //    else
        //    {
        //        Debug.Log("chassis " + item.chassisId);
        //        Debug.Log("skin " + item.skinId);
        //    }
        //}
    }

    //public void spawnMechFromPage(string message)
    //{
        
    //    Debug.Log("Trying to spawn from page " + RoomManager.instance.MechSpawners.Count);
    //    RoomManager.instance.spawnMech(index);
    //    Debug.Log("Spawned");
    //}

    //public void spawnMechFromUnity()
    //{
    //    Debug.Log("Trying to spawn from page " + RoomManager.instance.MechSpawners.Count);
    //    RoomManager.instance.spawnMech(index);
    //    Debug.Log("Spawned");
    //}
}
