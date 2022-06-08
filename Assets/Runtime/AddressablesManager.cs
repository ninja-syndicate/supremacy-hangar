using SupremacyHangar.Scriptable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

[Serializable]
public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
{
    public AssetReferenceAudioClip(string guid) : base(guid) { }
}

public class LoadedAsset
{
    public GameObject mech = null;
    public Skin skin = null;
    public AssetReference assetReference = null;
}

[Serializable]
public class IntLoadedAssetDictionary : SerializableDictionary<int, LoadedAsset> { }

public class AddressablesManager : MonoInstaller
{
    public AssetReference targetMech { get; set; }
    public AssetReference targetSkin { get; set; }

    private AssetReference previousMech;

    public string factionName { get; set; }

    private LoadedAsset myMech { get; set; } = new LoadedAsset();

    public override void InstallBindings()
    {
        Container.Bind<AddressablesManager>().FromInstance(this).AsSingle().NonLazy();
    }

    // Start is called before the first frame update
    public override void Start()
    {
        Debug.Log("Initializing Addressables...");
        Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
    }

    private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
    {
        //Debug.Log("Initialized Addressables...");

        //Load Faction specific hallway

        //Debug.Log("Loaded intial assets");
    }

    private void loadSkinReference(Action<Skin> callBack)
    {
        if (myMech.skin == null)
        {
            Debug.Log("loading skin");
            //loadedAssets.Add(targetSkin, new LoadedAsset());
            targetSkin.LoadAssetAsync<Skin>().Completed += (skin) =>
            {
                Debug.Log("saving Skin" + skin.Result);
                myMech.skin = skin.Result;
                callBack(myMech.skin);
            };
        }
        else
        {
            Debug.Log("existing skin");
            //Skin alreasy loaded
            callBack(myMech.skin);
        }
    }

    private void loadMechReference(Action<GameObject> callBack)
    {
        //if (loadedAssets.ContainsKey(targetMech)
        if (myMech.mech == null)
        {
            previousMech = targetMech;
            Debug.Log("loading mech");
            //loadedAssets.Add(targetMech, new LoadedAsset());
            targetMech.LoadAssetAsync<GameObject>().Completed += (mech) =>
            {
                Debug.Log("saving Mech");
                myMech.mech = mech.Result;

                callBack(myMech.mech);
            };
        }
        else
        {//Mech already loaded
            Debug.Log("Existing mech");
            callBack(myMech.mech);
        }
    }

    public void spawnMech(Transform spawnLocation)
    {
        //When mech out of view release addressables
        unloadMech();

        //Load new Mech & Skin
        loadMechReference(
            (result) =>
            {
                targetMech.InstantiateAsync(spawnLocation.position, spawnLocation.rotation, spawnLocation).Completed += (mech) =>
                {
                    myMech.mech = mech.Result;
                    if (targetSkin == null) return;
                    loadSkinReference(
                        (skin) =>//22.77
                        {
                            Debug.Log("Setting Skin");
                            MeshRenderer mechMesh = myMech.mech.GetComponentInChildren<MeshRenderer>();
                            mechMesh.sharedMaterials = skin.mats;
                            mechMesh.enabled = true;
                        }
                    );
                };
            });
    }

    public void unloadMech(bool completeUnload = false)
    {
        if (myMech.skin != null)
        {
            Addressables.Release(myMech.skin);
            myMech.skin = null;
        }

        if (targetMech != previousMech &&
            previousMech != null)
        {
            Addressables.ReleaseInstance(myMech.mech);
            myMech.mech = null;
        }
        else
        {
            Destroy(myMech.mech);
        }

        if(completeUnload)
            Addressables.Release(previousMech);
    }
}