using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Minarc.Client
{
    public static class MeshPool
    {
        private static List<(Mesh mesh, BatchMeshID meshId)> _freeMeshes = new();
        private static List<Mesh> _allMeshes = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _freeMeshes = new List<(Mesh mesh, BatchMeshID meshId)>();
            _allMeshes = new List<Mesh>();
        }
        
        public static (Mesh mesh, BatchMeshID meshId) Get(World world)
        {
            if (_freeMeshes.Count > 0)
            {
                var result = _freeMeshes[_freeMeshes.Count - 1];
                _freeMeshes.RemoveAt(_freeMeshes.Count - 1);
                return result;
            }
            var graphicsSystem = world.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            var mesh = new Mesh();
            _allMeshes.Add(mesh);
            var meshId = graphicsSystem.RegisterMesh(mesh);
            return (mesh, meshId);
        }

        public static void Return(Mesh mesh, BatchMeshID meshID)
        {
            _freeMeshes.Add((mesh,meshID));
        }
        
        public static void Return(BatchMeshID meshID, World world)
        {
            var graphicsSystem = world.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            var mesh = graphicsSystem.GetMesh(meshID);
            _freeMeshes.Add((mesh,meshID));
        }
    }
}