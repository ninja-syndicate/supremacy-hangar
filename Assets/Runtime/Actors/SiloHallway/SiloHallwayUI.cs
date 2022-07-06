using System;
using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using MysteryCrate = SupremacyHangar.Runtime.Types.MysteryCrate;

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

        [SerializeField] private Color startFactionColor;
        [SerializeField] private int siloOffset;
        [SerializeField] private float clockUpdateDelay = 0.25f;

        private Color currentFactionColor = Color.black;

        private bool enableCounter = false;
        private float nextClockUpdate = -1;
        private DateTime counterValue;

        public void Awake()
        {
            UpdateFactionColor(startFactionColor);
        }
        
        [Inject]
        public void SetDependencies(AddressablesManager addressablesManager, EnvironmentManager environmentManager)
        {
            int mySiloNumber = environmentManager.SiloOffset + siloOffset;

            SiloItem myContents;
            if (mySiloNumber < environmentManager.SiloItems.Count)
            {
                myContents = environmentManager.SiloItems[mySiloNumber];
            }
            else
            {
                myContents = new SiloItem();
            }
            siloNumber.text = (mySiloNumber + 1).ToString();
            switch (myContents)
            {
                case Mech mech:
                    UpdateTypeString(mech);
                    UpdateName1(mech);
                    UpdateName2(mech);
                    break;
                case MysteryCrate box:
                    UpdateTypeString(box);
                    UpdateName2(addressablesManager.CurrentFaction, box);
                    enableCounter = true;
                    counterValue = box.CanOpenOn;
                    break;
               default:
                   UpdateTypeString("Empty");
                   UpdateName1("");
                   UpdateName2("");
                   break;
            }
        }

        public void Update()
        {
            if (enableCounter && nextClockUpdate <= Time.unscaledTime) UpdateCounter();

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

        private void UpdateCounter()
        {
            var now = DateTime.UtcNow;
            var diff = counterValue - now;
            siloContentsName1.text = diff.ToString(diff.Days > 0 ? "d':'hh':'mm':'ss" : "hh':'mm':'ss");
            nextClockUpdate = Time.unscaledTime + clockUpdateDelay;
        }

        private void UpdateTypeString(Mech mech)
        {
            siloContentsType.text = "War Machine";
        }

        private void UpdateTypeString(MysteryCrate crate)
        {
            switch (crate.MysteryCrateDetails.DataMysteryCrate.Type)
            {
                case SupremacyData.Runtime.MysteryCrate.ModelType.Mech:
                    siloContentsType.text = "War Machine Crate";
                    break;
                case SupremacyData.Runtime.MysteryCrate.ModelType.Weapon:
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
            if (mech.MechChassisDetails == null)
            {
                siloContentsName1.text = "Unable to load";
                return;
            }
            siloContentsName1.text = mech.MechChassisDetails.DataMechModel.HumanName;
        }

        private void UpdateName2(Faction currentFaction, MysteryCrate crate)
        {
            var boxFaction = crate.MysteryCrateDetails.DataMysteryCrate.Faction;
            siloContentsName2.text = boxFaction == currentFaction ? "" : boxFaction.HumanName;
        }
        
        public void UpdateName2(string newString)
        {
            siloContentsName2.text = newString;
        }     
        
        private void UpdateName2(Mech mech)
        {
            if (mech.MechSkinDetails == null)
            {
                siloContentsName2.text = "Unable to load";
                return;
            }
            siloContentsName2.text = mech.MechSkinDetails.DataMechSkin.HumanName;
        }
    }
}