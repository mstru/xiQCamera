using BitMiracle.LibTiff.Classic;
using System;

namespace xiQCamera
{
    class SavingImage
    {
        public static int Write(ushort[][] imageData, String path, int width, int height, int bitdepth)
        {
            int toreturn = 0;

            using (Tiff tif = Tiff.Open(path, "w"))
            {

                if (tif == null)
                {
                    toreturn = 0;
                }
                else
                {
                    tif.SetField(TiffTag.IMAGEWIDTH, width);
                    tif.SetField(TiffTag.IMAGELENGTH, height);
                    tif.SetField(TiffTag.BITSPERSAMPLE, bitdepth);
                    tif.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                    tif.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                    tif.SetField(TiffTag.ROWSPERSTRIP, height);
                    tif.SetField(TiffTag.XRESOLUTION, 88.0);
                    tif.SetField(TiffTag.YRESOLUTION, 88.0);
                    tif.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    tif.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    tif.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    tif.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                    int bytespersample = (bitdepth / 8);


                    int i = 0;
                    if (bytespersample == 2)
                    {
                        foreach (ushort[] stripe in imageData)
                        {
                            byte[] buffer = new byte[width * bytespersample];

                            Buffer.BlockCopy(stripe, 0, buffer, 0, stripe.Length * sizeof(short));
                            tif.WriteScanline(buffer, i);
                            i++;
                        }
                    }
                    else if (bytespersample == 1)
                    {
                        foreach (ushort[] stripe in imageData)
                        {
                            byte[] buffer = new byte[width * bytespersample]; 

                            int n = 0;

                            foreach (ushort Pixel in stripe)
                            {
                                buffer[n] = BitConverter.GetBytes(Pixel)[1];
                                n++;
                            }

                            tif.WriteScanline(buffer, i);
                            i++;
                        }
                    }
                    else
                    {
                    }
                    tif.FlushData();
                    tif.Close();
                    toreturn = 1;
                }
            }

            return toreturn;
        }       
    }
}
