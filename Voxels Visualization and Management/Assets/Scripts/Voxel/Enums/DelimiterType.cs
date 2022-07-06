using System.ComponentModel;

namespace VoxelSystem
{
    public enum DelimiterType
    {
        [Description(" ")]
        space,
        [Description(",")]
        comma,
        [Description(";")]
        semicolon
    }
}
