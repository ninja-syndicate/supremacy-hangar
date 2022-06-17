using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SupremacyHangar.Runtime.Actors.SiloHallway
{
    [RequireComponent(typeof(Canvas))]
    public class SiloHallwayUI : MonoBehaviour
    {
        [SerializeField] private Color testFactionColorChange;
        
        [SerializeField] private Image[] factionColorImages;
        [SerializeField] private Button[] factionColorButtons;
        [SerializeField] private TMP_Text[] factionColorText;
        [SerializeField] private float darkerFactionColorMultiplier;
        [SerializeField] private Image[] darkerFactionColorImages;

        [SerializeField] private TMP_Text siloNumber;
        [SerializeField] private TMP_Text siloContentsType;
        [SerializeField] private TMP_Text siloContentsName1;
        [SerializeField] private TMP_Text siloContentsName2;
        [SerializeField] private Button loadButton;
        
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
        
#if UNITY_EDITOR
        public void Update()
        {
            if (currentFactionColor != testFactionColorChange)
            {
                UpdateFactionColor(testFactionColorChange);
            }
        }
#endif        
    }
}