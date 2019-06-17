using System;
using System.Diagnostics;
using System.Drawing;
using xiQCamera.CameraAPI;
using xiQCamera.Helper;

namespace xiQCamera
{
    /* Vseobecna trieda, ktora pracuje z kamerou pomocou rozhrania xiAPI.
        * 
        * Predpoklada sa, ze staticke triedy nie su overrriden
        * 
        * Kazda zdedena trieda od kamery by mala poskytnut implementaciu pre instancne metody
        * 
        * */
    abstract class CameraBase : IDisposable
    {

        /*xiAPI Parameters*/
        protected volatile int _width;
        protected volatile int _height;
        protected volatile float _gammaY = 1;
        protected volatile float _gammaC = 1;
        protected volatile int _exposure;
        protected volatile int _bitdepth = 10;
        protected volatile float _gain;
        protected ushort[][] _image; // konecny storage images, ktory sa pouziva na vratenie hodnot

        public volatile int Xoffset = 0;
        public volatile int Yoffset = 0;
        public volatile float expCorrection = 1;
        public volatile float digitCorrection = 1;

        protected float MAXGAIN;
        protected float MAXWidth;
        protected float MAXHeight;
        protected string SerialNumber;

        protected CameraType cameraType;

        public enum CameraType
        {
            XimeaCamera,
            Unknown
        }

        protected volatile Trigger nowTrigger = Trigger.Software;

        public enum Trigger
        {
            Software
        }

        /* Progresivne stavy cinnosti kamery, vzajomne sa vylucuju
             *  (Closed) Uzavreta kamera znamena, ze nie je inicializovana 
             *  (OpenIdle) Otvorena kamera znamena, ze je aktivna program ju drzi, ale nic sa nedeje
             *  (OpenUpdating) Aktualizacia kamery znamena, ze je v procese zmeny nastaveni a proces by nemal byt preruseny
             *  (AquiIdle) Akvizicia znamena, ze kamera je v rezime Aquisition a pripravena na snimanie 
             *  (AquiExposure) Expozicia znamena, ze kamera bola spustena a integracia zacala a proces by nemal byt preruseny
             *  (AquiReading) Akvizicia citanie znamena, ze udaje sa citaju a kopiruju z fotoaparatu a proces by sa nemal prerusovat            
             * */
        public enum State
        {
            Closed,
            OpenIdle,
            OpenUpdating,
            AquiIdle,
            AquiExposure,
            AquiReading
        }

        //16 bitovy obraz vo formate 2D pola
        public ushort[][] TakeImageUshort2D(Stopwatch stopwatch)
        {
            Bitmap bitmap = TakeImage();

            //Aquisition performance time
            Logic.CameraHelper.PerformanceTime[1] = (int)stopwatch.ElapsedMilliseconds;

            ushort[][] data = Conversions.BitmapToUShort2DArray(bitmap, _bitdepth, (float)digitCorrection);

            //Write performance time
            Logic.CameraHelper.PerformanceTime[2] = (int)stopwatch.ElapsedMilliseconds - Logic.CameraHelper.PerformanceTime[1];

            return data;

        }

        public static CameraBase[] InitAllCameras()
        {
            CameraBase[] allCameras = new CameraBase[GetNOfAllCameras()];

            int index = 0;
            XimeaCamera[] Xcameras = XimeaCamera.InitAllXimeaCameras();
            Xcameras.CopyTo(allCameras, index);
            index = Xcameras.Length;

            return allCameras;
        }

        /* Aktulany stav kamery  
         * 
         * */
        protected volatile State nowState;

        /* Staticke metody, pouzitelne pre kameru
         * Pocet vsetkych Ximea kamier
         * */
        public static int GetNOfAllCameras()
        {        
            int numberOfCameras = 0;
            numberOfCameras += XimeaCamera.GetNOfXimeaCameras();
            return numberOfCameras;
        }


        /*getters a setters spolocne pre vsetky zdedene triedy*/

        /// <summary>
        /// Stav
        /// </summary>
        /// <returns>Vrati aktualny stav</returns>
        public State GetCurrentState()
        {
            return nowState;
        }

        public float GetGammaY()
        {
            return _gammaY;
        }

        public float GetGammaC()
        {
            return _gammaC;
        }


        /* Metody, ktore musia byt prepisane aby boli implementovane, inak sa program neskompiluje
         *         
         */
        //Funkcia, ktora aktulizuje nastavenia kamery, pre tento krok musi byt zastavena
        abstract public int UpdateSettings(CameraHelper cam, int exposure, float gainDb = 0, float gammaY = 1, float gammaC = 1);

        //Tato funkcia aktualizuje nastavenia fotoaparatu bez prerusenia akvizicie
        abstract public int DirectlyUpdateSettings(int exposureUs, float gainDb = 0);

        //Nastavenie premennychv triedach
        abstract protected int InitaliseParamCamera();

        //Otvorenie kamery pomocou indexu
        abstract public int Open(int i);

        //Otvorenie kamery pomocou serioveho cisla
        abstract public int Open(string serial);

        abstract public int StartImageAcquisition();

        abstract public int StopImageAcquisition();

        protected abstract Bitmap TakeImage();

        /* Mala by sa uvolnit kamera, ak je uz otvorena, vrati 1 pri uspechu, 0 ak je uz zatvorená, vtedy vrati -1, vtedy sa vyskytla chyba*/
        abstract protected int Close();

        public void Dispose()
        {
            Close();
        }
    }
}
