using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using xiQCameraUI.Properties;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using xiQCamera.Helper;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using xiQCamera;
using System.Threading.Tasks;

namespace xiQCameraUI
{
    /// <summary>
    /// Integracia logiky pre MainWindow.xaml UI
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /*Objek na synchonizaciu*/
        private object sync = new object();

        /*tracks pre pouzivatelsky vstup cez textblock*/
        private static readonly Command UI_action = new Command(false, 10, 1);

        private static XimeaSource Camera;

        /*Spatna vazba*/
        public string Feedback = "";

        /* Trieda umoznuje vytvaranie datovej polozky a jej postupnu aktualizaciu a uchovavanie kazdej timestamp*/
        public CameraHelper cameraHelper = new CameraHelper();

        public int NOfcameras = 0;

        /*Pouziva sa na rozlisenie pouzivatelskych udalosti na UI vstup cez textbox alebo zmenou kodu na UI cez slider*/
        private static bool InternalChange = false;

        /* programm state tracker*/
        public static State _gstatus = State.Idle; 

        private static bool UI_loaded = false;

        /* Kolekcia images */
        public List<Models.Image> iCollection = new List<Models.Image>();

        /* Color pre progressBar */
        public SolidColorBrush progressBarColor;

        /* Povolene hodnoty Alpha numeric*/
        private Regex _RegexAlphanumeric = new Regex(@"[a-zA-Z0-9,.,:,\\]");

        /* Povolene hodnoty Numeric*/
        private Regex _RegexNumeric = new Regex("[0-9.]");

        public int TotalImages = 0;
        public int TakenImageIncoming = 0;

        /* Stav */
        public enum State
        {
            Idle,
            Connecting,
            ConnectedOff,
            ConnectedOn,
            Error,
        }

        /* Resources - pouzite ikonky v aplikacii */
        private ImageSource IconLoad = (ImageSource)new ImageSourceConverter().ConvertFrom(@"pack://application:,,,/Resources/Performance.png");
        private ImageSource IconOff = (ImageSource)new ImageSourceConverter().ConvertFrom(@"pack://application:,,,/Resources/ShutDown.png");
        private ImageSource IconOK = (ImageSource)new ImageSourceConverter().ConvertFrom(@"pack://application:,,,/Resources/OK.png");
        private ImageSource IconNOK = (ImageSource)new ImageSourceConverter().ConvertFrom(@"pack://application:,,,/Resources/NOK.png");
        private ImageSource IconImagingStart = (ImageSource)new ImageSourceConverter().ConvertFrom(@"pack://application:,,,/Resources/imagingStart.png");
        private ImageSource IconImagingStop = (ImageSource)new ImageSourceConverter().ConvertFrom(@"pack://application:,,,/Resources/imagingStop.png");
        private ImageSource IconSave = (ImageSource)new ImageSourceConverter().ConvertFrom(@"pack://application:,,,/Resources/Save.png");
        private ImageSource IconCamera = (ImageSource)new ImageSourceConverter().ConvertFrom(@"pack://application:,,,/Resources/Camera.png");


        public MainWindow()
        {           
            InitializeComponent();

            Logic.Load_Camera();
            Camera = new XimeaSource(this);
            LoadSettings();

            UI_loaded = true;

            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dt.Tick += ImagesLoaded;
            dt.Start();
            //dt.Stop();
        }

        /// <summary>
        /// Zalozka Images - nacitanie dat do gridu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImagesLoaded(object sender, EventArgs e)
        {
            //Performace - zatial to tu necham takto, pre update na UI
            UI_PerformanceUpdate.Content = Logic.CameraHelper.PerformanceTime[0].ToString() + " ms"; //Perf update [ms]
            UI_PerformanceImage.Content = Logic.CameraHelper.PerformanceTime[1].ToString() + " ms"; //Perf aquisition [ms]
            UI_PerformanceWrite.Content = Logic.CameraHelper.PerformanceTime[5].ToString() + " ms"; //Perf write [ms]
            UI_PerformanceTotal.Content = Logic.CameraHelper.PerformanceTime[3].ToString() + " ms"; //Perf total [ms]

            //Actual value - zatial to tu necham takto, pre update na UI
            UI_ActualExposure.Content = Logic.CameraHelper._nOfcameras > 0 ? Logic.CameraHelper.GetparamExposure.ToString() + " ms" : "? ms";
            UI_ActualGain.Content = Logic.CameraHelper._nOfcameras > 0 ? Logic.CameraHelper.GetparamGain.ToString() + " db" : "? db";

            #region Adresar pre images

            if (UI_FolderInput.Text.Length > 0)
            {
                UI_action.Foldername = UI_FolderInput.Text;
            }
            else
            { UI_action.Foldername = @"C:\xiqCamera\Images"; };

            string pathtoImages = UI_action.Foldername;
            string root = System.IO.Path.GetDirectoryName(pathtoImages);

            if (System.IO.Directory.Exists(pathtoImages))
            {               
            }
            else
            { System.IO.Directory.CreateDirectory(pathtoImages); }
            #endregion

            string[] supportedExtensions = new[] { ".bmp", ".jpeg", ".jpg", ".png", ".tiff" };
            var files = Directory.GetFiles(Path.Combine(root, "Images"), "*.*").Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower()));

            if (files.Count() != TotalImages)
            {
                List<Models.Image> images = new List<Models.Image>();

                foreach (var file in files)
                {
                    try
                    {
                        Models.Image id = new Models.Image()
                        {
                            Path = file,
                            FileName = Path.GetFileName(file),
                            Extension = Path.GetExtension(file)
                        };

                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.UriSource = new Uri(file, UriKind.Absolute);
                        img.EndInit();
                        id.Width = img.PixelWidth;
                        id.Height = img.PixelHeight;

                        FileInfo fi = new FileInfo(file);
                        id.Size = fi.Length;
                        id.CreationTime = fi.CreationTime;

                        images.Add(id);
                    }
                    catch (Exception) { }
                }

                if (iCollection.Count() != 0)
                {
                    foreach (var f in images)
                    {
                        bool result = iCollection.Any(x => x.FileName == f.FileName);

                        if (!result)
                        {
                            iCollection.Add(f);
                            TotalImages++;
                            TakenImageIncoming++;
                        }
                    }
                }
                else
                {
                    iCollection = images;
                    TotalImages = files.Count();
                }

                // Sortovanie podla casu vytvorenia
                var imagesOrderBy = iCollection.OrderByDescending(x => x.CreationTime);

                // Vkladam do gridu 
                ImageDataGrid.ItemsSource = imagesOrderBy;

                // Zobrazujem na UI pocet prijatych Images
                UI_ImagesTaken.Content = TakenImageIncoming.ToString() + " images";
            }
        }

        

        /// <summary>
        /// Tlacidlo Connect - pokus o pripojenie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TryToConnect(object sender, RoutedEventArgs e)
        {
            UI_Connect_Button.IsEnabled = false;
            UI_image_connection.Source = IconCamera;

            // Klknutie na Re-initialize
            if (UI_action.IsReseting)
            {
                Logic.Load_Camera();
                UI_action.IsReseting = false;
            }

            State update;

            switch (_gstatus)
            {
                //Prva inicializacia pokus o pripojenie
                case State.Idle:
                    try
                    {                                                                                                
                        var temp = new CameraHelper();
                        UpdateUi(temp);
                        update = State.ConnectedOn;
                        SaveSettings("Settings was successfully saving");
                    }
                    catch (Exception exception)
                    {
                        Feedback = exception.Message;
                        update = State.Idle;
                    }

                    break;
                    
                case State.Connecting:
                case State.ConnectedOn:
                    Feedback = "Connection Closed";
                    //Camera.Reset();
                    update = State.Idle;
                    break;
                case State.ConnectedOff:
                    Feedback = "Connection Closed";
                    //Camera.Reset();
                    update = State.Idle;
                    break;

                default:
                    Feedback = "Error";
                    update = State.Error;
                    break;
            }
            _gstatus = update;
            UpdateUi(Feedback);
            UI_Connect_Button.IsEnabled = true;

        }

        /* Uloží vstup používateľa pre ďalšiu session */
        private void SaveSettings(string feedBack)
        {
            if (UI_loaded)
            {

                Settings.Default.FolderName = UI_FolderInput.Text;
    
                Settings.Default.Xshift = UI_action.Xshift[0];
                Settings.Default.Yshift = UI_action.Yshift[0];
                Settings.Default.ExpCorrect = UI_action.ExpCorrection[0];
                Settings.Default.DigitCorrect = UI_action.DigitCorrection[0];

                Settings.Default.Save();

                //UI
                UI_Feedback.Content = feedBack;
                UI_image_connection.Source = IconSave;
            }
            else
            {
            }
        }

        /* Načíta nastavenia */
        private void LoadSettings()
        {
            UI_FeedbackGain.Text = Settings.Default.Gain.ToString();
            UI_setGain.Value = Settings.Default.Gain;

            UI_FeedbackImageFreq.Text = Settings.Default.FPS.ToString();
            UI_setImagingFreq.Value = Settings.Default.FPS;

            UI_FeedbackExposure.Text = Settings.Default.Exposure.ToString();

            if (Settings.Default.Xshift == null) //prve spustenie programu
            {
                Settings.Default.Xshift = 0;
            }

            //Xcorrection.Text = Settings.Default.Xshift.ToString();

            if (Settings.Default.Yshift == null) //prve spustenie programu
            {
                Settings.Default.Yshift = 0;
            }
            //Ycorrection.Text = Settings.Default.Yshift.ToString();

            if (Settings.Default.ExpCorrect == null) //prve spustenie programu
            {
                Settings.Default.ExpCorrect = 1;
            }
            //ExpCorr.Text = Settings.Default.ExpCorrect.ToString();

            if (Settings.Default.DigitCorrect == null) //prve spustenie programu
            {
                Settings.Default.DigitCorrect = 1;
            }
            //DigCorr.Text = Settings.Default.DigitCorrect.ToString();

            UI_FolderInput.Text = Settings.Default.FolderName;

        }

        /// <summary>
        /// Aktualizacia UI
        /// </summary>
        /// <param name="recieved"></param>
        /// <param name="feedback"></param>
        public void UpdateUi(CameraHelper recieved, string feedback = null)
        {
            NOfcameras = Logic.CameraHelper._nOfcameras;

            progressBarColor = NOfcameras > 0 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);

            UI_action.Active = recieved.Active;
            UI_Image_Button.Content = UI_action.Active ? "Stop" : "Start";
            UI_ActualExposure.Content = NOfcameras > 0 ? Logic.CameraHelper.GetparamExposure.ToString() + " ms" : "? ms";
            UI_ActualGain.Content = NOfcameras > 0 ? Logic.CameraHelper.GetparamGain.ToString() + " db" : "? db";
            string camSerialNumberString = NOfcameras > 0 ? UI_action.CamerasSerialN : "  ?";
            UI_Cameras_Connected.Content = NOfcameras.ToString() + " camera S/N " + camSerialNumberString;

            UI_ImagesTaken.Content = TakenImageIncoming.ToString() + " images";

            //Performance
            //UI_PerformanceUpdate.Content = recieved.PerformanceTime[0].ToString() + " ms"; //Perf update [ms]
            //UI_PerformanceImage.Content = recieved.PerformanceTime[1].ToString() + " ms"; //Perf aquisition [ms]
            //UI_PerformanceWrite.Content = recieved.PerformanceTime[5].ToString() + " ms"; //Perf write [ms]
            //UI_PerformanceTotal.Content = recieved.PerformanceTime[3].ToString() + " ms"; //Perf total [ms]

            if (recieved.Active)
            {
                _gstatus = State.ConnectedOn;
            }
            else { _gstatus = State.Connecting; }

            UpdateUi(feedback);
        }

        /// <summary>
        /// Aktualizacia UI
        /// </summary>
        public void UpdateUi(string feedback = null, bool newconnection = false)
        {
            string buttontext;
            if (feedback != null)
                UI_Feedback.Content = feedback;

            switch (_gstatus)
            {
                case State.Idle:
                    buttontext = "Connect";
                    ProgressbarAnimation(0, 0.5);
                    UI_Progress.Background = new SolidColorBrush(Colors.White);
                    UI_Image_Button.IsEnabled = false;
                    UI_Apply_Button.IsEnabled = false;
                    //UI_SelectFileFormat.IsEnabled = false;
                    UI_image_connection.Source = IconOff;
                    UI_Cameras_Connected.Content = "";
                    UI_Reset_Button.IsEnabled = false;
                    UI_Reset_Button.IsEnabled = false;
                    break;
                case State.Connecting:                   
                    buttontext = "Cancel";
                    ProgressbarAnimation(100, 25);
                    UI_Image_Button.IsEnabled = false;
                    UI_Apply_Button.IsEnabled = false;
                    //UI_SelectFileFormat.IsEnabled = false;
                    UI_image_connection.Source = IconCamera;
                    UI_Reset_Button.IsEnabled = false;
                    UI_Reset_Button.IsEnabled = false;
                    //UI_Feedback.Content = "Attempting to connect";

                    break;
                case State.ConnectedOff:
                    buttontext = "Disconnect";
                    UI_Progress.Value = 100;
                    ProgressbarAnimation(100, 0.5);                                     
                    UI_Image_Button.IsEnabled = true;
                    UI_Apply_Button.IsEnabled = true;
                    //UI_SelectFileFormat.IsEnabled = true;
                    UI_image_connection.Source = NOfcameras > 0 ? IconOK : IconLoad;
                    UI_Reset_Button.IsEnabled = true;
                    UI_Reset_Button.IsEnabled = true;
                    //UI_Feedback.Content = "XIMEA camera connected to the system.";
                    break;
                case State.ConnectedOn:                  
                    buttontext = NOfcameras > 0 ? "Disconnect" : "Cancel" ;
                    UI_Progress.Value = 100;
                    ProgressbarAnimation(100, 0.5);
                    UI_Image_Button.IsEnabled = NOfcameras > 0 ? true : false;
                    UI_Apply_Button.IsEnabled = NOfcameras > 0 ? true : false;
                    //UI_SelectFileFormat.IsEnabled = NOfcameras > 0 ? true : false;
                    UI_image_connection.Source = NOfcameras > 0 ? IconOK : IconNOK;
                    UI_Reset_Button.IsEnabled = true;       
                    UI_Reset_Button.IsEnabled = NOfcameras > 0 ? true : false;
                    UI_Feedback.Content = NOfcameras > 0 ? "Camera connected to the system." : "Camera not found."; 
                    break;
                default:
                    buttontext = "Error";
                    UI_Image_Button.IsEnabled = false;
                    break;
            }

            UI_Connect_Button.Content = buttontext;
        }

        /// <summary>
        /// Logika pre progress Bar
        /// </summary>
        private void ProgressbarAnimation(int value, double durationS)
        {
            var duration = new Duration(TimeSpan.FromSeconds(durationS));
            var doubleanimation = new DoubleAnimation(value, duration);
            UI_Progress.Background = progressBarColor;
            UI_Progress.BeginAnimation(RangeBase.ValueProperty, doubleanimation);
        }

        /// <summary>
        /// Pokus o Reseting
        /// </summary>
        private void Reset(object sender, RoutedEventArgs e)
        {
            UI_action.IsReseting = true;
            Update_Setting(sender, e);
            //UI_action.Reset = false;
                      
            //xiQCamera.Logic.Load_Camera();

            _gstatus = State.Idle;

            //Spravim na UI update
            UpdateUi("Re-initialize camera.");
        }



        /* EXPOSURE slider*/
        /* Funkcia, ktora meni stupnicu expozicie
         * zobrazi text v UI_FeedbakcExposure a tento parameter spristupní prikazu Command Exposure */
        private void Exposure_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (InternalChange == true)
            {
                InternalChange = false;
            }
            else
            {
                var temp = (Math.Pow(10, e.NewValue));

                UI_action.Exposure = (int)temp;

                var ExposureDisplayMs = Math.Round(temp / 1000, 2);

                if (UI_FeedbackExposure != null)
                {
                    UI_FeedbackExposure.Text = ExposureDisplayMs.ToString();

                    //Update Settings
                    Settings.Default.Exposure = ExposureDisplayMs;
                    Settings.Default.Save();
                }
            }
        }

        /* EXPOSURE textBox*/
        private void UI_FeedbackExposure_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Min/Max hodnoty ms
            double max = 1000000;
            double min = 0.000001; 

            System.Windows.Controls.TextBox textbox = (System.Windows.Controls.TextBox)sender; //Odovzdanie objektu ako textBox

            //Kontrola vstupu na blbosti
            inputcontrol(textbox, _RegexNumeric);

            if (InternalChange == true)
            {
                InternalChange = false;
            }
            else if (textbox != null && UI_setExposure != null)
            {
                try
                {
                    double temp = 1000 * Double.Parse(textbox.Text);

                    if (temp > max)
                    {
                        System.Media.SystemSounds.Exclamation.Play(); //zvuk
                        temp = max;
                    }
                    else if (temp < min)
                    {
                        System.Media.SystemSounds.Exclamation.Play(); //zvuk
                        temp = min;
                    }


                    double sliderPosition = Math.Log(temp, 10);
                    // update pre slider
                    InternalChange = true;
                    UI_setExposure.Value = sliderPosition;
                    InternalChange = false;


                    UI_action.Exposure = (int)temp; //Save

                    Settings.Default.Exposure = Math.Round(temp / 1000, 2);
                    Settings.Default.Save();

                }
                catch (FormatException exception)
                {
                    UI_Feedback.Content = "Invalid Format";
                }
            }
            else
            {
                //UI nie je inicializovane
            }

        }


        /* FPS slider*/
        private void FPS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = updateTextboxFromSlider(UI_setImagingFreq, UI_FeedbackImageFreq);
            value = Math.Round(value, 1);
            if (value >= 0)
            {
                Settings.Default.FPS = value;
                Settings.Default.Save();

                UI_action.FPS = value;
            }
            else
            { //Invalid Value, do nothing
            };
        }

        /* FPS textbox*/
        private void UI_FeedbackFPS_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textbox = (System.Windows.Controls.TextBox)sender; //cast the calling object as a textbox
            inputcontrol(textbox, _RegexNumeric);

            double value = updateSliderFromTextbox(UI_FeedbackImageFreq, UI_setImagingFreq, 10d, 0d);
            if (value >= 0)
            {
                Settings.Default.FPS = value;
                Settings.Default.Save();

                UI_action.FPS = value;
            }
            else
            { //Invalid Value, do nothing
            }
        }

        /* Zoberie hodnotu v slider a aktualizuje textove pole */
        private double updateTextboxFromSlider(System.Windows.Controls.Slider slider, System.Windows.Controls.TextBox textbox)
        {
            if (InternalChange == true)
            {
                InternalChange = false;
                return -1;
            }
            else
            {   //Kontrolujem ci je UI pripravene
                if (slider != null && textbox != null)
                {
                    InternalChange = true;
                    double temp = Math.Round(slider.Value, 1);
                    textbox.Text = temp.ToString();


                    return slider.Value;
                }
                else
                {
                    return -1;
                }

            }
        }

        /* GAIN slider*/
        private void Gain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = updateTextboxFromSlider(UI_setGain, UI_FeedbackGain);
            value = Math.Round(value, 1);
            if (value >= 0)
            {
                Settings.Default.Gain = value;
                Settings.Default.Save();

                UI_action.Gain = value;
            }
            else
            { //Invalid nic nerob
            }
        }

        /* GAIN textBox*/
        private void UI_FeedbackGain_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textbox = (System.Windows.Controls.TextBox)sender; //cast the calling object as a textbox
            inputcontrol(textbox, _RegexNumeric);

            double value = updateSliderFromTextbox(UI_FeedbackGain, UI_setGain, 15d, 0d);
            if (value >= 0)
            {
                Settings.Default.Gain = value;
                Settings.Default.Save();

                UI_action.Gain = value;
            }
            else
            { //Invalid nic nerob
            }

        }

        /* Zoberie hodnotu v textBox a aktualizuje slider */
        private double updateSliderFromTextbox(System.Windows.Controls.TextBox textbox, System.Windows.Controls.Slider slider, Double upperLimit, Double lowerLimit)
        {
            if (InternalChange == true)
            {
                InternalChange = false;
                return -1;
            }
            else
            {   //Kontrolujem ci je UI pripravene
                if (slider != null && textbox != null)
                {
                    try
                    {
                        Double temp = Double.Parse(textbox.Text);
                        if (temp > upperLimit)
                        {
                            System.Media.SystemSounds.Exclamation.Play(); //zvuk
                            temp = upperLimit;
                        }
                        else if (temp < lowerLimit)
                        {
                            System.Media.SystemSounds.Exclamation.Play(); //zvuk
                            temp = lowerLimit;
                        }
                        else
                        {
                            //Hodnota je v hranici pokracujem dalej
                        }

                        if (slider.Value != Math.Round(temp, 0))
                        {
                            InternalChange = true;
                            slider.Value = Math.Round(temp, 0);
                        }
                        else
                        {   
                        }


                        return temp;
                    }
                    catch (FormatException exception)
                    {

                        UI_Feedback.Content = "Invalid Format";
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }

            }
        }


        //Kontrola vstupu - Porovna text v textovom poli s regexom a vymaze znak, ktorý je neplatný + zvuk
        private void inputcontrol(System.Windows.Controls.TextBox textbox, Regex regex)
        {
            if (textbox.Text.Length > 0)
            {
                string a = textbox.Text;
                string validated = "";
                int i = 0;

                foreach (Match m in regex.Matches(a))
                {

                    validated += m;
                    i++;
                }

                if (i < a.Length)
                {
                    textbox.Text = validated;
                    System.Media.SystemSounds.Exclamation.Play(); // zvuk 
                }
                else
                {
                    //vsetky vstupy su validne - OK
                }
            }
            else
            {
                //Textove pole je prazdne
            }

        }

        private static Task _CaptureImage = new Task(xiQCamera.Logic.ImageStream);

 
        private void Start_Imaging(object sender, RoutedEventArgs e)
        {
            UI_action.Active = !UI_action.Active;
            UI_action.Foldername = UI_FolderInput.Text;

            DateTime folderstamp = DateTime.Now;
            Logic.MyPath = string.Format("{0}\\Images", Logic.StaticPath);//, folderstamp.Hour.ToString(), folderstamp.Minute.ToString(), folderstamp.Day.ToString(), folderstamp.Month.ToString());

            UI_Image_Button.Content = UI_action.Active ? "Stop" : "Start";
           
            if (Camera != null && (_gstatus == State.ConnectedOff || _gstatus == State.ConnectedOn))
            {          
                lock (sync)
                {
                    UpdateCommand.Execute(UI_action);
                }
            }
            else
            { 
            }

                
            UI_Feedback.Content = UI_action.Active ? "Camera imaging start" : "Camera imaging stop";

            UI_image_connection.Source = UI_action.Active ? IconImagingStart : IconImagingStop;
        }


        private void Update_Setting(object sender, RoutedEventArgs e)
        {
            UI_action.Foldername = UI_FolderInput.Text;
            SaveSettings("Settings was successfully saving");

            if (Camera != null && (_gstatus == State.ConnectedOff || _gstatus == State.ConnectedOn))
                UpdateCommand.Execute(UI_action);
            else
            {
            }
        }

        // TODO: NoImplementation
        private void UI_Xcorrection_textChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                //Double temp = Double.Parse(Xcorrection.Text);
                //UI_action.Xshift[0] = (int)temp;

                SaveSettings("Settings was successfully saving");
            }
            catch (Exception exception)
            {
                System.Media.SystemSounds.Exclamation.Play(); //zvuk
            }
        }

        // TODO: NoImplementation
        private void UI_Ycorrection_textChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                //Double temp = Double.Parse(Ycorrection.Text);
                //UI_action.Yshift[0] = (int)temp;

                SaveSettings("Settings was successfully saving");
            }
            catch (Exception exception)
            {
                System.Media.SystemSounds.Exclamation.Play(); //zvuk
            }
        }

        // TODO: NoImplementation
        private void UI_DigCorr_Textchanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                //UI_action.DigitCorrection[0] = Double.Parse(DigCorr.Text);

                //UI_CorrectTotal.Text = (UI_action.ExpCorrection[0] * UI_action.DigitCorrection[0]).ToString();

                SaveSettings("Settings was successfully saving");
            }
            catch (Exception exception)
            {
                System.Media.SystemSounds.Exclamation.Play(); //zvuk
            }
        }

        // TODO: NoImplementation
        private void UI_ExpCorr_Textchanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {

                //UI_action.ExpCorrection[0] = Double.Parse(ExpCorr.Text);

                //UI_CorrectTotal.Text = (UI_action.ExpCorrection[0] * UI_action.DigitCorrection[0]).ToString();

                SaveSettings("Settings was successfully saving");
            }
            catch (Exception exception)
            {
                System.Media.SystemSounds.Exclamation.Play(); //zvuk
            }
        }

        // Kontrolujem cestu cez povolene hodnoty
        private void CheckInput(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textbox = (System.Windows.Controls.TextBox)sender;
            inputcontrol(textbox, _RegexAlphanumeric);
            SaveSettings($"Folder path '{textbox.Text}' was successfully saving");
        }

        // TODO: NoImplementation
        private void UI_FileFormatSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //System.Windows.Controls.ComboBoxItem item = (System.Windows.Controls.ComboBoxItem)UI_SelectFileFormat.SelectedItem;

            //if (item == UI_cItem_bmp)
            //{
            //    User_input.CurImageFileFormat = Command.ImageFileFormat.Bmp;
            //}
            //else if (item == UI_cItem_jpeg)
            //{
            //    User_input.CurImageFileFormat = Command.ImageFileFormat.Jpeg;
            //}
            //else if (item == UI_cItem_jpg)
            //{
            //    User_input.CurImageFileFormat = Command.ImageFileFormat.Jpg;
            //}
            //else if (item == UI_cItem_png)
            //{
            //    User_input.CurImageFileFormat = Command.ImageFileFormat.Png;
            //}
            //else if (item == UI_cItem_tiff)
            //{
            //    User_input.CurImageFileFormat = Command.ImageFileFormat.Tiff;
            //}
            //else
            //{
            //    string debug = UI_SelectFileFormat.SelectedItem.ToString();
            //    throw new Exception("Following file format is not implemented:" + debug);
            //}

            SaveSettings("Settings was successfully saving");
        }
    }
}