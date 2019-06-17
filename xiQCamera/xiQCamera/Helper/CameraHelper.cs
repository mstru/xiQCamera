using System;
using System.Runtime.Serialization;

namespace xiQCamera.Helper
{

    /* Trieda umoznuje vytvaranie datovej polozky a jej postupnu aktualizaciu a uchovavanie kazdej timestamp*/
    [DataContract]
    public class CameraHelper : Command
    {
        [DataMember] private DateTime _timestamp;
        [DataMember] public short _nOfcameras;
        [DataMember] public int ImagesTaken;
        [DataMember] public int[] PerformanceTime = new int[6];
        [DataMember] public float GetparamGain;
        [DataMember] public int GetparamExposure;

        [DataMember] public short Cameras { get { return _nOfcameras; } set { _nOfcameras = value; _timestamp = DateTime.Now; } }

        public CameraHelper(bool imaging, int exposure, double fps)
        {
            Active = imaging;
            Exposure = exposure;
            FPS = fps;
        }

        public CameraHelper()
        {
        }

        [DataMember]
        public DateTime Timestap
        {
            get { return _timestamp; }
            set { _timestamp = DateTime.Now; }
        }
    }
}
