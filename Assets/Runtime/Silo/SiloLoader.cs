using SupremacyData.Runtime;
using SupremacyHangar.Runtime.ContentLoader;
using SupremacyHangar.Runtime.Interaction;
using SupremacyHangar.Runtime.Types;
using UnityEngine;
using Zenject;
using MysteryCrate = SupremacyHangar.Runtime.Types.MysteryCrate;

namespace SupremacyHangar.Runtime.Silo
{
    public class SiloLoader : InteractionZoneResponder
    {
        [SerializeField] private SiloPositioner spawner;
        [SerializeField] private int siloOffset;

        [Inject]
        private AddressablesManager addressablesManager;
        
        [Inject]
        private SiloItem[] siloContents;
        
        private bool playerPresent;
        private FirstPersonController playerController;

        public override void OnPlayerEntered(GameObject go, FirstPersonController controller)
        {
            playerPresent = true;
            playerController = controller;
            playerController.OnInteractionTriggered += StartLoad;
            playerController.IncrementInteractionPromptRequests();
        }
        
        public override void OnPlayerExited()
        {
            if (!playerPresent) return;
            playerController.OnInteractionTriggered -= StartLoad;
            playerController.DecrementInteractionPromptRequests();
            playerPresent = false;
            playerController = null;
        }
        
        private void StartLoad()
        {
            var myContent = siloContents[siloOffset];
            bool empty = true;
            switch(myContent)
            {
                case Mech mech:
                    empty = !PopulateWithMech(mech);
                    break;
                case MysteryCrate box:
                    empty = false;
                    addressablesManager.TargetMech = box.MysteryCrateDetails.MysteryCrateReference;
                    addressablesManager.TargetSkin = null;
                    break;
                default:
                    Debug.LogWarning($"Unexpected type of {myContent.GetType()} - cowardly refusing to fill the silo", this);
                    break;
            }

            if(!empty) spawner.PrepareSilo();
        }

        private bool PopulateWithMech(Mech mech)
        {
            bool populated = true;
            if (mech.MechChassisDetails == null)
            {
                Debug.LogWarning($"Unmapped Mech ID {mech.StaticID} can't load silo", this);
                populated = false;
            }
            else
            {
                addressablesManager.TargetMech = mech.MechChassisDetails.MechReference;
            }

            if (mech.MechSkinDetails == null)
            {
                Debug.LogWarning($"Unmapped Skin ID {mech.StaticID} can't load silo", this);
                populated = false;
            }
            else
            {
                addressablesManager.TargetSkin = mech.MechSkinDetails.SkinReference;
            }

            return populated;
        }
    }
}