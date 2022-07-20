using SupremacyHangar.Runtime.Environment;
using SupremacyHangar.Runtime.Types;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace SupremacyHangar.Runtime.Actors.Player
{
    public class SiloListing : MonoBehaviour
    {
        private int maxSiloOffset;
        private List<SiloItem> playerSilos;
        private TMP_Dropdown myDropdown;

        [Inject]
        public void Construct(EnvironmentManager environmentManager)
        {
            maxSiloOffset = environmentManager.MaxSiloOffset;
            playerSilos = environmentManager.PlayerInventory.Silos;
            SetupDropdown();
        }

        private void SetupDropdown()
        {
            myDropdown = GetComponent<TMP_Dropdown>();
            myDropdown.ClearOptions();
            for (int i = 0; i < maxSiloOffset;)
            {
                var newOption = new TMP_Dropdown.OptionData();
                newOption.text = SiloContentType(playerSilos[i]);
                i++;
                if (i < maxSiloOffset)
                    newOption.text += " | " + SiloContentType(playerSilos[i]);
                else
                    newOption.text = "Empty";

                i++;
                myDropdown.options.Add(newOption);
            }
            myDropdown.value = 0;
        }

        private string SiloContentType(SiloItem item)
        {
            switch (item)
            {
                case Weapon weapon:
                    return "Weapon";
                case Mech mech:
                    return "Mech";
                case EmptySilo empty:
                    return "Empty";
                case MysteryCrate crate:
                    return "Mystery Crate";
            }
            return "Empty";
        }
    }
}
