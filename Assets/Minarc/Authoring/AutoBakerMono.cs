using Unity.Entities;
using UnityEngine;

namespace Minarc.Authoring
{
    [RequireComponent(typeof(AutoBakerRoot))]
    public abstract class AutoBakerMono : MonoBehaviour
    {
        public AutoBakerRoot.AutoBaker Baker { get; set; }

        public abstract void Bake();
        
        protected Entity GetEntity(TransformUsageFlags flags = TransformUsageFlags.None) => Baker.GetEntity(flags);
        protected Entity GetEntity(Component component,TransformUsageFlags flags = TransformUsageFlags.None) => Baker.GetEntity(component,flags);
        protected Entity GetEntity(GameObject authoring,TransformUsageFlags flags = TransformUsageFlags.None) => Baker.GetEntity(authoring,flags);

        protected void AddComponent<T>(Entity entity) where T : IComponentData
        {
            Baker.AddComponent<T>(entity);
        }

        protected Entity CreateAdditionalEntity(TransformUsageFlags transformUsageFlags, bool bakingOnlyEntity = false, string entityName = "")
        {
            return Baker.CreateAdditionalEntity(transformUsageFlags, bakingOnlyEntity, entityName);
        }

        
        protected DynamicBuffer<T> AddBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData
        {
            return Baker.AddBuffer<T>(entity);
        }
        
        protected void AddComponent<T>(Entity entity,T data) where T : unmanaged, IComponentData
        {
            Baker.AddComponent<T>(entity, data);
        }
        
        protected void AddComponent<T>(Entity entity, bool isEnabled) where T : struct, IComponentData, IEnableableComponent
        {
            Baker.AddComponent<T>(entity);
            Baker.SetComponentEnabled<T>(entity, isEnabled);
        }
        
        protected void SetComponentEnabled<T>(Entity entity, bool isEnabled) where T : struct, IComponentData, IEnableableComponent
        {
            Baker.SetComponentEnabled<T>(entity, enabled);
        }

        private void Reset()
        {
            var root = GetComponent<AutoBakerRoot>();
        }
    }
}