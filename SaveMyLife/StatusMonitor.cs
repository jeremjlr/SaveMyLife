using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace SaveMyLife
{
    public class StatusMonitor
    {
        #region CONST
        const int _HPBAR_WIDTH = 42;
        const int _HPBAR_HEIGHT = 181;
        const int _HPBAR_XPOS = 104;
        const int _HPBAR_YPOS = 871;
        #endregion

        #region Private attributes
        private IntPtr _GAME_HWND;
        private string _calibrationPath = "Calibrations/";
        private int _maxHpPixel;
        #endregion

        #region Public attributes
        public double HP
        {
            get;
            private set;
        }
        #endregion

        #region P/Invoke stuff
        [DllImportAttribute("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        #endregion

        #region Constructors and public methods
        public StatusMonitor(IntPtr game_hWnd)
        {
            _GAME_HWND = game_hWnd;
            //Starts the information gathering thread
            StatusUpdater.DoWork += StatusUpdate;
            StatusUpdater.RunWorkerAsync();
        }

        public void Calibrate()
        {
            //Calibrates where the 100% hp pixel is
            _maxHpPixel = GetHighestHpPixel();
            Console.WriteLine("Highest hp pixel is : " + _maxHpPixel);

            GetHpBar().Save(_calibrationPath + "Hp.bmp");
            Console.WriteLine("Calibrated");
        }
        #endregion

        #region Private methods
        private BackgroundWorker StatusUpdater = new BackgroundWorker();
        private void StatusUpdate(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                //If the game has focus and we want to gather information
                if (GameHasFocus())
                {
                    //We gather information on screen
                    HP = GetHP();
                }
                Thread.Sleep(50);
            }
        }

        private bool GameHasFocus()
        {
            if (GetForegroundWindow() == _GAME_HWND)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int GetHighestHpPixel()
        {
            Image<Bgr, byte> hpBar = GetHpBar();
            //Scans all pixels until it reaches a non black pixel then returns the y postion of that pixel
            for (int i = 0; i < _HPBAR_HEIGHT; i++)
            {
                for (int j = 0; j < _HPBAR_WIDTH; j++)
                {
                    if (hpBar[i, j].Red != Color.Black.R)
                    {
                        return _HPBAR_HEIGHT - i;
                    }
                }
            }
            return 0;
        }

        //Returns the number of HP in %
        private double GetHP()
        {
            //Console.WriteLine(GetHighestHpPixel());
            return GetHighestHpPixel() / (double)_maxHpPixel * 100f;
        }

        //Get the HP bar image from screen, filtered to red
        private Image<Bgr, byte> GetHpBar()
        {
            Bitmap hpBar = new Bitmap(
                        _HPBAR_WIDTH,
                        _HPBAR_HEIGHT,
                        PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(hpBar);
            g.CopyFromScreen(
                _HPBAR_XPOS,
                _HPBAR_YPOS,
                0,
                0,
                new Size(_HPBAR_WIDTH, _HPBAR_HEIGHT),
                CopyPixelOperation.SourceCopy);

            return FilterToRed(hpBar, 85, 50);
            //return new Image<Bgr, byte>(hpBar);
        }

        //Filters an image, only keeping red pixels within thresholds
        private Image<Bgr, byte> FilterToRed(Bitmap source, int thresholdUp, int thresholdDown)
        {
            Image<Bgr, byte> image = new Image<Bgr, byte>(source);
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    if (image[i, j].Red > thresholdUp && image[i, j].Blue < thresholdDown && image[i, j].Green < thresholdDown)
                    {
                        image[i, j] = image[i, j];
                    }
                    else
                    {
                        image[i, j] = new Bgr(Color.Black);
                    }
                }
            }
            return image;
        }
        #endregion
    }
}
