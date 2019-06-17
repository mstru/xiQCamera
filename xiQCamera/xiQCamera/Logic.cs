using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using xiQCamera.CameraAPI;
using xiQCamera.Helper;

namespace xiQCamera
{
    public static class Logic
    {
        //Premenne
        public const String StaticPath = "";
        public static String MyPath { get; set; }

        private static Task _CaptureImage = new Task(ImageStream);
        private static Stopwatch stopwatchCapture = new Stopwatch();
        private static Stopwatch stopwatchWrite = new Stopwatch();

        //ak za zapisuje nastavenie na 1 inak 0
        private static volatile Int32 _updateDue = 1;

        private volatile static Int32 _FilesWriting = 0; 

        // Odchytenie vsetkych cameras cez bazovu triedu
        private static CameraBase[] _cameras;

        public static CameraHelper CameraHelper = new CameraHelper(false, 1, 10);

                      
        /*Spustenie snimania*/
        public static void ImageStream()
        {
            while (CameraHelper.Active && _cameras.Length > 0)
            {
                stopwatchCapture.Reset();
                stopwatchCapture.Start();

                if (Interlocked.CompareExchange(ref _updateDue, 0, 1) == 1)
                // Skontroluje, ci nastala aktualizacia na UI, ak potom nahradi aktualizovanu hodnotu 0 a vstupi do cyklu
                {
                    foreach (CameraBase camera in _cameras)
                    {          
                        if (camera.GetCurrentState() == CameraBase.State.AquiIdle)
                        //stop
                        {                
                            camera.StopImageAcquisition();
                        }
   
                        camera.UpdateSettings(CameraHelper, CameraHelper.Exposure, (float)CameraHelper.Gain, (float)CameraHelper.GammaY, (float)CameraHelper.GammaC);
                    }

                    //Update time zobrazujem na UI
                    CameraHelper.PerformanceTime[0] = (int)stopwatchCapture.ElapsedMilliseconds;

                    stopwatchCapture.Stop();
                }
                else
                {
                    foreach (CameraBase camera in _cameras)
                    {
                        if (camera.GetCurrentState() == CameraBase.State.OpenIdle)
                        //start akvizicie na kamere
                        {
                            camera.StartImageAcquisition();
                        }

                    }

                    ushort[][][] temp = new ushort[_cameras.Length][][];

                    Parallel.For(0, _cameras.Length, i =>
                    {
                        temp[i] = _cameras[i].TakeImageUshort2D(stopwatchCapture);
                    });

                    bool imagingSucceded = true;

                    //Kontrola
                    foreach (var image in temp)
                    {
                        if (image == null)
                            imagingSucceded = false;
                    }

                    if (!imagingSucceded)
                    {
                        //zlyhanie kamery pri ziskavani image
                    }
                    else if (Interlocked.Increment(ref _FilesWriting) < 3)
                    {
                        //WriteToDisk(temp);

                        Task _writeToDisk = new Task(() =>
                        {
                            WriteToDisk(temp);
                        });
                        _writeToDisk.Start();
                    }
                    else
                    {
                        //Skip zapisavania
                    }

                    stopwatchCapture.Stop();
                    int delay = (int)((1000 / CameraHelper.FPS) - stopwatchCapture.ElapsedMilliseconds);

                    if (delay > 0)
                    {
                        System.Threading.Thread.Sleep(delay);
                    }
                    else
                    {

                    }

                    //Total performance time - zobrazujem na UI
                    CameraHelper.PerformanceTime[3] = (int)stopwatchCapture.ElapsedMilliseconds;
                }          
            }
        }

        //Volam na zaciatku
        public static void Load_Camera()
        {
            _cameras = new CameraBase[CameraBase.GetNOfAllCameras()];

            CameraHelper.Cameras = (short)_cameras.Length;

            if (_cameras.Length > 0)
                _cameras[0] = new XimeaCamera(0);
            else
            {
                // Ak pouzivatel zavola tlacidlo Reset a kamera je v stave odpojena. Resetujem.

                CameraHelper._nOfcameras = 0; 
                CameraHelper._SerialNumberCamera = null;
            }               
        }

        public static void ResetCamera()
        {
            if (_CaptureImage.Status != System.Threading.Tasks.TaskStatus.Running)
            {
                Close_Camera();
                Load_Camera();
            }
            else
            {
            }
        }

        private static void Close_Camera()
        {
            foreach (CameraBase camera in _cameras)
            {
                camera.Dispose();
            }
        }


        public static void Start()
        {
            // Ak task caka, tak spustam.
            if (_CaptureImage.Status == System.Threading.Tasks.TaskStatus.Created ||
                _CaptureImage.Status == System.Threading.Tasks.TaskStatus.WaitingForActivation ||
                _CaptureImage.Status == System.Threading.Tasks.TaskStatus.WaitingToRun)
            {
                _CaptureImage.Start();

            }// Ak je task dokonceny spustam znovu.
            else if (_CaptureImage.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
                _CaptureImage.ContinueWith((continuation) =>
                {
                    ImageStream();
                });
            }// Ak je zlyhanie,znovu aktivujem.
            else if (_CaptureImage.Status == System.Threading.Tasks.TaskStatus.Faulted)
            {
                ResetCamera();
                _CaptureImage.ContinueWith((continuation) =>
                {
                    ImageStream();
                });
            }
            else if (_CaptureImage.Status == System.Threading.Tasks.TaskStatus.Running)
            {
                //OK
            }
        }

        /// <summary>
        /// Zapis na disk podporeny iba format tiff
        /// </summary>
        /// <param name="allImages"></param>
        private static void WriteToDisk(ushort[][][] allImages)
        {
            stopwatchWrite.Reset();
            stopwatchWrite.Start();
            int bpp = 16;
            string fileFormat = ".tiff";

            allImages = imageshift(allImages);

            CameraHelper.PerformanceTime[4] = (int)stopwatchWrite.ElapsedMilliseconds;

            if (CameraHelper.curBitDepth == Command.BitDepth.Mono8bpp)
            {
                bpp = 8;
            }
            else if (CameraHelper.curBitDepth == Command.BitDepth.Mono16Bpp)
            {
                bpp = 16;
            }

            //if (CameraHelper.CurImageFileFormat == Command.ImageFileFormat.Bmp)
            //{
            //    fileFormat = ".bmp";
            //}
            //else if (CameraHelper.CurImageFileFormat == Command.ImageFileFormat.Jpeg)
            //{
            //    fileFormat = ".jpeg";
            //}
            //else if (CameraHelper.CurImageFileFormat == Command.ImageFileFormat.Jpg)
            //{
            //    fileFormat = ".jpg";
            //}
            //else if (CameraHelper.CurImageFileFormat == Command.ImageFileFormat.Png)
            //{
            //    fileFormat = ".png";
            //}
            //else if (CameraHelper.CurImageFileFormat == Command.ImageFileFormat.Tiff)
            //{
            //    fileFormat = ".tiff";
            //}

            int i = 1;
            foreach (ushort[][] image in allImages)
            {
                String path = MyPath + "/" + "Camera" + (i).ToString() + " frame" + CameraHelper.ImagesTaken.ToString() + ".tiff";
                SavingImage.Write(image, path, allImages[0][0].Length, allImages[0].Length, bpp);
                i++;
            }

            Interlocked.Decrement(ref _FilesWriting);

            CameraHelper.PerformanceTime[5] = (int)stopwatchWrite.ElapsedMilliseconds - CameraHelper.PerformanceTime[4];
            stopwatchWrite.Stop();

            CameraHelper.ImagesTaken++;
        }

        // TODO: NoImplementation
        public static void ChangeShift()
        {
            for (int i = 0; i < _cameras.Length; i++) //aby nepadalo do chyby index out of range ked nenajde kameru
            {
                _cameras[i].Xoffset = CameraHelper.Xshift[i];
                _cameras[i].Yoffset = CameraHelper.Yshift[i];
            }
        }

        // TODO: NoImplementation
        public static void ChangeExpCorr()
        {
            for (int i = 0; i < _cameras.Length; i++) //aby nepadalo do chyby index out of range ked nenajde kameru
            {
                _cameras[i].digitCorrection = (float)CameraHelper.DigitCorrection[i];
                _cameras[i].expCorrection = (float)CameraHelper.ExpCorrection[i];
            }
        }

        private static ushort[][][] imageshift(ushort[][][] allImages)
        {
            int XshiftMax = 0; //maximum positive shift
            int YshiftMax = 0;

            foreach (CameraBase cam in _cameras)
            {
                if (cam.Xoffset > XshiftMax)
                    XshiftMax = cam.Xoffset;

                if (cam.Yoffset > YshiftMax)
                    YshiftMax = cam.Yoffset;

            }

            int NewYres = allImages[0].Length - Math.Abs(YshiftMax);
            int NewXres = allImages[0][0].Length - Math.Abs(XshiftMax);

            ushort[][][] corrected = new ushort[allImages.Length][][];

            for (int cam = 0; cam < allImages.Length; cam++)
            {
                corrected[cam] = new ushort[NewYres][];
                for (int Y = 0; Y < NewYres; Y++)
                {
                    corrected[cam][Y] = new ushort[NewXres];
                    for (int X = 0; X < NewXres; X++)
                    {
                        corrected[cam][Y][X] = allImages[cam][Y + _cameras[cam].Yoffset][X + _cameras[cam].Xoffset];
                    }
                }
            }

            return corrected;
        }

        public static void QueUpdate()
        {
            _updateDue = 1;
        }
    }
}
