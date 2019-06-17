using System;

namespace xiQCamera.Helper
{
    public class UpdateCommand
    {
        public static void Execute(Command data)
        {
            if (Logic.CameraHelper.Active == false && data.Active == true)        
            {
                if (data.Foldername.Length == 0)
                {
                }
                else
                {
                    Logic.MyPath = Logic.StaticPath + data.Foldername;
                }


                if (System.IO.Directory.Exists(Logic.MyPath))
                {
                    //Nic nerob ak existuje
                }
                else
                { System.IO.Directory.CreateDirectory(Logic.MyPath); }

                Logic.CameraHelper.Active = data.Active;
                Logic.Start();
            }
            else if (Logic.CameraHelper.Active == false && data.Active == false)
            { }
            else if (Logic.CameraHelper.Active == true && data.Active == false)
            { Logic.CameraHelper.Active = data.Active; }
            else if (Logic.CameraHelper.Active == true && data.Active == true)
            { }
            else
            {
                throw new Exception("Status imaging mismatch!");
            }

            if (Logic.CameraHelper.Gain != data.Gain)
            {
                Logic.CameraHelper.Gain = data.Gain;
            }
            else { }
            if (Logic.CameraHelper.Exposure != data.Exposure)
            {
                Logic.CameraHelper.Exposure = data.Exposure;
            }

            if (Logic.CameraHelper.FPS != data.FPS)
            {
                Logic.CameraHelper.FPS = data.FPS;
            }
            else { }

            if (Logic.CameraHelper.curBitDepth != data.curBitDepth)
            {
                Logic.CameraHelper.curBitDepth = data.curBitDepth;
            }
            else { }

            if (Logic.CameraHelper.CurImageFormat != data.CurImageFormat)
            {
                Logic.CameraHelper.CurImageFormat = data.CurImageFormat;
            }

            bool shiftChanged = false;
            bool correctionUpdated = false;

            for (int i = 0; i < data.Xshift.Length; i++)
            {
                if (data.Xshift[i] != Logic.CameraHelper.Xshift[i] || data.Yshift[i] != Logic.CameraHelper.Yshift[i])
                {
                    shiftChanged = true;
                }
                if (data.ExpCorrection[i] != Logic.CameraHelper.ExpCorrection[i] || data.DigitCorrection[i] != Logic.CameraHelper.DigitCorrection[i])
                {
                    correctionUpdated = true;
                }
            }

            if (shiftChanged)
            {
                Array.Copy(data.Xshift, Logic.CameraHelper.Xshift, Logic.CameraHelper.Xshift.Length);
                Array.Copy(data.Yshift, Logic.CameraHelper.Yshift, Logic.CameraHelper.Yshift.Length);
                //Logging
                Logic.ChangeShift();
            }
            if (correctionUpdated)
            {
                Array.Copy(data.ExpCorrection, Logic.CameraHelper.ExpCorrection, Logic.CameraHelper.ExpCorrection.Length);
                Array.Copy(data.DigitCorrection, Logic.CameraHelper.DigitCorrection, Logic.CameraHelper.DigitCorrection.Length);
                //Logging
                Logic.ChangeExpCorr();
            }


            if (data.IsReseting)
                Logic.ResetCamera();

            Logic.QueUpdate();  
        }
    }
}
