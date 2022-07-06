using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SupremacyHangar.Runtime.Types;
using System;
using UnityEngine;

public class SiloItemConterter : JsonConverter<SiloItem>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, SiloItem value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override SiloItem ReadJson(JsonReader reader, Type objectType, SiloItem existingValue, bool hasExistingValue, JsonSerializer serializer)
    {

        JObject jo = JObject.Load(reader);

        string type = (string)jo["type"];

        SiloItem siloItem;
        
        switch (type)
        {
            case "mech":
                siloItem = new Mech();
                break;
            case "mystery_crate":
                siloItem = new MysteryCrate();
                break;
            case "skin":
                siloItem = new Skin();
                break;
            case "weapon":
                siloItem = new Weapon();
                break;
            case "power_core":
                siloItem = new PowerCore();
                break;
            default:
                Debug.LogError($"Unknown silo item type: {type}");
                return null;
        }

        serializer.Populate(jo.CreateReader(), siloItem);

        return siloItem;
    }
}
