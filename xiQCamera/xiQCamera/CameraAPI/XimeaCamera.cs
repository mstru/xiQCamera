using System;
using xiApi.NET;
using System.Drawing;
using xiQCamera.Helper;

namespace xiQCamera.CameraAPI
{
    class XimeaCamera : CameraBase
    {
        private static readonly xiCam detector = new xiCam();
        private xiCam Camera = new xiCam();

        /* Konstruktor
         */
        public XimeaCamera(int i)
        {
            cameraType = CameraType.XimeaCamera;
            nowState = State.Closed;

            Open(i);
        }

        public XimeaCamera(string serial)
        {
            cameraType = CameraType.XimeaCamera;
            nowState = State.Closed;
            Open(serial);
        }


        /* Staticke public metody
         *       
         *  Vracia pocet vsetkych najdenych zariadeni.       
         */
        public static int GetNOfXimeaCameras()
        {
            int NofCameras;
            detector.GetNumberDevices(out NofCameras);
            return NofCameras;
        }

        /*   
         *  Vracia serial number najdenych zariadeni.       
         */
        public static string GetSNXimeaCameras()
        {
            string SerialNumber;
            detector.GetParam(PRM.DEVICE_SN, out SerialNumber);
            return SerialNumber;
        }

        public static XimeaCamera[] InitAllXimeaCameras()
        {
            int NumbersOfXimeaCameras = GetNOfXimeaCameras();
            XimeaCamera[] allCameras = new XimeaCamera[NumbersOfXimeaCameras];

            try
            {
                for (int i = 0; i < NumbersOfXimeaCameras; i++)
                {
                    allCameras[i] = new XimeaCamera(i);
                }
            }
            catch (Exception exception)
            {
                //Logging
            }
            return allCameras;
        }

        /* Modifikacia nastavenia kamery: (0) - vrati ak je kamera v zlom stave inom ako Open, (-1) - vrati v pripade vynimky, (1) - vrati v pripade uspechu
             * exposure - nastavuje pocet expozicii v jednom ramci (frame), Toto nastavenie je platne len, vtedy ak je trigger selector nastaveny na ExposureActive  alebo ExposureStart
             * gainDb - riadenie zosilnenia signalu kamery (vyuzitie bitovej hlbky kamery pri slabom osvetleni) pouzitie po optimalizacii nastavenie casu expozicie
             * gammaY - (luminiosity - svietivost) nastavenie stupne sedej reprodukcie na zabere, gamma = 1, gamma > 1
             * gammaC - (Chromaticity gamma - chromatickost)
             */
        public override int UpdateSettings(CameraHelper cam, int exposure, float gainDb = 0, float gammaY = 1, float gammaC = 1)
        {
            int toreturn;
            if (nowState == State.OpenIdle)
            {
                try
                {
                    //
                    _exposure = (int)(exposure * expCorrection + 0.5);
                    Camera.SetParam(PRM.EXPOSURE, _exposure);

                    Camera.SetParam(PRM.GAIN, gainDb);
                    _gain = gainDb;

                    Camera.SetParam(PRM.GAMMAY, gammaY);
                    _gammaY = gammaY;
                    Camera.SetParam(PRM.GAMMAC, gammaC);
                    _gammaC = gammaC;
                    toreturn = 1;
                    nowState = State.OpenIdle;

                    //When parameter is set by xiSetParam the API checks the range. If it is within the range, it tries to find the closest settable value and set it.
                    Camera.GetParam(PRM.EXPOSURE, out cam.GetparamExposure);              
                    Camera.GetParam(PRM.GAIN, out cam.GetparamGain);

                }
                catch (Exception exception)
                {
                    toreturn = -1;
                }
            }
            else
            {
                toreturn = 0;
            }
            return toreturn;
        }


        /* (0) - vrati ak je kamera v zlom stave inom ako Open, (-1) - vrati v pripade vynimky, (1) - vrati v pripade uspechu*/
        public override int DirectlyUpdateSettings(int exposureUs, float gainDb = 0)
        {
            int toreturn = 0;
            if (nowState == State.OpenIdle)
            {
                try
                {
                    Camera.SetParam(PRM.EXPOSURE_DIRECT_UPDATE, (int)(exposureUs * expCorrection + 0.5));
                    _exposure = exposureUs;
                    Camera.SetParam(PRM.GAIN_DIRECT_UPDATE, gainDb);
                    _gain = gainDb;
                }
                catch (Exception exception)
                {
                    toreturn = -1;                   
                }
            }
            else
            {
                toreturn = 0;
            }

            return toreturn;
        }


        /*Initializacia kamery: (0) - vrati ak kamera nie je v stave Open, (-1) - vrati v pripade vynimky, (1) - vrati v pripade uspechu*/
        protected override int InitaliseParamCamera()
        {
            int toreturn;
            if (nowState != State.OpenIdle)
            {                
                toreturn = 0;
            }
            else
            {
                nowState = State.OpenUpdating;
                try
                {
                    /*get static camera parameters */
                    Camera.GetParam(PRM.DEVICE_SN, out SerialNumber);     
                    //Resolution
                    // image width must be divisible by 4
                    Camera.GetParam(PRM.WIDTH, out _width);
                    _width = _width - (_width % 4);
                    Camera.SetParam(PRM.WIDTH, _width);
                    Camera.GetParam(PRM.HEIGHT, out _height);
                    _height = _height - (_height % 4);
                    Camera.SetParam(PRM.HEIGHT, _height);

                    Camera.SetParam(PRM.IMAGE_DATA_FORMAT, IMG_FORMAT.RAW16);
                    Camera.SetParam(PRM.OUTPUT_DATA_BIT_DEPTH, _bitdepth);

                    //control
                    Camera.SetParam(PRM.BUFFER_POLICY, BUFF_POLICY.UNSAFE);
                    Camera.SetParam(PRM.TRG_SOURCE, TRG_SOURCE.SOFTWARE);
                    nowTrigger = Trigger.Software;
                    toreturn = 1;


                    nowState = State.OpenIdle;

                    CameraHelper._SerialNumberCamera = SerialNumber;
                }
                catch (Exception exception)
                {
                    toreturn = -1;                   
                }
            }
            return 0;
        }

        /*Otvorenie kamery pomocou indexu: (0) - vrati ak kamera nie je v stave Closed, (-1) - vrati v pripade vynimky, (1) - vrati v pripade uspechu*/
        public override int Open(int i)
        {
            int toreturn;

            if (nowState == State.Closed)
            {
                try
                {
                    Camera.OpenDevice(i);
                    nowState = State.OpenIdle;
                    InitaliseParamCamera();
                    toreturn = 1;
                }
                catch (Exception exception)
                {
                    toreturn = -1;
                }
            }
            else
            {
                toreturn = 0;
            }

            return toreturn;
        }

        /*Otvorenie kamery pomocou serioveho cisla: (0) - vrati ak kamera nie je v stave Closed, (-1) - vrati v pripade vynimky, (1) - vrati v pripade uspechu*/
        public override int Open(string serial)
        {
            int toreturn;
            if (nowState == State.Closed)
            {
                try
                {
                    Camera.OpenDevice(xiCam.OpenDevBy.SerialNumber, serial);
                    nowState = State.OpenIdle;
                    InitaliseParamCamera();
                    toreturn = 1;
                }
                catch (Exception exception)
                {
                    toreturn = -1;
                }

            }
            else
            {
                toreturn = 0;
            }

            return toreturn;
        }

        /* Start Acquisition na zariadeni ak kamera je v stave (OpenIdle) nastavi sa (AquiIdle) */
        public override int StartImageAcquisition()
        {
            int toreturn;
            if (nowState == State.OpenIdle)
            {
                try
                {
                    Camera.StartAcquisition();
                    nowState = State.AquiIdle;
                    toreturn = 1;
                }
                catch (Exception exception)
                {
                    toreturn = -1;
                }
            }
            else if (nowState == State.AquiIdle)
            {
                toreturn = 1;
            }
            else
            {
                toreturn = 0;
            }
            return toreturn;
        }

        /* Stop Acquisition na zariadeni ak kamera je v stave (AquiIdle) nastavi sa (OpenIdle) */
        public override int StopImageAcquisition()
        {
            int toreturn;
            if (nowState == State.AquiIdle)
            {
                try
                {
                    Camera.StopAcquisition();
                    nowState = State.OpenIdle;
                    toreturn = 1;
                }
                catch (Exception exception)
                {
                    toreturn = -1;
                }
            }
            else
            {
                toreturn = 0;
            }
            return toreturn;
        }


        /*Vrati null ak snimanie zlyhalo*/
        protected override Bitmap TakeImage()
        {

            Bitmap tempImage = null;

            if (nowState != State.AquiIdle)
            {
                //logging
            }
            else
            {
                int timeout = _exposure * 2 + 200;

                try
                {
                    nowState = State.AquiExposure;
                    Camera.SetParam(PRM.TRG_SOFTWARE, 1);
                    nowState = State.AquiReading;
                    Camera.GetImage(out tempImage, timeout);
                    nowState = State.AquiIdle;

                }
                catch (Exception exception)
                {


                    nowState = State.AquiIdle;

                    if (exception.Message.Equals("Error 1", StringComparison.OrdinalIgnoreCase))
                    {
                        nowState = State.Closed;
                        Dispose();
                    }
                    else if (exception.Message.Contains("Error 10"))
                    {
                        //logging
                    }
                    else
                    {
                        //logging
                    }
                }


            }

            return tempImage;
        }

        protected override int Close()
        {
            int toreturn = 0;
            if (nowState != State.Closed)
            {
                try
                {
                    Camera.CloseDevice();
                }
                catch (Exception exception)
                {
                    toreturn = -1;
                }
                finally
                {
                    nowState = State.Closed;
                }
            }
            else
            {
                toreturn = 0;
            }

            return toreturn;
        }
    }
}

