using SupremacyData.Editor;
using UnityEditor;
using UnityEngine;

namespace SupremacyHangar
{
    public class AssetMappingsEditorHelpWindow : EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private string factionImportingHelp = "";
        private string objectImportingHelp = "";
        private string skinAssetImportingHelp = "";
        private string keyImportingHelp = "";

        private LogWidget logWidget = new();

        public void OnEnable()
        {
            keyImportingHelp = "1) Ensure static data exists with values\n" +
                "2) Click import: DONE!";

            factionImportingHelp = "1) Graph name must contain first word of key\n" +
                "       e.g. Faction - Boston Cybernetics looks for graph containing Boston\n" +
                "2) Graph must be marked as Addressable\n" +
                "3) If no asset is found it used the last one in the list or leaves it empty";

            objectImportingHelp = "1) Asset must be the name of the model (key)\n" +
                "       e.g. Mech Model - Tenshi Mk1 expects a GamObject named Tenshi Mk1\n" + 
                "2) GameObject must be marked as Addressable\n" +
                "3) If no asset is found it used the last one in the list or leaves it empty";

            skinAssetImportingHelp = "1) Skin asset must be addressable\n" +
                "2) Skin asset must be named the same as in static Data\n" +
                "3) For mech skins the folder must go under Assets/Content/Mechs\n" +
                "  3.1) Mech skins must be stored in a folder with the name of the Mech it belongs to\n" +
                "       e.g. Mech Skin - Annihilator - Gold will search Assets/Content/Mechs & all subdirectories\n" +
                "            for the Annihilator folder and get the skin named Gold\n" +
                "4) For Weapon Skins the folder must go under Assets/Content/Weapons\n" +
                "  4.1) Weapon skins must be stored in a folder with the name of the Weapons type\n" +
                "       e.g. Weapon Skin - ARCHON SPINSHOT MING-549 - Daison Avionics (type minigun)\n" +
                "            will search Assets/Content/Weapons & all subdirectories for\n" +
                "            the Minigun folder and get the skin named Daison Avionics\n" +
                "6) If the folder or skin does not exist it will use the last set skin or none";

            logWidget.LogNormal("Purpose: Import missing keys and try to import assets. If key exists with no asset the importer WILL NOT try to set it.\n\n");
            logWidget.LogNormal("How to import keys\n");
            logWidget.LogNormal(keyImportingHelp);

            logWidget.LogNormal("\nHow to import assets\n");
            logWidget.LogNormal("Import Faction assets\n");
            logWidget.LogNormal(factionImportingHelp);
            
            logWidget.LogNormal("\nImport Object (Mech/Crate/Weapon) assets\n");
            logWidget.LogNormal(objectImportingHelp);
            
            logWidget.LogNormal("\nImport Skin assets\n");
            logWidget.LogNormal(skinAssetImportingHelp);
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            logWidget.Render(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            EditorGUILayout.EndScrollView();
        }
    }
}
