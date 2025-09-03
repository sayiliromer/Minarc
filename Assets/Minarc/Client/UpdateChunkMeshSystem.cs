using System;
using System.Collections.Generic;
using Drawing;
using Minarc.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
        private NativeParallelHashMap<int2,Entity> _chunks;
        private NativeList<Entity> _updateMeshChunks;
        private FixedList128Bytes<int2> _neighbors;
        private NativeHashSet<Entity> _updateMeshChunksSet;
        
        public void OnCreate(ref SystemState state)
        {
            _updateMeshChunks = new NativeList<Entity>(Allocator.Persistent);
            _chunks = new NativeParallelHashMap<int2, Entity>(100000, Allocator.Persistent);
            _updateMeshChunksSet = new NativeHashSet<Entity>(200,Allocator.Persistent);
            _neighbors.Add(new int2(0, 1));
            _neighbors.Add(new int2(1, 1));
            _neighbors.Add(new int2(1, 0));
            _neighbors.Add(new int2(1, -1));
            _neighbors.Add(new int2(0, -1));
            _neighbors.Add(new int2(-1, -1));
            _neighbors.Add(new int2(-1, 0));
            _neighbors.Add(new int2(-1, 1));
        }

        public void OnDestroy(ref SystemState state)
        {
            _updateMeshChunksSet.Dispose();
            _updateMeshChunks.Dispose();
            _chunks.Dispose();
        }
        
        [BurstCompile]
        [WithAll(typeof(TileMapChunkElement))]
        private partial struct UpdateChunkMapJob : IJobEntity
        {
            public NativeParallelHashMap<int2, Entity>.ParallelWriter Chunks;

            private void Execute(in Entity entity, in LocalTransform transform)
            {
                var chunkIndex = transform.Position.ToChunkIndex();
                Chunks.TryAdd(chunkIndex, entity);
            }
        }
        
        [BurstCompile]
        private partial struct FindChangedChunksJob : IJobEntity
        {
            public bool Force;
            public NativeParallelHashMap<int2, Entity> Chunks;
            public NativeList<Entity> Entities;
            public FixedList128Bytes<int2> Neighbors;
            
            private void Execute(in Entity entity,ref TileMapChunkMeshVersion meshVersion, in ChangeVersion changeVersion, in LocalTransform transform)
            {
                if(meshVersion.CheckedVersion == changeVersion.Version && !Force) return;
                meshVersion.CheckedVersion = changeVersion.Version;
                var chunkIndex = transform.Position.ToChunkIndex();
                Entities.Add(entity);
                for (int i = 0; i < Neighbors.Length; i++)
                {
                    if(!Chunks.TryGetValue(chunkIndex + Neighbors[i], out var chunk)) continue;
                    Entities.Add(chunk);
                }
            }
        }
        
        public void OnUpdate(ref SystemState state)
        {
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
            _updateMeshChunks.Clear();
            new UpdateChunkMapJob()
            {
                Chunks = _chunks.AsParallelWriter(),
            }.ScheduleParallel();
            new FindChangedChunksJob()
            {
                Force = Input.GetKey(KeyCode.R),
                Chunks = _chunks,
                Neighbors = _neighbors,
                Entities = _updateMeshChunks,
            }.Schedule();
            state.CompleteDependency();

            var chunkLookup = SystemAPI.GetBufferLookup<TileMapChunkElement>();
            var materialMeshInfoLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>();
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            var renderBoundsLookup = SystemAPI.GetComponentLookup<RenderBounds>();
            var graphicsSystem = state.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            var chunks = _chunks;
            _updateMeshChunksSet.Clear();
            for (int i = 0; i < _updateMeshChunks.Length; i++)
            {
                var updateEntity = _updateMeshChunks[i];
                if(!_updateMeshChunksSet.Add(updateEntity)) continue;
                var chunkBuffer = chunkLookup[updateEntity];
                var localTransform = transformLookup[updateEntity];
                var materialMeshInfo = materialMeshInfoLookup[updateEntity];
                var renderBoundsRef = renderBoundsLookup.GetRefRW(updateEntity);
                var mesh = graphicsSystem.GetMesh(materialMeshInfo.MeshID);
                var chunkIndex = localTransform.Position.ToChunkIndex();
                var vertices = new List<Vector3>();
                var indices = new List<int>();
                var uv = new List<Vector2>();
                for (int x = 0; x < ChunkSize; x++)
                {
                    for (int y = 0; y < ChunkSize; y++)
                    {
                        var tile = chunkBuffer[x + y * ChunkSize];
                        if(tile.MaterialType == TileMaterialType.None) continue; // Empty tile
                        TileMapChunkElement? GetTileElement(int2 nDir)
                        {
                            var tileIndex = nDir + new int2(x,y);
                            if (tileIndex.x < 0 || tileIndex.x >= ChunkSize || tileIndex.y < 0 || tileIndex.y >= ChunkSize)
                            {
                                var nChunkIndex = chunkIndex;
                                if (tileIndex.x < 0)
                                {
                                    nChunkIndex.x += nDir.x;
                                }
                                else if (tileIndex.x >= ChunkSize)
                                {
                                    nChunkIndex.x += nDir.x;
                                }

                                if (tileIndex.y < 0)
                                {
                                    nChunkIndex.y += nDir.y;
                                }
                                else if (tileIndex.y >= ChunkSize)
                                {
                                    nChunkIndex.y += nDir.y;
                                }
                                
                                var ws = chunkIndex * 10 + new int2(x,y);
                                tileIndex = new int2(
                                    (tileIndex.x + ChunkSize) % ChunkSize,
                                    (tileIndex.y + ChunkSize) % ChunkSize
                                );
                                if (!chunks.TryGetValue(nChunkIndex, out var chunkEntity))
                                {
                                    return null;
                                }
                                var nChunkBuffer = chunkLookup[chunkEntity];
                                var nTile = nChunkBuffer[tileIndex.x + tileIndex.y * ChunkSize];
                                return nTile;
                            }
                            return chunkBuffer[tileIndex.x + tileIndex.y * ChunkSize];
                        }
                        int flag = 0;
                        for (int nIndex = 0; nIndex < _neighbors.Length; nIndex++)
                        {
                            var nDir = _neighbors[nIndex];
                            var nTileElement = GetTileElement(nDir);
                            if (nTileElement == null) continue;
                            if (nTileElement.Value.MaterialType == TileMaterialType.None) continue;
                            flag |= 1 << nIndex;
                        }
                        var tileIndex = brushCollection.NeighborFlagToTile.Value.FlagToIndex[flag];
                        ref var ruleTileData = ref brushCollection.Brushes.Value.Brushes[0].Rules[tileIndex.CanonicalTileIndex];
                        var spriteData = ruleTileData.Sprites[0];
                        uv.Add(spriteData.Uv0);
                        uv.Add(spriteData.Uv1);
                        uv.Add(spriteData.Uv2);
                        uv.Add(spriteData.Uv3);
                        
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
                    }
                }
                mesh.vertices = vertices.ToArray();
                mesh.triangles = indices.ToArray();
                mesh.uv = uv.ToArray();
                mesh.RecalculateBounds();
                renderBoundsRef.ValueRW.Value = new AABB()
                {
                    Center = mesh.bounds.center,
                    Extents = mesh.bounds.extents,
                };
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