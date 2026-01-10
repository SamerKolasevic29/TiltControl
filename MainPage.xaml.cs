using TiltControl.Logic;
using System.Diagnostics;

namespace TiltControl
{
    public partial class MainPage : ContentPage
    {

        private readonly HysteresisMapper _mapper = new HysteresisMapper();

        Color OnColor = Colors.Cyan;
        Color OffColor = Color.FromArgb("#333");

        private string _lastProtocol = "";
        private ControlLevel _lastAccLevel = ControlLevel.STOP;
        private ControlLevel _lastStrLevel = ControlLevel.STOP;

        Border[] _fwd;
        Border[] _bck;
        Border[] _lft;
        Border[] _rgt;

        Color[] _cyanRamp;
        Color[] _limeRamp;

        public MainPage()
        {
            InitializeComponent();

            _fwd = new[] { F1, F2, F3, F4, F5 };
            _bck = new[] { B1, B2, B3, B4, B5 };
            _lft = new[] { L1, L2, L3, L4, L5 };
            _rgt = new[] { R1, R2, R3, R4, R5 };

            _cyanRamp = new[]
            {
                (Color)Application.Current.Resources["Cyan5"],
                (Color)Application.Current.Resources["Cyan4"],
                (Color)Application.Current.Resources["Cyan3"],
                (Color)Application.Current.Resources["Cyan2"],
                (Color)Application.Current.Resources["Cyan1"],
            };

            _limeRamp = new[]
            {
                (Color)Application.Current.Resources["Lime5"],
                (Color)Application.Current.Resources["Lime4"],
                (Color)Application.Current.Resources["Lime3"],
                (Color)Application.Current.Resources["Lime2"],
                (Color)Application.Current.Resources["Lime1"],
            };

            //                                 --- CORE TECHNOLOGY: Sensor Initialization and Robustness Check ---
            //         -checking does this device supporting wanting sensors
            if (Accelerometer.Default.IsSupported)
            {
                try
                {
//                -Subscribing to the reading event and setting the speed to Game (fast updates ~50-60Hz)
                    Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                    Accelerometer.Default.Start(SensorSpeed.Game);

                    System.Diagnostics.Debug.WriteLine("INFO: Accelerometer running successfully!");
                }

                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: failed while running Accelerometer {ex.Message} ");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Accelerometer is not supported on this device!");
            }
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            // Pozivamo pametni mapper
            var state = _mapper.GetState(-e.Reading.Acceleration.X, e.Reading.Acceleration.Y);

            if (state.protocol == _lastProtocol) return;
            _lastProtocol = state.protocol;

            if(_lastAccLevel != state.accLvl || _lastStrLevel != state.strLvl) 
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(25));

                _lastAccLevel = state.accLvl;
                _lastStrLevel = state.strLvl;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
               
                FeedbackLabel.Text = state.protocol;

               
                UpdateGrid(state.accLvl, state.isFwd, state.strLvl, state.isRgt);
            });
        }


        //      Cleanup: Stop the accelerometer when the page disappears
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (Accelerometer.Default.IsMonitoring)
            {
                try
                {
                    Accelerometer.Default.Stop();
                    Accelerometer.Default.ReadingChanged -= Accelerometer_ReadingChanged;
                    Debug.WriteLine("INFO: Accelerometer stopped.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR: failed while stopping Accelerometer {ex.Message} ");
                }
            }
        }

        private void UpdateGrid(ControlLevel accLvl, bool isFwd, ControlLevel strLvl, bool isRgt)
        {
            // Resetuj sve prvo na sivo
            ResetAll();

            PaintAxis(_fwd, _bck, _cyanRamp, (int)accLvl, isFwd);
            PaintAxis(_rgt, _lft, _limeRamp, (int)strLvl, isRgt);
        
        }

        private void ResetAll()
        {
            // Brzi nacin: stavi ih u listu ako zelis optimizaciju, ali i ovo radi
            F1.Background = F2.Background = F3.Background = F4.Background = F5.Background = OffColor;
            B1.Background = B2.Background = B3.Background = B4.Background = B5.Background = OffColor;
            L1.Background = L2.Background = L3.Background = L4.Background = L5.Background = OffColor;
            R1.Background = R2.Background = R3.Background = R4.Background = R5.Background = OffColor;
        }

        void PaintAxis(Border[] positive, Border[] negative, Color[] ramp, int level, bool isPositive)
        {
            if (level <= 0) return;

            var target = isPositive ? positive : negative;

            for (int i = 0; i < level; i++)
                target[i].Background = ramp[i];
        }

    }
    }

