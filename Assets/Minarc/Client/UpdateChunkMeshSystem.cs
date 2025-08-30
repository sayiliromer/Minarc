using Minarc.Common;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Minarc.Client
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct UpdateChunkMeshSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            var cmd = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (chunkBuffer, materialMeshInfoRef,entity) in 
                     SystemAPI.Query<DynamicBuffer<TerrainChunkElement>,RefRW<MaterialMeshInfo>>().WithEntityAccess().WithNone<TerrainMeshCleanup>())
            {
                var (mesh,meshId) = MeshPool.Get(state.World);
                cmd.AddComponent(entity, new TerrainMeshCleanup()
                {
                    MeshID = meshId,
                });
                materialMeshInfoRef.ValueRW.MeshID = meshId;
                
            }
            cmd.Playback(state.EntityManager);
            
            
            var graphicsSystem = state.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            foreach (var (chunkBuffer,meshVersionRef,versionRef, materialMeshInfoRef) in 
                     SystemAPI.Query<DynamicBuffer<TerrainChunkElement>,RefRW<TerrainChunkMeshVersion>, RefRO<ChangeVersion>, RefRO<MaterialMeshInfo>>())
            {
                if(meshVersionRef.ValueRO.CheckedVersion == versionRef.ValueRO.Version) continue;
                meshVersionRef.ValueRW.CheckedVersion = versionRef.ValueRO.Version;
                var mesh = graphicsSystem.GetMesh(materialMeshInfoRef.ValueRO.MeshID);
                mesh.vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f),
                    new Vector3(-0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f),
                    new Vector3(0.5f, -0.5f),
                };
                mesh.SetIndices(new int[]
                {
                    0, 1, 2,
                    2,3, 0
                }, MeshTopology.Triangles, 0);
            }
            
            
            cmd = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            foreach (var (meshCleanupRef,entity) in 
                     SystemAPI.Query<RefRO<TerrainMeshCleanup>>().WithEntityAccess().WithNone<TerrainChunkElement>())
            {
                cmd.RemoveComponent<TerrainMeshCleanup>(entity);
                MeshPool.Return(meshCleanupRef.ValueRO.MeshID,state.World);
            }
            cmd.Playback(state.EntityManager);
        }
    }
}