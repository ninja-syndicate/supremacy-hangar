using System;
using UnityEngine;

namespace SupremacyHangar.Runtime.ScriptableObjects
{

    [CreateAssetMenu(fileName = "NewJoin", menuName = "Supremacy/ConnetivityJoin")]
    public class ConnectivityJoin : ScriptableObject, ISerializationCallbackReceiver
    {
        public Guid Id { get; internal set; } = Guid.NewGuid();

        [SerializeField] public byte[] idBytes;

        public virtual void OnBeforeSerialize()
        {
            if (Id == Guid.Empty) return;
            idBytes = Id.ToByteArray();
        }

        public virtual void OnAfterDeserialize()
        {
            Id = idBytes != null ? new Guid(idBytes) : Guid.Empty;
        }

        //public override bool Equals(object other)
        //{
        //    Debug.Log("Override equals");
        //    if (other == null) return false;
        //    ConnectivityJoin join = other as ConnectivityJoin;

        //    return join.Id.Equals(Id);
        //}

        //public override int GetHashCode()
        //{
        //    return Id.GetHashCode();
        //}
    }
}