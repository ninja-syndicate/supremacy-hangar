using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.ScriptableObjects;
using SupremacyHangar.Runtime.Types;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SupremacyHangar.Runtime.Silo
{
    public class SiloInfoDisplay : MonoBehaviour
    {
        private EnvironmentManager _environmentManager;
        private SiloItem[] _siloContent;

        [SerializeField]
        private Text _siloNumber;

        [SerializeField]
        private Text[] mechDisplayLayout = new Text[4];

        [SerializeField]
        private Text[] lootBoxDisplayLayout = new Text[4];

        [SerializeField]
        private int siloIndex = 0;

        [Inject]
        public void Construct(EnvironmentManager environmentManager, SiloItem[] siloContents)
        { 
            _environmentManager = environmentManager;
            _siloContent = siloContents;
            switch(_siloContent[siloIndex])
            {
                case Mech mech:
                    SetMechInfoDisplay(mech);
                    break;
                case MysteryBox box:
                    SetLootBoxInfoDisplay(box);
                    break;
                default:
                    SetEmptyInfoDisplay();
                    break;
            }
        }

        private void SetEmptyInfoDisplay()
        {
            _siloNumber.text = "" + (_environmentManager.SiloOffset + siloIndex + 1);
            mechDisplayLayout[0].text = "Empty";
            mechDisplayLayout[1].text = "";
            mechDisplayLayout[2].text = "";
        }

        //ToDo: convert to key values not GUIDs & working timer and enable layout after its set
        private void SetMechInfoDisplay(Mech mech)
        {
            _siloNumber.text = "" + (_environmentManager.SiloOffset + siloIndex + 1);
            mechDisplayLayout[0].text = "Mech";
            mechDisplayLayout[1].text = mech.MechChassisDetails.DataMechModel.HumanName;
            mechDisplayLayout[2].text = mech.MechSkinDetails.DataMechSkin.HumanName;
        }

        private void SetLootBoxInfoDisplay(MysteryBox box)
        {
            _siloNumber.text = "" + (_environmentManager.SiloOffset + siloIndex + 1);
            mechDisplayLayout[0].text = "Myster Box";
            mechDisplayLayout[1].text = box.MysteryCrateDetails.DataMysteryCrate.HumanName;
            mechDisplayLayout[2].text = box.can_open_on;
        }
    }
}
