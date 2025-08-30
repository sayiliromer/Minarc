using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Minarc.Authoring
{
    public class EntityReferenceAuthoring<T> : EntityPrefabRepoAuthoringRunner where T : unmanaged, IComponentData
    {
        public override void AddComponent(IBaker baker, Entity mainEntity, object result)
        {
            baker.AddComponent(mainEntity, (T)result);
        }

        public override object CreateInstance()
        {
            return new T();
        }

        public override Type GetComponentType()
        {
            return typeof(T);
        }
    }
    
    public abstract class EntityReferenceBaker<T> : Baker<T> where T : EntityPrefabRepoAuthoringRunner
    {
        public override void Bake(T authoring)
        {
            var memberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            object result = authoring.CreateInstance();
            var repoType = result.GetType();
            var mainEntity = GetEntity(TransformUsageFlags.None);
            if(authoring.Objects == null) return;
            for (int i = 0; i < authoring.Objects.Count; i++)
            {
                var go = authoring.Objects[i];
                var name = authoring.ObjectNames[i];
                var targetMember = repoType.GetField(name, memberFlags);
                var entity = GetEntity(go, TransformUsageFlags.None);
                if (entity != Entity.Null && targetMember != null) targetMember.SetValue(result, entity);
                    
            }
                
            authoring.AddComponent(this,mainEntity,result);;
        }
    }
    
    public abstract class EntityPrefabRepoAuthoringRunner : MonoBehaviour
    {
        public List<GameObject> Objects;
        public List<string> ObjectNames;
        public abstract void AddComponent(IBaker baker, Entity mainEntity, object result);
        public abstract object CreateInstance();
        public abstract Type GetComponentType();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EntityPrefabRepoAuthoringRunner), true)]
    public class EntityPrefabRepoAuthoringBaseEditor : Editor
    {
        private SerializedProperty _objects;
        private SerializedProperty _names;
    
        private void OnEnable()
        {
            _objects = serializedObject.FindProperty("Objects");
            _names = serializedObject.FindProperty("ObjectNames");
        }
    
        public override void OnInspectorGUI()
        {
            var memberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            var authoring = (EntityPrefabRepoAuthoringRunner)target;
            var repoType = authoring.GetComponentType();
            var entityType = typeof(Entity);
            var field = repoType.GetFields(memberFlags);
            var changed = false;
            if (field.Length != _objects.arraySize)
            {
                _objects.arraySize = field.Length;
                _names.arraySize = field.Length;
                changed = true;
            }
            
            if (field.Length != _names.arraySize)
            {
                _objects.arraySize = field.Length;
                _names.arraySize = field.Length;
                changed = true;
            }
    
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < field.Length; i++)
            {
                var f = field[i];
                if (f.FieldType != entityType)
                {
                    Debug.LogError("it is not Entity, not allowed");
                    continue;
                }
    
                var gameObjectProperty = _objects.GetArrayElementAtIndex(i);
                var nameProperty = _names.GetArrayElementAtIndex(i);
                nameProperty.stringValue = f.Name;
                EditorGUILayout.PropertyField(gameObjectProperty, new GUIContent(f.Name));
            }
    
            if (EditorGUI.EndChangeCheck() || changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}