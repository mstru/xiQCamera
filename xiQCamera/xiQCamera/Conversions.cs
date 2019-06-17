using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace xiQCamera
{
    //Konverzia datove typu Bitmap
    public static class Conversions
    {  
        public static ushort[][] BitmapToUShort2DArray(Bitmap image, int truebitPerPixel, float correction)
        {

            UInt16[][] destination = null;
            if (image == null)
            {
            }
            else if ((truebitPerPixel <= 16) && (truebitPerPixel >= 8))
            {
                destination = new UInt16[image.Height][];
                for (int i = 0; i < destination.Length; i++)
                {
                    destination[i] = new ushort[image.Width];
                }

                ushort shift = (ushort)(16 - truebitPerPixel);
   
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);
         
                IntPtr ptr = bmpData.Scan0;

                // Deklaruje pole, ktoré bude obsahovať bitmapy. Vrátená hodnota je v bajtoch a my ukladáme ushort, takže musíme rozdeliť 2
                int length = (Math.Abs(bmpData.Stride) * image.Height) / 2;

                if (image.PixelFormat == PixelFormat.Format16bppGrayScale)
                {
                    unsafe
                    {
                        var sourcePtr = (ushort*)ptr;
                        for (int i = 0; i < destination.Length; i++)
                        {
                            for (int n = 0; n < destination[i].Length; n++)
                            {
                                destination[i][n] = (ushort)((*sourcePtr << shift) * correction);
                                sourcePtr++;
                            }
                        }
                    }
                }
                else
                {
                }
            }
            else
            {
                
            }

            return destination;
        }
    }
}
