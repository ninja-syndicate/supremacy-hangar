using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SupremacyData.Editor.Importers
{
    public class WeaponModels : Base
    {
        private static readonly string[] CsvHeaders =
        {   
            "id",
            "brand_id",
            "label",
            "weapon_type",
            "default_skin_id",
            "deleted_at",
            "updated_at",
            "created_at",
        };
        
        public WeaponModels(ILogInterface logger, string staticDataDir) : base(logger, staticDataDir) { }
        protected override string Filename => "weapon_models.csv";
        protected override string[] Headers => CsvHeaders;
        public override string ImporterName => "Weapon Models";
        protected override void SetupForImport(Runtime.Data data)
        {
            data.weaponModels ??= new List<Runtime.WeaponModel>();
        }

        protected override void ProcessRecord(Runtime.Data data, int index, string[] fields)
        {
            if (!TryParseGuid(index, fields[0], "id", out var id)) return;
            
            var weaponModel = data.weaponModels.Find(x => x.Id == id);
            if (weaponModel == null)
            {
                weaponModel = ScriptableObject.CreateInstance<Runtime.WeaponModel>();
                AssetDatabase.AddObjectToAsset(weaponModel, data);
                weaponModel.Id = id;
                data.weaponModels.Add(weaponModel);
            }

            weaponModel.humanName = fields[2];
            weaponModel.name = $"Weapon Model - {weaponModel.humanName}";
            if (!ParseType(fields[3], out weaponModel.type))
            {
                logger.LogError($"{ImporterName} data - unknown weapon type {fields[3]} from {dataPath}:{index}");
            }
            
            if (string.IsNullOrWhiteSpace(fields[1])) return;

            if (!TryParseGuid(index, fields[1], "brand ID", out var brandId)) return;
            var brand = data.brands.Find(x => x.Id == brandId);
            if (brand == null)
            {
                logger.LogError($"{ImporterName} data - could not find brand with GUID {brandId} from {dataPath}:{index}");
            }
            else
            {
                weaponModel.brand = brand;
            }
        }

        internal static bool ParseType(string field, out Runtime.WeaponModel.ModelType type)
        {
            type = Runtime.WeaponModel.ModelType.LightningGun;
            switch (field.ToLowerInvariant())
            {
                case "lightning gun":
                    type = Runtime.WeaponModel.ModelType.LightningGun;
                    return true;
                case "minigun":
                    type =  Runtime.WeaponModel.ModelType.Minigun;
                    return true;
                case "missile launcher":
                    type =  Runtime.WeaponModel.ModelType.MissileLauncher;
                    return true;
                case "bfg":
                    type =  Runtime.WeaponModel.ModelType.BFG;
                    return true;
                case "flamethrower":
                    type =  Runtime.WeaponModel.ModelType.Flamethrower;
                    return true;
                case "flak":
                    type =  Runtime.WeaponModel.ModelType.FlakGun;
                    return true;
                case "cannon":
                    type =  Runtime.WeaponModel.ModelType.Cannon;
                    return true;
                case "grenade launcher":
                    type =  Runtime.WeaponModel.ModelType.GrenadeLauncher;
                    return true;
                case "machine gun":
                    type =  Runtime.WeaponModel.ModelType.MachineGun;
                    return true;
                case "laser beam":
                    type =  Runtime.WeaponModel.ModelType.LaserBeam;
                    return true;
                case "sword":
                    type =  Runtime.WeaponModel.ModelType.Sword;
                    return true;
                case "sniper rifle":
                    type =  Runtime.WeaponModel.ModelType.SniperRifle;
                    return true;
                case "plasma gun":
                case "rifle":
                    type =  Runtime.WeaponModel.ModelType.PlasmaRifle;
                    return true;
                default:
                    return false;
            }
        }        
        
    }
}