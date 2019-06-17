using System.Runtime.Serialization;

namespace xiQCamera.Helper
{
    [DataContract]
    public class Command
    {
        /*ak je kamera v stave zobrazovania, true / false*/
        [DataMember] public volatile bool Active;
        /*Expozicia v milisekundach*/
        [DataMember] public int Exposure;
        [DataMember] public double FPS;
        [DataMember] public double Gain;
        [DataMember] public double GammaY = 1;
        [DataMember] public double GammaC = 1;
        [DataMember] public BitDepth curBitDepth;
        [DataMember] public ImageFormat CurImageFormat;
        [DataMember] public ImageFileFormat CurImageFileFormat;
        [DataMember] public string Foldername;
        [DataMember] public string LogChanges = "";
        [DataMember] public int[] Xshift = new int[4]; //Nepodporene
        [DataMember] public int[] Yshift = new int[4]; //Nepodporene
        [DataMember] public double[] ExpCorrection = new double[4]; //Nepodporene
        [DataMember] public double[] DigitCorrection = new double[4]; //Nepodporene
        [DataMember] public bool IsReseting = false;
        [DataMember] public bool Shutdown = false;
        [DataMember] public static string _SerialNumberCamera;


        [DataMember]
        public string CamerasSerialN { get { return _SerialNumberCamera; } set { _SerialNumberCamera = value;  } }

        public enum ImageFileFormat { Tiff, Jpeg, Jpg, Png, Bmp}
        public enum BitDepth { Mono8bpp, Mono16Bpp };
        public enum ImageFormat { Seperate, Joined, Three };

        public Command(bool active, int exposure, double fps, double gain = 0, BitDepth curBitDepth = BitDepth.Mono8bpp, ImageFormat imageformat = ImageFormat.Joined, ImageFileFormat imageFileFormat = ImageFileFormat.Tiff, string foldername = "")
        {
            Active = active;
            Exposure = exposure;
            FPS = fps;
            Gain = gain;
            curBitDepth = curBitDepth;
            CurImageFormat = imageformat;
            CurImageFileFormat = imageFileFormat;
            Foldername = foldername;

        }

        protected Command()
        {
        }
    }
}
