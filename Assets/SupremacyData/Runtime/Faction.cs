using UnityEngine;

namespace SupremacyData.Runtime
{
    public class Faction : BaseRecord
    {
        public string Description => description;
        public string LogoURL => humanName;
        public string BackgroundURL => humanName;
        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public Color BackgroundColor => backgroundColor;
        
        [SerializeField] internal Color primaryColor;
        [SerializeField] internal Color secondaryColor;
        [SerializeField] internal Color backgroundColor;
        [SerializeField] internal string logoURL;
        [SerializeField] internal string backgroundURL;
        [SerializeField] internal string description;
    }
}