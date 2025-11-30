using Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using System;

namespace TiltControl
{
    public partial class MainPage : ContentPage
    {
//          --- State variables ---
        private double CurrentX = 0.5;
        private double CurrentY = 0.5;
        private readonly double SpeedFactor = 0.02;
        private const double DiscSize = 50;




        public MainPage()
        {
            InitializeComponent();

//          -for now, we will say "for sure, device has accelerometer sensors"
//          -turning on sensors automaticly
            Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Default.Start(SensorSpeed.Game);

        }

        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;

            float x_reading = Math.Abs(data.Acceleration.X) > 0.02 ? data.Acceleration.X : 0;
            float y_reading = Math.Abs(data.Acceleration.Y) > 0.02 ? data.Acceleration.Y : 0;

            CurrentX -= x_reading * SpeedFactor;
            CurrentY += y_reading * SpeedFactor;

            CurrentX = Math.Clamp(CurrentX, 0, 1);
            CurrentY = Math.Clamp(CurrentY, 0, 1);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AbsoluteLayout.SetLayoutBounds(Disc, new Rect(CurrentX, CurrentY, DiscSize, DiscSize));
            });
        }

       
    }
}
