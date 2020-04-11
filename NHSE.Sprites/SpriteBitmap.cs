using System;
using System.Drawing;

namespace NHSE.Sprites
{
    class SpriteBitmap
    {
        public SpriteBitmap(Image image)
        {
            Bitmap materialBitmap = new Bitmap(image);

            Rectangle materialBitmapRect = new Rectangle(0, 0, materialBitmap.Width, materialBitmap.Height);
            System.Drawing.Imaging.BitmapData materialBitmapData =
                materialBitmap.LockBits(materialBitmapRect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                materialBitmap.PixelFormat);

            IntPtr materialBitmapLinePtr = materialBitmapData.Scan0;

            int materialBitmapBytes = Math.Abs(materialBitmapData.Stride) * materialBitmap.Height;
            byte[] materialBitmapRGBA = new byte[materialBitmapBytes];

            System.Runtime.InteropServices.Marshal.Copy(materialBitmapLinePtr, materialBitmapRGBA, 0, materialBitmapBytes);

            System.Runtime.InteropServices.Marshal.Copy(materialBitmapRGBA, 0, materialBitmapLinePtr, materialBitmapBytes);

            materialBitmap.UnlockBits(materialBitmapData);
        }
    }
}
