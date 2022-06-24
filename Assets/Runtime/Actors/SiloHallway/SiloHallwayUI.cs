using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.SiloHallway
{
    [RequireComponent(typeof(Canvas))]
    public class SiloHallwayUI : MonoBehaviour
    {
        [SerializeField] private Image[] factionColorImages;
        [SerializeField] private Button[] factionColorButtons;
        [SerializeField] private TMP_Text[] factionColorText;
        [SerializeField] private float darkerFactionColorMultiplier;
        [SerializeField] private Image[] darkerFactionColorImages;

        [SerializeField] private Image factionLogo;
        [SerializeField] private TMP_Text siloNumber;
        [SerializeField] private TMP_Text siloContentsType;
        [SerializeField] private TMP_Text siloContentsName1;
        [SerializeField] private TMP_Text siloContentsName2;
        [SerializeField] private Button loadButton;

        [SerializeField] private int siloOffset;
        
        private Color currentFactionColor = Color.black;

        [Inject]
        public void SetDependencies(AddressablesManager addressablesManager, EnvironmentManager environmentManager)
        {
            int mySiloNumber = environmentManager.SiloOffset + siloOffset;
            var myContents = environmentManager.SiloItems[mySiloNumber];
            siloNumber.text = (mySiloNumber + 1).ToString();
            switch (myContents)
            {
                case Mech mech:
                    UpdateTypeString(mech);
                    UpdateName1(mech);
                    UpdateName2(mech);
                    break;
                case MysteryBox box:
                    UpdateTypeString(box);
                    UpdateName1(addressablesManager.CurrentFaction, box);
                    //TODO: this needs to be changed to a counter...
                    UpdateName2(box.CanOpenOn);
                    break;
               default:
                   UpdateTypeString("Empty");
                   UpdateName1("");
                   UpdateName2("");
                   break;
            }
        }
        
        public void UpdateFactionColor(Color newColor)
        {
            if (newColor == currentFactionColor) return;
            currentFactionColor = newColor;
            foreach (var image in factionColorImages)
            {
                image.color = newColor;
                image.SetMaterialDirty();
            }

            foreach (var button in factionColorButtons)
            {
                button.image.color = newColor;
                button.image.SetMaterialDirty();
            }

            foreach (var text in factionColorText)
            {
                text.color = newColor;
                text.SetMaterialDirty();
            }
            foreach (var image in darkerFactionColorImages)
            {
                image.color = newColor * darkerFactionColorMultiplier;
                image.SetMaterialDirty();
            }
        }

        public void UpdateFactionLogo(Sprite sprite)
        {
            if (factionLogo == null)
            {
                Debug.LogError("No Faction Logo Set!", this);
                return;
            }

            if (sprite == factionLogo.sprite) return;
            factionLogo.sprite = sprite;
            factionLogo.SetNativeSize();
            factionLogo.SetMaterialDirty();
        }

        public void UpdateSiloNumber(int newInt)
        {
            siloNumber.text = newInt.ToString();
        }
        
        public void UpdateTypeString(string newString)
        {
            siloContentsType.text = newString;
        }
        
        private void UpdateTypeString(Mech mech)
        {
            siloContentsType.text = "Mech";
        }

        private void UpdateTypeString(MysteryBox box)
        {
            switch (box.MysteryCrateDetails.DataMysteryCrate.Type)
            {
                case MysteryCrate.ModelType.Mech:
                    siloContentsType.text = "War Machine Crate";
                    break;
                case MysteryCrate.ModelType.Weapon:
                    siloContentsType.text = "Weapon Crate";
                    break;
                default:
                    siloContentsType.text = "Mystery Crate";
                    Debug.LogError("Unknown crate type!");
                    break;
            }
        }        
        
        public void UpdateName1(string newString)
        {
            siloContentsName1.text = newString;
        }

        private void UpdateName1(Mech mech)
        {
            siloContentsName1.text = mech.MechChassisDetails.DataMechModel.HumanName;
        }

        private void UpdateName1(Faction currentFaction, MysteryBox box)
        {
            var boxFaction = box.MysteryCrateDetails.DataMysteryCrate.Faction;
            siloContentsName1.text = boxFaction == currentFaction ? "" : boxFaction.HumanName;
        }
        
        public void UpdateName2(string newString)
        {
            siloContentsName2.text = newString;
        }     
        
        private void UpdateName2(Mech mech)
        {
            siloContentsName2.text = mech.MechSkinDetails.DataMechSkin.HumanName;
        }             
    }
}