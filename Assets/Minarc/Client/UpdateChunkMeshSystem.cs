using System;
using System.Collections.Generic;
using Minarc.Common;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using static Minarc.Common.Constants;

namespace Minarc.Client
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct UpdateChunkMeshSystem : ISystem
    {
        private NativeHashMap<int2,DynamicBuffer<TileMapChunkElement>> _chunks;
        
        public void OnCreate(ref SystemState state)
        {
            _chunks = new NativeHashMap<int2, DynamicBuffer<TileMapChunkElement>>(16, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            _chunks.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            if(!SystemAPI.TryGetSingletonBuffer(out DynamicBuffer<TileSpriteElement> tileSprites)) return;
            if(!SystemAPI.TryGetSingletonBuffer(out DynamicBuffer<TileSpriteSetElement> tileSpriteSets)) return;
            if(!SystemAPI.TryGetSingleton(out BrushCollection brushCollection)) return;
            _chunks.Clear();
            var cmd = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (chunkBuffer, materialMeshInfoRef, entity) in
                     SystemAPI.Query<DynamicBuffer<TileMapChunkElement>, RefRW<MaterialMeshInfo>>().WithEntityAccess().WithNone<TileMapChunkMeshCleanup>())
            {
                var (mesh, meshId) = MeshPool.Get(state.World);
                cmd.AddComponent(entity, new TileMapChunkMeshCleanup
                {
                    MeshID = meshId
                });
                materialMeshInfoRef.ValueRW.MeshID = meshId;
            }

            cmd.Playback(state.EntityManager);
            
            _chunks.Clear();
            foreach (var (chunkBuffer, transformRef, entity) in
                     SystemAPI.Query<DynamicBuffer<TileMapChunkElement>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                var chunkIndex = transformRef.ValueRO.Position.ToChunkIndex();
                _chunks.Add(chunkIndex, chunkBuffer);
            }

            //Neighboring indexes
            Span<int2> nIndex = stackalloc int2[8];
            nIndex[0] = new int2(0, 1);
            nIndex[1] = new int2(1, 1);
            nIndex[2] = new int2(1, 0);
            nIndex[3] = new int2(1, -1);
            nIndex[4] = new int2(0, -1);
            nIndex[5] = new int2(-1, -1);
            nIndex[6] = new int2(-1, 0);
            nIndex[7] = new int2(-1, 1);
            
            var graphicsSystem = state.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            foreach (var (chunkBuffer, meshVersionRef, versionRef, materialMeshInfoRef, transformRef, rende) in
                     SystemAPI.Query<DynamicBuffer<TileMapChunkElement>,
                         RefRW<TileMapChunkMeshVersion>,
                         RefRO<ChangeVersion>,
                         RefRO<MaterialMeshInfo>,
                         RefRO<LocalTransform>, RefRW<RenderBounds>>())
            {
                if (meshVersionRef.ValueRO.CheckedVersion == versionRef.ValueRO.Version) continue;
                
                meshVersionRef.ValueRW.CheckedVersion = versionRef.ValueRO.Version;
                var mesh = graphicsSystem.GetMesh(materialMeshInfoRef.ValueRO.MeshID);
                var chunkIndex = transformRef.ValueRO.Position.ToChunkIndex();
                var vertices = new List<Vector3>();
                var indices = new List<int>();
                var uv = new List<Vector2>();
                for (int x = 0; x < ChunkSize; x++)
                {
                    for (int y = 0; y < ChunkSize; y++)
                    {
                        var tile = chunkBuffer[x + y * ChunkSize];
                        if(tile.MaterialType == TileMaterialType.None) continue; // Empty tile
                        
                        var vertIndex = vertices.Count;
                        vertices.Add(new Vector3(x,y));
                        vertices.Add(new Vector3(x,y + 1));
                        vertices.Add(new Vector3(x + 1,y + 1));
                        vertices.Add(new Vector3(x + 1,y));
                        
                        indices.Add(vertIndex + 0);
                        indices.Add(vertIndex + 1);
                        indices.Add(vertIndex + 2);
                        indices.Add(vertIndex + 2);
                        indices.Add(vertIndex + 3);
                        indices.Add(vertIndex + 0);

                        int flag = 0;
                        for (int i = 0; i < nIndex.Length; i++)
                        {
                            var n = nIndex[i] + new int2(x,y);
                            if(n.x < 0 || n.x >= ChunkSize) continue;
                            if(n.y < 0 || n.y >= ChunkSize) continue;
                            var nbh = chunkBuffer[n.x + n.y * ChunkSize];
                            if (nbh.MaterialType == TileMaterialType.None) continue;
                            flag |= 1 << i;
                        }
                        var tileIndex = brushCollection.NeighborFlagToTile.Value.FlagToIndex[flag];

                        ref var r = ref brushCollection.Brushes.Value.Brushes[0].Rules[tileIndex.CanonicalTileIndex];
                        var sp = r.Sprites[0];
                        uv.Add(sp.Uv0);
                        uv.Add(sp.Uv1);
                        uv.Add(sp.Uv2);
                        uv.Add(sp.Uv3);
                        // byte caseIndex = 0;
                        // for (int i = 0; i < nIndex.Length; i++)
                        // {
                        //    var neighbourChunkIndex = nIndex[i] + chunkIndex; 
                        // }
                    }
                }
                mesh.vertices = vertices.ToArray();
                mesh.triangles = indices.ToArray();
                mesh.uv = uv.ToArray();
                mesh.RecalculateBounds();
                rende.ValueRW.Value = new AABB()
                {
                    Center = mesh.bounds.center,
                    Extents = mesh.bounds.extents,
                };
                // mesh.vertices = new[]
                // {
                //     new Vector3(-0.5f, -0.5f),
                //     new Vector3(-0.5f, 0.5f),
                //     new Vector3(0.5f, 0.5f),
                //     new Vector3(0.5f, -0.5f)
                // };
                // mesh.SetIndices(new[]
                // {
                //     0, 1, 2,
                //     2, 3, 0
                // }, MeshTopology.Triangles, 0);
            }


            cmd = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach (var (meshCleanupRef, entity) in
                     SystemAPI.Query<RefRO<TileMapChunkMeshCleanup>>().WithEntityAccess().WithNone<TileMapChunkElement>())
            {
                cmd.RemoveComponent<TileMapChunkMeshCleanup>(entity);
                MeshPool.Return(meshCleanupRef.ValueRO.MeshID, state.World);
            }

            cmd.Playback(state.EntityManager);
        }
    }
}