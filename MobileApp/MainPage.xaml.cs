using TiltControl.Logic;
using System.Diagnostics;

namespace TiltControl
{
    public partial class MainPage : ContentPage
    {

        private readonly HysteresisMapper _mapper = new HysteresisMapper();

        private readonly UdpService _udpSender = new UdpService("10.206.81.108", 4210);

        //inital parameters
        private string _lastProtocol = "";
        private ControlLevel _lastAccLevel = ControlLevel.STOP;
        private ControlLevel _lastStrLevel = ControlLevel.STOP;

        //reset (OFF) color
        Color OffColor = Color.FromArgb("#333");

        //arrays for manipulating UI Components
        Border[] _fwd;
        Border[] _bck;
        Border[] _lft;
        Border[] _rgt;

        Color[] _cyanRamp;
        Color[] _limeRamp;

        public MainPage()
        {
            InitializeComponent();

            //modular code where when i add or remove some levels, just delete the data from arrays
            _fwd = new[] { F1, F2, F3 };
            _bck = new[] { B1, B2, B3 };
            _lft = new[] { L1, L2, L3 };
            _rgt = new[] { R1, R2, R3 };


            //same for colors
            _cyanRamp = new[]
            {
                
                (Color)Application.Current.Resources["Cyan3"],
                (Color)Application.Current.Resources["Cyan2"],
                (Color)Application.Current.Resources["Cyan1"]
            };

            _limeRamp = new[]
            {
               
                (Color)Application.Current.Resources["Lime3"],
                (Color)Application.Current.Resources["Lime2"],
                (Color)Application.Current.Resources["Lime1"]
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
            // calling mapper in reading loop
            var state = _mapper.GetState(-e.Reading.Acceleration.X, e.Reading.Acceleration.Y);

            //checking last protocol (perfomance optimisation)
            if (state.protocol == _lastProtocol) return;
            _lastProtocol = state.protocol;

            _ = _udpSender.SendPacketAsync(state.protocol);

            //when levels changed, vibrate for better UX (prototyping the feedback from phone)
            if (_lastAccLevel != state.accLvl || _lastStrLevel != state.strLvl) 
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(25));

                _lastAccLevel = state.accLvl;
                _lastStrLevel = state.strLvl;
            }

            //thread for refreshing UI
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
            //"First reset all..."
            ResetAll();

            //"...Then update borders"
            PaintAxis(_fwd, _bck, _cyanRamp, (int)accLvl, isFwd);
            PaintAxis(_rgt, _lft, _limeRamp, (int)strLvl, isRgt);
        
        }

        private void ResetAll()
        {
            // fast thing: just paint all of them with OffColor
            F1.Background = F2.Background = F3.Background = OffColor;
            B1.Background = B2.Background = B3.Background = OffColor;
            L1.Background = L2.Background = L3.Background = OffColor;
            R1.Background = R2.Background = R3.Background = OffColor;
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

