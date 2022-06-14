using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SupremacyHangar.Runtime.Types;
using System;

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

        SiloItem siloItem = type.Contains("mech") ? new Mech() : new MysteryBox();

        serializer.Populate(jo.CreateReader(), siloItem);

        return siloItem;
    }
}
