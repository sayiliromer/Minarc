using Unity.Entities;
using Unity.NetCode;

namespace Minarc.Common
{
    public struct ChangeVersion : IComponentData
    {
        [GhostField] public int Version;
    }

    public class Bootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 22066;
            return base.Initialize(defaultWorldName);
        }
    }
}