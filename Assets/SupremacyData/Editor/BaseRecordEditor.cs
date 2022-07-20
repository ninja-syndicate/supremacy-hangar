using UnityEditor;

namespace SupremacyData.Editor
{
    public abstract class BaseRecordEditor<T> : UnityEditor.Editor where T : Runtime.BaseRecord
    {
        protected T TargetRecord => targetRecord;
        protected bool TargetRecordSet => targetRecordSet;
        
        private T targetRecord;
        private bool targetRecordSet;
        
        public virtual void OnEnable()
        {
            targetRecord = serializedObject.targetObject as T;
            targetRecordSet = targetRecord != null;
        }

        public override void OnInspectorGUI()
        {
            if (!TargetRecordSet)
            {
                EditorGUILayout.LabelField("Invalid Data!");
                return;
            }
            
            RenderID();
            RenderHumanName();
        }
        
        protected void RenderID()
        {
            EditorGUILayout.TextField("Record ID", targetRecord.Id.ToString());
        }

        protected void RenderHumanName()
        {
            EditorGUILayout.TextField("Human Name", targetRecord.HumanName);
        }
        
    }
}