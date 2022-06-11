using UnityEngine;

namespace SupremacyData.Runtime
{
    public class MechModel : BaseRecord
    {
        public enum ModelType
        {
            Humanoid,
            Platform,
        }
        
        //TODO: add skin id
        public Brand Brand => brand;
        public ModelType Type => type;
        
        [SerializeField] [SerializeReference] internal Brand brand;
        [SerializeField] [SerializeReference] internal ModelType type;
    }
}