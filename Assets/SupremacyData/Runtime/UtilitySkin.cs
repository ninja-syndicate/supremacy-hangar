using UnityEngine;

namespace SupremacyData.Runtime
{
    public class UtilitySkin : BaseRecord
    {
        public UtilityModel UtilityModel => utilityModel;
        public UtilityModel.ModelType Type => type;
        
        [SerializeField] [SerializeReference] internal UtilityModel utilityModel;
        [SerializeField] [SerializeReference] internal UtilityModel.ModelType type;
    }
}