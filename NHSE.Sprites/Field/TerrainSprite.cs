using System;
using System.Collections.Generic;
using System.Drawing;
using NHSE.Core;
using NHSE.Sprites.Properties;

namespace NHSE.Sprites
{
    public static class TerrainSprite
    {
        public static Color GetTileColor(TerrainTile tile)
        {
            var name = tile.UnitModel.ToString();
            var baseColor = GetTileColor(name);
            if (tile.Elevation == 0)
                return baseColor;

            return ColorUtil.Blend(baseColor, Color.White, 1d / (tile.Elevation + 1));
        }

        private static Color GetTileColor(string name)
        {
            if (name.StartsWith("River")) // River
                return Color.DeepSkyBlue;
            if (name.StartsWith("Fall")) // Waterfall
                return Color.DarkBlue;
            if (name.Contains("Cliff"))
                return ColorUtil.Blend(Color.ForestGreen, Color.Black, 0.5d);
            return Color.ForestGreen;
        }

        private static readonly char[] Numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static string GetTileName(TerrainTile tile)
        {
            var name = tile.UnitModel.ToString();
            var num = name.IndexOfAny(Numbers);
            if (num < 0)
                return name;
            return name.Substring(0, num) + Environment.NewLine + name.Substring(num);
        }

        public static Image GetTileImage(TerrainTile tile)
        {
            var tileName = tile.UnitModel.ToString();
            var tileRotation = tile.TileRotation;
            var pathSpriteID = tile.PathSpriteID.ToString();
            var pathSpriteRotation = tile.PathSpriteRotation;

            if (pathSpriteID != "" && pathSpriteID != "Base") tileName = pathSpriteID;

            if (pathSpriteRotation == 0 && tileRotation > 0) pathSpriteRotation = tileRotation;

            Image? baseSprite = (Image?)Resources.ResourceManager.GetObject("Base");

            if (tileName.Substring(0, 4) == "Fall")
                baseSprite = (Image?)Resources.ResourceManager.GetObject(tileName);

            if (baseSprite == null) baseSprite = (Image?)Resources.ResourceManager.GetObject("unknown");

            if (baseSprite == null) throw new KeyNotFoundException("Resources not found in ResourceManager");

            // baseSprite rotation 1 = 90degree left

            if (tileRotation > 0 && tileName.StartsWith("Fall"))
            {
                baseSprite.RotateFlip((RotateFlipType)((4 - tileRotation) % 4));
            }

            if (tileName != "Base")
                return OverlayImages(baseSprite, GetPathSprite(tileName, pathSpriteRotation));

            return baseSprite;
        }

        private static Image? GetPathSprite(string spriteID = "", ushort rotation = 0)
        {
            if (spriteID != "")
            {
                var spritePattern = spriteID.Substring(spriteID.Length - 2);
                var spriteMaterial = spriteID.Substring(0, spriteID.Length - 2);
                var spriteMod = "";

                if (spriteMaterial == "Cliff")
                    spriteMod = "_C";

                if (spriteMaterial == "River")
                    spriteMod = "_R";

                if (rotation < 0 || rotation > 3) rotation = 0;

                Image? tileSprite = (Image?)Resources.ResourceManager.GetObject(spritePattern + spriteMod);

                if (tileSprite == null) tileSprite = (Image?)Resources.ResourceManager.GetObject(spritePattern);

                if (tileSprite == null)
                {
                    tileSprite = (Image?)Resources.ResourceManager.GetObject("unknown");
                }
                else
                {
                    tileSprite.RotateFlip((RotateFlipType)((4 - rotation) % 4));
                }

                return ApplySpriteMaterial(tileSprite, spriteMaterial);
            }
            else
            {
                Image? tileSprite = (Image?)Resources.ResourceManager.GetObject("unknown");

                return tileSprite;
            }
        }

        public static Image? ApplySpriteMaterial(Image? sprite, string material)
        {
            Image? materialImage = (Image?)Resources.ResourceManager.GetObject(material);

            if (materialImage == null) return sprite;

            // Create Bitmaps and lock them
            Bitmap materialBitmap = new Bitmap(materialImage);

            Rectangle materialBitmapRect = new Rectangle(0, 0, materialBitmap.Width, materialBitmap.Height);
            System.Drawing.Imaging.BitmapData materialBitmapData =
                materialBitmap.LockBits(materialBitmapRect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                materialBitmap.PixelFormat);

            Bitmap imageMask = new Bitmap(sprite);

            Rectangle imageMaskRect = new Rectangle(0, 0, imageMask.Width, imageMask.Height);
            System.Drawing.Imaging.BitmapData imageMaskData =
                imageMask.LockBits(imageMaskRect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                imageMask.PixelFormat);

            // Get pointers to first lines
            IntPtr materialBitmapLinePtr = materialBitmapData.Scan0;
            IntPtr imageMaskLinePtr = imageMaskData.Scan0;

            // Create arrays for all bytes of the bitmap
            int materialBitmapBytes = Math.Abs(materialBitmapData.Stride) * materialBitmap.Height;
            byte[] materialBitmapRGBA = new byte[materialBitmapBytes];
            int imageMaskBytes = Math.Abs(imageMaskData.Stride) * imageMask.Height;
            byte[] imageMaskRGBA = new byte[imageMaskBytes];

            // Copy all the RGBA value from the Bitmaps to the arrays
            System.Runtime.InteropServices.Marshal.Copy(materialBitmapLinePtr, materialBitmapRGBA, 0, materialBitmapBytes);
            System.Runtime.InteropServices.Marshal.Copy(imageMaskLinePtr, imageMaskRGBA, 0, imageMaskBytes);

            // Copy colors of all pixels that are not transparent  
            for (int y = 3; y < imageMaskRGBA.Length; y += 4)
            {
                if (imageMaskRGBA[y] == 0) continue;
                imageMaskRGBA[y - 3] = materialBitmapRGBA[y - 3];
                imageMaskRGBA[y - 2] = materialBitmapRGBA[y - 2];
                imageMaskRGBA[y - 1] = materialBitmapRGBA[y - 1];
                imageMaskRGBA[y] = materialBitmapRGBA[y];
            }

            // Copy the RGBA value back into the Bitmaps
            System.Runtime.InteropServices.Marshal.Copy(materialBitmapRGBA, 0, materialBitmapLinePtr, materialBitmapBytes);
            System.Runtime.InteropServices.Marshal.Copy(imageMaskRGBA, 0, imageMaskLinePtr, imageMaskBytes);

            // Lock the Bitmaps
            materialBitmap.UnlockBits(materialBitmapData);
            imageMask.UnlockBits(imageMaskData);

            return (Image)imageMask;
        }

        // https://stackoverflow.com/questions/38566828/overlap-one-image-as-transparent-on-another-in-c-sharp
        public static Image OverlayImages(Image? background, Image? foreground)
        {
            if (background == null || foreground == null) throw new ArgumentNullException("background image or foreground image null");
            if (background.Height != foreground.Height || background.Width != foreground.Width) throw new ArgumentException("Both images need to be the same size");
            Bitmap resultImage = new Bitmap(background);
            Graphics tempCompose = Graphics.FromImage(resultImage);

            tempCompose.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

            tempCompose.DrawImage(background, 0, 0);
            tempCompose.DrawImage(foreground, 0, 0);

            return resultImage;
        }

        public static Bitmap CreateMap(TerrainManager mgr)
        {
            var bmp = new Bitmap(mgr.MapWidth, mgr.MapHeight);
            for (int x = 0; x < mgr.MapWidth; x++)
            {
                for (int y = 0; y < mgr.MapHeight; y++)
                {
                    var tile = mgr.GetTile(x, y);
                    var color = GetTileColor(tile);
                    bmp.SetPixel(x, y, color);
                }
            }

            return bmp;
        }

        public static Bitmap CreateMap(TerrainManager mgr, int scale, int x, int y)
        {
            var img = CreateMap(mgr);
            var map = ImageUtil.ResizeImage(img, img.Width * scale, img.Height * scale);
            return DrawReticle(map, mgr, x, y, scale);
        }

        public static Bitmap CreateMap(TerrainManager mgr, int scale, int acreIndex = -1)
        {
            var img = CreateMap(mgr);
            var map = ImageUtil.ResizeImage(img, img.Width * scale, img.Height * scale);

            if (acreIndex < 0)
                return map;

            var acre = MapGrid.Acres[acreIndex];
            var x = acre.X * mgr.GridWidth;
            var y = acre.Y * mgr.GridHeight;

            return DrawReticle(map, mgr, x, y, scale);
        }

        private static Bitmap DrawReticle(Bitmap map, MapGrid mgr, int x, int y, int scale)
        {
            using var gfx = Graphics.FromImage(map);
            using var pen = new Pen(Color.Red);

            int w = mgr.GridWidth * scale;
            int h = mgr.GridHeight * scale;
            gfx.DrawRectangle(pen, x * scale, y * scale, w, h);
            return map;
        }

        public static Bitmap GetMapWithBuildings(TerrainManager mgr, IReadOnlyList<Building> buildings, ushort plazaX, ushort plazaY, Font f, int scale = 4, int index = -1)
        {
            var map = CreateMap(mgr, scale);
            using var gfx = Graphics.FromImage(map);

            gfx.DrawPlaza(mgr, plazaX, plazaY, scale);
            gfx.DrawBuildings(mgr, buildings, f, scale, index);
            return map;
        }

        private static void DrawPlaza(this Graphics gfx, MapGrid g, ushort px, ushort py, int scale)
        {
            var plaza = Brushes.RosyBrown;
            GetBuildingCoordinate(g, px, py, scale, out var x, out var y);

            var width = scale * 2 * 6;
            var height = scale * 2 * 5;

            gfx.FillRectangle(plaza, x, y, width, height);
        }

        private static void DrawBuildings(this Graphics gfx, MapGrid g, IReadOnlyList<Building> buildings, Font f, int scale, int index = -1)
        {
            var selected = Brushes.Red;
            var others = Brushes.Yellow;
            var text = Brushes.White;
            var stringFormat = new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center};

            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                if (b.BuildingType == 0)
                    continue;
                GetBuildingCoordinate(g,b.X, b.Y, scale, out var x, out var y);

                var pen = index == i ? selected : others;
                gfx.FillRectangle(pen, x - scale, y - scale, scale * 2, scale * 2);

                var name = b.BuildingType.ToString();
                gfx.DrawString(name, f, text, new PointF(x, y - (scale * 2)), stringFormat);
            }
        }

        private static void GetBuildingCoordinate(MapGrid g, ushort bx, ushort by, int scale, out int x, out int y)
        {
            // Although there is terrain in the Top Row and Left Column, no buildings can be placed there.
            // Adjust the building coordinates down-right by an acre.
            int buildingShift = g.GridWidth;
            x = (int) (((bx / 2f) - buildingShift) * scale);
            y = (int) (((by / 2f) - buildingShift) * scale);
        }
    }
}
