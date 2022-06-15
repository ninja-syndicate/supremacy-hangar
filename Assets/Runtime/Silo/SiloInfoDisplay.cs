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
        private SupremacyDictionary _supremacyDictionary;

        [SerializeField]
        private Text _siloNumber;

        [SerializeField]
        private Text[] mechDisplayLayout = new Text[4];

        [SerializeField]
        private Text[] lootBoxDisplayLayout = new Text[4];

        [SerializeField]
        private int siloIndex = 0;

        [Inject]
        public void Construct(EnvironmentManager environmentManager, SiloItem[] siloContents, SupremacyDictionary supremacyDictionary)
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

        //private SiloContent[] currentSiloInfo()
        //{
        //    SiloContent[] temp = new SiloContent[2] { new(), new() };
        //    int i = 0;

        //    foreach (SiloContent silo in temp)
        //    {
        //        //Stop due to silo limit reached
        //        if ((_environmentManager.SiloOffset + i) >= _environmentManager.MaxSiloOffset)
        //            break;

        //        silo.type = _siloContent[i].type;
        //        if (silo.type.Contains("mech"))
        //        {
        //            silo.skinId = _supremacyDictionary.AllSkinsDictionary[_siloContent[i].chassisId][_siloContent[i].skinId].editorAsset.ToString();
        //            silo.chassisId = _supremacyDictionary.mechDictionary[_siloContent[i].chassisId].editorAsset.ToString();

        //            //Only get the asset name (not its type)
        //            silo.skinId = silo.skinId.Substring(0, silo.skinId.IndexOf('('));
        //            silo.chassisId = silo.chassisId.Substring(0, silo.chassisId.IndexOf('('));
        //        }
        //        else if (silo.type.Contains("loot"))
        //        {
        //            //Todo get object string equivalent to ID (e.g. Red mountain)
        //            silo.id = _siloContent[i].id;
        //            silo.expires = _siloContent[i].expires;

        //            //Only get the asset name (not its type)
        //            //silo.id = silo.id.Substring(0, silo.id.IndexOf('('));
        //            //silo.chassisId = silo.expires;
        //        }
        //        else
        //            Debug.LogError($"Unknown silo type: {silo.type}", this);

        //        ++i;
        //    }
        //    return temp;
        //}

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
            mechDisplayLayout[1].text = mech.mech_id;
            mechDisplayLayout[2].text = mech.skin_id;
        }

        private void SetLootBoxInfoDisplay(MysteryBox box)
        {
            _siloNumber.text = "" + (_environmentManager.SiloOffset + siloIndex + 1);
            mechDisplayLayout[0].text = "Myster Box";
            mechDisplayLayout[1].text = box.ownership_id;
            mechDisplayLayout[2].text = box.can_open_on;
        }
    }
}
