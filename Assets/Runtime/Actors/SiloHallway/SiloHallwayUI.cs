using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        
        public void UpdateName1(string newString)
        {
            siloContentsName1.text = newString;
        }
        
        public void UpdateName2(string newString)
        {
            siloContentsName2.text = newString;
        }     
    }
}