using System;
using SupremacyData.Runtime;
using SupremacyHangar.Runtime.Actors.Silo;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Silo;
using SupremacyHangar.Runtime.Types;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
        [SerializeField] private TMP_Text siloContentsName3;
        [FormerlySerializedAs("loadButton"), SerializeField] private Button interactionButton;
        [SerializeField] private Image interactionButtonProgress;
        [SerializeField] private TMP_Text interactionButtonText;

        [SerializeField] private Color startFactionColor;
        [SerializeField] private float clockUpdateDelay = 0.25f;

        [SerializeField] private string loadRequestText;
        [SerializeField] private string loadingText;
        [SerializeField] private string openRequestText;
        [SerializeField] private string openingText;

        private Color currentFactionColor = Color.black;

        private SiloState siloState;
        private SignalBus bus;

        private bool enableCounter = false;
        private float nextClockUpdate = -1;
        private DateTime counterValue;

        private bool crateOpenSet;

        private bool subscribed;

        public void Awake()
        {
            UpdateFactionColor(startFactionColor);
        }

        [Inject]
        public void Inject(SiloState siloState, SignalBus bus)
        {
            this.siloState = siloState;
            siloNumber.text = (siloState.SiloNumber + 1).ToString();

            this.bus = bus;
            subscribed = true;
            bus.Subscribe<AssetLoadingProgressSignal>(ProgressUpdated);

            PopulateDisplay();

            SiloStateChanged(siloState.CurrentState);
            siloState.OnStateChanged += SiloStateChanged;
        }

        private void PopulateDisplay()
        {
            switch (siloState.Contents)
            {
                case Mech mech:
                    UpdateTypeString(mech);
                    UpdateName1(mech);
                    UpdateName2(mech);
                    break;
                case MysteryCrate box:
                    UpdateTypeString(box);
                    UpdateName2(siloState.CurrentFaction, box);
                    siloContentsName3.text = "Building";
                    enableCounter = true;
                    counterValue = box.CanOpenOn;
                    break;
                case Weapon weapon:
                    UpdateTypeString("Weapon");
                    UpdateName1(weapon);
                    UpdateName2(weapon);
                    break;
                //case Utility utility:
                //UpdateTypeString(utility.UtilityModelDetails.Data.Type.ToString());
                //UpdateName1(utility);
                //UpdateName2(utility);
                //break;
                default:
                    UpdateTypeString("Empty");
                    UpdateName1("");
                    UpdateName2("");
                    break;
            }
        }

        public void OnEnable()
        {
            if (bus == null || subscribed) return;
            bus.Subscribe<AssetLoadingProgressSignal>(ProgressUpdated);
            subscribed = true;
        }

        public void OnDisable()
        {
            if (bus == null) return;
            bus.TryUnsubscribe<AssetLoadingProgressSignal>(ProgressUpdated);
        }

        private void ProgressUpdated(AssetLoadingProgressSignal signal)
        {
            switch (siloState.CurrentState)
            {
                case SiloState.StateName.NotLoaded:
                case SiloState.StateName.Unloading:
                case SiloState.StateName.Loaded:
                case SiloState.StateName.LoadedWithCrate:
                    return;
            }

            interactionButtonProgress.fillAmount = signal.PercentageComplete;
        }

        private void SiloStateChanged(SiloState.StateName newState)
        {
            //Update the action button
            switch (newState)
            {
                case SiloState.StateName.NotLoaded:
                case SiloState.StateName.Unloading:
                    interactionButton.gameObject.SetActive(true);
                    interactionButtonProgress.fillAmount = 1;
                    interactionButtonText.text = loadRequestText;
                    break;
                case SiloState.StateName.LoadingSilo:
                    interactionButton.gameObject.SetActive(true);
                    interactionButtonProgress.fillAmount = 0;
                    interactionButtonText.text = loadingText;
                    break;
                case SiloState.StateName.Loaded:
                    interactionButton.gameObject.SetActive(false);
                    PopulateDisplay();
                    break;
                case SiloState.StateName.LoadedWithCrate:
                    interactionButtonProgress.fillAmount = 1;
                    SetupOpenButton();
                    break;
                case SiloState.StateName.LoadingCrateContent:
                    interactionButton.gameObject.SetActive(true);
                    interactionButtonProgress.fillAmount = 0;
                    interactionButtonText.text = openingText;
                    break;
            }
        }

        private void SetupOpenButton()
        {
            interactionButtonText.text = openRequestText;
            
            var crate = siloState.Contents as MysteryCrate;
            if (crate == null)
            {
                Debug.LogError("could not set crate button state because we don't have a crate!");
                interactionButton.gameObject.SetActive(false);
                return;
            }
            
            interactionButton.gameObject.SetActive(crate.Openable);
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
            if (crateOpenSet) return;
            var now = DateTime.UtcNow;
            var diff = counterValue - now;
            if (diff <= TimeSpan.Zero && !crateOpenSet)
            {
                SetupOpenButton();
                siloState.CrateCanOpen();
                crateOpenSet = true;
                siloContentsName1.text = TimeSpan.Zero.ToString("hh':'mm':'ss");
                siloContentsName3.text = "Built";
                Debug.Log("built crate", this);
                return;
            }
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

        //private void UpdateName1(Utility utility)
        //{
        //    if (utility.UtilityModelDetails == null)
        //    {
        //        siloContentsName1.text = "Unable to load";
        //        return;
        //    }
        //    siloContentsName1.text = utility.UtilityModelDetails.Data.HumanName;
        //}


        private void UpdateName3(Mech mech)
        {
            //siloContentsName3.text = mech.MechChassisDetails.DataMechModel.DisplayName;
        }

        private void UpdateName1(Weapon weapon)
        {
            if (weapon.WeaponModelDetails == null)
            {
                siloContentsName1.text = "Unable to load";
                return;
            }
            siloContentsName1.text = weapon.WeaponModelDetails.Data.HumanName;
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

        private void UpdateName2(Weapon weapon)
        {
            if (weapon.WeaponSkinDetails == null)
            {
                siloContentsName2.text = "Unable to load";
                return;
            }
            siloContentsName2.text = weapon.WeaponSkinDetails.Data.HumanName;
        }

        //private void UpdateName2(Utility utility)
        //{
        //    if (utility.UtilitySkinDetails == null)
        //    {
        //        siloContentsName2.text = "Unable to load";
        //        return;
        //    }
        //    siloContentsName2.text = utility.UtilitySkinDetails.Data.HumanName;
        //}

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