using Unity.Entities;
using UnityEngine;

namespace Minarc.Authoring
{
    public sealed class AutoBakerRoot : MonoBehaviour
    {
        public class AutoBaker : Baker<AutoBakerRoot>
        {
            public override void Bake(AutoBakerRoot authoring)
            {
                var allComponents = authoring.GetComponents<AutoBakerMono>();
                DependsOn(authoring.gameObject);
                for (int i = 0; i < allComponents.Length; i++)
                {
                    var component = allComponents[i];
                    DependsOn(component);
                    component.Baker = this;
                    component.Bake();
                }
            }
        }
    }
}