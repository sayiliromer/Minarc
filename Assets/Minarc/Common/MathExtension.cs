using Unity.Mathematics;
using static Minarc.Common.Constants;

namespace Minarc.Common
{
    public static class MathExtension
    {
        public static int2 Snap(this float2 vec, int snapValue)
        {
            return (int2)math.floor(vec / snapValue) * snapValue;
        }

        public static int2 ToChunkIndex(this float2 position)
        {
            return (int2)math.floor(position / ChunkSize);
        }
        
        public static int2 ToChunkIndex(this float3 position)
        {
            return (int2)math.floor(position.xy / ChunkSize);
        }
    }
}