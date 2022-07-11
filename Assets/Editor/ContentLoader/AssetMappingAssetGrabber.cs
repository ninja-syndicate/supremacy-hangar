using SupremacyHangar.Editor;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.ContentLoader.Types;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.AddressableAssets;
using static SupremacyHangar.Editor.ContentLoader.AssetMappingsEditor;

namespace SupremacyHangar.Editor.ContentLoader
{
    public class AssetMappingAssetGrabber
    {
        public Dictionary<ListType, string[]> TypeToNameMap => typeToNameMap;
        private Dictionary<ListType, string[]> typeToNameMap = new();

        public AssetMappingAssetGrabber()
        {
            typeToNameMap.Clear();
            //String Order
            /*
                0 - list label
                1 - data reference property name
                2 - asset reference property name
            */
            typeToNameMap.Add(ListType.Faction, new[] { "Factions", "dataFaction", "connectivityGraph" });
            typeToNameMap.Add(ListType.MysteryCrate, new[] { "Mystery Crates", "dataMysteryCrate", "mysteryCrateReference" });
            typeToNameMap.Add(ListType.MechChassis, new[] { "Mech Chassis", "dataMechModel", "mechReference" });
            typeToNameMap.Add(ListType.MechSkin, new[] { "Mech Skins", "dataMechSkin", "skinReference" });
            typeToNameMap.Add(ListType.WeaponModel, new[] { "Weapon Model", "data", "reference" });
            typeToNameMap.Add(ListType.WeaponSkin, new[] { "Weapon Skin", "data", "reference" });
            typeToNameMap.Add(ListType.PowerCore, new[] { "Power Core", "data", "reference" });
            
        }
        public AssetReferenceEnvironmentConnectivity GetFactionGraphAssetReferenceValue(SerializedProperty assetReference, ref string label)
        {
            System.Type type = MyExtensionMethods.GetType(assetReference, ReferenceTypes.Graph);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReferenceEnvironmentConnectivity>(fieldInfo, ref label);
        }

        public AssetReference GetAssetReferenceValue(SerializedProperty assetReference, ref string label)
        {
            System.Type type = MyExtensionMethods.GetType(assetReference, ReferenceTypes.Graph);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReference>(fieldInfo, ref label);
        }

        public AssetReferenceSkin GetSkinAssetReferenceValue(SerializedProperty assetReference, ref string label)
        {
            System.Type type = MyExtensionMethods.GetType(assetReference, ReferenceTypes.Skin);
            FieldInfo fieldInfo = MyExtensionMethods.GetFieldViaPath(type, assetReference.propertyPath);
            return assetReference.GetActualObjectForSerializedProperty<AssetReferenceSkin>(fieldInfo, ref label);
        }
    }


    public enum ReferenceTypes
    {
        Graph,
        Skin,
    }

    public static class MyExtensionMethods
    {
        public static System.Type GetType(SerializedProperty property, ReferenceTypes type)
        {
            var parentType = property.serializedObject.targetObject as AssetMappings;

            switch (type)
            {
                case ReferenceTypes.Graph:
                    foreach (var s in parentType.FactionMappingByGuid.Values)
                        return s.GetType();
                    break;
                case ReferenceTypes.Skin:
                    foreach (var s in parentType.MechSkinMappingByGuid.Values)
                        return s.GetType();
                    break;
                default:
                    break;
            }

            return null;
        }

        public static FieldInfo GetFieldViaPath(this System.Type type, string path)
        {
            if (type == null) return null;
            FieldInfo[] fi = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
                return fi[1];
            else return null;
        }
    }
}
