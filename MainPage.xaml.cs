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
        private double SpeedFactor = 0.02;              //removed 'readable' because of new features

//      -fixed size of moving element (Disc) in device-indepent 
        private const double DiscSize = 50;

//      -bool flags to track sensor state
        private bool IsToggled = false;
        private bool IsInverted = false;



        public MainPage()
        {
            InitializeComponent();

             CheckSensor(this, EventArgs.Empty);


        }

        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;

//                                    --- 1. Deadzone Filtering ---
//         -If the tilt magnitude is too small (e.g., < 0.02 G), treat it as zero.
//         -This prevents the disc from shaking when the device is resting flat.
            float x_reading = Math.Abs(data.Acceleration.X) > 0.02 ? data.Acceleration.X : 0;
            float y_reading = Math.Abs(data.Acceleration.Y) > 0.02 ? data.Acceleration.Y : 0;


//          -something new: checking the state of inversion flag
            if (IsInverted)
            {
                x_reading = -x_reading;
                y_reading = -y_reading;
            }

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
                SpeedLabel.Text = $"{SpeedFactor:F2}";
            });
        }




//                  --- UI Event Handlers ---

//      Uptading speed label after changing speed factor
        private void UpdateSpeedLabel()
        {
            SpeedLabel.Text = $"{SpeedFactor:F2}";
        }

        //      Increasing speed factor and putting him in limits [0.01, 0.1]
        private void SpeedUp(object sender, EventArgs e)
        {
            SpeedFactor += 0.01;
            SpeedFactor = Math.Clamp(SpeedFactor, 0.01, 0.1);

            UpdateSpeedLabel();
        }

//      Decreasing speed factor and putting him in limits [0.01, 0.1]
        private void SpeedDown(object sender, EventArgs e)
        {
            SpeedFactor -= 0.01;
            SpeedFactor = Math.Clamp(SpeedFactor, 0.01, 0.1);

            UpdateSpeedLabel();
        }



        //      Toggling inversion of sensor values
        private void InvertValues(object sender, EventArgs e)
        {
            IsInverted = !IsInverted;
            if(IsInverted)
                InvertButton.BackgroundColor = (Color)Application.Current.Resources["Success"];
            
            else
                InvertButton.BackgroundColor = (Color)Application.Current.Resources["Danger"];
            
        }

//      Toggling sensor ON/OFF state
        private void ToggleSensor(object sender, EventArgs e)
        {
            IsToggled = !IsToggled;
            if (IsToggled)
            {
              
                ToggleButton.BackgroundColor = (Color)Application.Current.Resources["Success"];
            }
            else
            {
                
                ToggleButton.BackgroundColor = (Color)Application.Current.Resources["Danger"]; ;
            }

            CheckSensor(this, EventArgs.Empty);
        }

//      Checking sensor state and starting/stopping it accordingly
        private void CheckSensor(object sender, EventArgs e)
        {
            if (IsToggled)
            {
                try
                {
                    Accelerometer.Start(SensorSpeed.Game);
                    Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error starting accelerometer: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    Accelerometer.Stop();
                    Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping accelerometer: {ex.Message}");
                }
            }
        }
    }
}
