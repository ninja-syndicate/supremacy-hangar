using System;
using UnityEngine;

namespace SupremacyHangar.Runtime.ScriptableObjects
{
    public abstract class BaseConnection : ScriptableObject, ISerializationCallbackReceiver
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

        public override bool Equals(object other)
        {
            if (other == null) return false;
            BaseConnection join = other as BaseConnection;

            return join.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}