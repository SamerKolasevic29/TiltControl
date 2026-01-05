
using System.Diagnostics;
using TiltControl.Enums;
using TiltControl.Logic;


namespace TiltControl
{
    public partial class MainPage : ContentPage
    {

        public (AccState acc, StrState str) FeedbackState = (acc: AccState.STOP, str: StrState.STOP);


        public MainPage()
        {
            InitializeComponent();

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

        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;

            //                                    --- 1. Deadzone Filtering ---
            //         -If the tilt magnitude is too small (e.g., < 0.02 G), treat it as zero.
            //         -This prevents the disc from shaking when the device is resting flat.
            float x_reading = Math.Abs(data.Acceleration.X) > 0.02 ? data.Acceleration.X : 0;
            float y_reading = Math.Abs(data.Acceleration.Y) > 0.02 ? data.Acceleration.Y : 0;

            FeedbackState = ControllerMapper.MapReadings(x_reading, y_reading);

            MainThread.BeginInvokeOnMainThread(() =>
            {

                UpdateUI(FeedbackState.acc, FeedbackState.str);
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

            private void UpdateUI(AccState acc, StrState str)
        {
            // 1. Dohvati boje iz resursa
            var ddc = (Color)Application.Current.Resources["DarkDarkCyan"];
            var dc = (Color)Application.Current.Resources["DarkCyan"];
            var c = (Color)Application.Current.Resources["Cyan"];

            var ddl = (Color)Application.Current.Resources["DarkDarkLime"];
            var dl = (Color)Application.Current.Resources["DarkLime"];
            var l = (Color)Application.Current.Resources["Lime"];

            // 2. Ugasi sve prije paljenja novih
            ResetBorders();

            // 3. Logika za gas (Acceleration) - Koristimo switch expression koji si naučio!
            switch (acc)
            {
                case AccState.FWD3: F3.BackgroundColor = c; goto case AccState.FWD2;
                case AccState.FWD2: F2.BackgroundColor = dc; goto case AccState.FWD1;
                case AccState.FWD1: F1.BackgroundColor = ddc; break;

                case AccState.BCK3: B3.BackgroundColor = c; goto case AccState.BCK2;
                case AccState.BCK2: B2.BackgroundColor = dc; goto case AccState.BCK1;
                case AccState.BCK1: B1.BackgroundColor = ddc; break;
            }

            // 4. Logika za skretanje (Steering)
            switch (str)
            {
                case StrState.RGT3: R3.BackgroundColor = l; goto case StrState.RGT2;
                case StrState.RGT2: R2.BackgroundColor = dl; goto case StrState.RGT1;
                case StrState.RGT1: R1.BackgroundColor = ddl; break;

                case StrState.LFT3: L3.BackgroundColor = l; goto case StrState.LFT2;
                case StrState.LFT2: L2.BackgroundColor = dl; goto case StrState.LFT1;
                case StrState.LFT1: L1.BackgroundColor = ddl; break;
            }

            // 5. Finalni string
            FeedbackLabel.Text = $"{acc}{str}";
        }

        private void ResetBorders()
        {
            var gray = (Color)Application.Current.Resources["Gray900"];
            var allBorders = new[] { F1, F2, F3, B1, B2, B3, L1, L2, L3, R1, R2, R3 };

            foreach (var border in allBorders)
            {
                border.BackgroundColor = gray;
            }
        }

    }
    }

