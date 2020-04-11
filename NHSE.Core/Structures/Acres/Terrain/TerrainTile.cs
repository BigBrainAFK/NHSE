using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace NHSE.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public class TerrainTile
    {
        public const int SIZE = 0xE;
        private const string Details = nameof(Details);

        [Category(Details), Description("Terrain model to be loaded for this tile.")]
        public TerrainUnitModel UnitModel { get; set; }

        public ushort Unk2 { get; set; }
        public ushort TileRotation { get; set; }
        public TerrainUnitModel PathSpriteID { get; set; }
        public ushort Unk8 { get; set; }
        public ushort PathSpriteRotation { get; set; }

        [Category(Details), Description("How high the terrain tile is elevated.")]
        public ushort Elevation { get; set; }

        public static TerrainTile[] GetArray(byte[] data) => data.GetArray<TerrainTile>(SIZE);
        public static byte[] SetArray(IReadOnlyList<TerrainTile> data) => data.SetArray(SIZE);

        public void Clear()
        {
            UnitModel = PathSpriteID = 0;
            Unk2 = TileRotation = Unk8 = PathSpriteRotation = Elevation = 0;
        }

        public void CopyFrom(TerrainTile source)
        {
            UnitModel = source.UnitModel;
            Unk2 = source.Unk2;
            TileRotation = source.TileRotation;
            PathSpriteID = source.PathSpriteID;
            Unk8 = source.Unk8;
            PathSpriteRotation = source.PathSpriteRotation;
            Elevation = source.Elevation;
        }
    }
}
