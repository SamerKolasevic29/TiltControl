using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using System;

namespace TiltControl
{
    public partial class MainPage : ContentPage
    {
//                                             --- State variables ---
//          current proportional position (0.5 = center)
        private double CurrentX = 0.5;
        private double CurrentY = 0.5;

//      -defines the rate of movement (sensitivity to tilt)
//      -Higher value means faster acceleration/ movement
        private readonly double SpeedFactor = 0.02;

//      -fixed size of moving element (Disc) in device-indepent 
        private const double DiscSize = 50;



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

//                              --- 2. Position Accumulation (Physics Logic) ---
//          The tilt (reading) determines the continuous acceleration (velocity change).
//          -X-axis: Tilting right (positive Accel.X) decreases proportional X coordinate (Screen coordinates typically grow right).
//          -Y-axis: Tilting towards the user (positive Accel.Y) increases proportional Y coordinate (Screen coordinates grow down).
            CurrentX -= x_reading * SpeedFactor;
            CurrentY += y_reading * SpeedFactor;

//                          --- 3. Boundary Clamping ---
//          -Ensuring the calculated position stays within the proportional range [0, 1].
//          -This prevents the disc from flying off the screen edges.
            CurrentX = Math.Clamp(CurrentX, 0, 1);
            CurrentY = Math.Clamp(CurrentY, 0, 1);


//                                  --- 4. UI Update on Main Thread ---
//          -All UI modifications must be executed on the main thread (MainThread.BeginInvokeOnMainThread).
            MainThread.BeginInvokeOnMainThread(() =>
            {
//              Moving the disc with new calculating values in-time
                AbsoluteLayout.SetLayoutBounds(Disc, new Rect(CurrentX, CurrentY, DiscSize, DiscSize));

//              Displaying the original values from sensors
                SensorXLabel.Text = $"{x_reading:F2} G";
                SensorYLabel.Text = $"{y_reading:F2} G";

//              formatting moving values in range [-100,100]
//              but these sensors have inverted quadrants unlike the well-known ones from mathematics
                int DisplayX = (int)((CurrentX - 0.5) * 200);
                int DisplayY = (int)((CurrentY - 0.5) * 200);

//              and displaying them on screen
                CoordXLabel.Text = DisplayX.ToString("+0;-0;0");
                CoordYLabel.Text = DisplayY.ToString("+0;-0;0");
            });
        }

       
    }
}
