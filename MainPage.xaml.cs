using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Layouts;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TiltControl.Models;

namespace TiltControl
{
    public partial class MainPage : ContentPage
    {
        private double LayoutWidth;
        private double LayoutHeight;
        private bool IsLayoutInitialized = false;

        Disc myDisc; 
        Obstacle myObstacle;

        public BoxView DiscUIElement;

        public MainPage()
        {
            InitializeComponent();

            myDisc = new Disc(0.5, 0.5, 80, Colors.Blue);
            myObstacle = new Obstacle(0.7, 0.7, 80, Colors.Red);


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

        private void GameLayout_SizeChanged(object sender, EventArgs e)
        {
            if (sender is AbsoluteLayout GameLayout && !IsLayoutInitialized)
            {
                LayoutWidth = GameLayout.Width;
                LayoutHeight = GameLayout.Height;
                
                myObstacle.RecalculateDUICenter(LayoutWidth, LayoutHeight);
                DrawObstacleOnScreen(myObstacle);

                myDisc.RecalculateDUICenter(LayoutWidth, LayoutHeight);
                DrawDiscOnScreen(myDisc);

                IsLayoutInitialized = true;
                Debug.WriteLine($"INFO: Layout initialized with size {LayoutWidth}x{LayoutHeight}");
            }
        }

        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            if (!IsLayoutInitialized) return;


            var data = e.Reading;


            float x_reading = Math.Abs(data.Acceleration.X) > 0.02 ? data.Acceleration.X : 0;
            float y_reading = Math.Abs(data.Acceleration.Y) > 0.02 ? data.Acceleration.Y : 0;

            myDisc.MakePrediction(x_reading, y_reading);

           if(Disc.CheckCollision(myDisc, myObstacle, LayoutWidth, LayoutHeight))
            
                myDisc.Effect();
            
        
           
             myDisc.CommitMove();
             myDisc.RecalculateDUICenter(LayoutWidth, LayoutHeight);



            MainThread.BeginInvokeOnMainThread(() =>
            {
                AbsoluteLayout.SetLayoutBounds(DiscUIElement,
                    new Rect(myDisc.X, myDisc.Y, myDisc.ObjectSize, myDisc.ObjectSize));
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

        private void DrawObstacleOnScreen(Obstacle o)
        {
            var box = new BoxView
            {
                Color = o.DiscColor,
                BackgroundColor = Colors.Transparent,
                CornerRadius = o.ObjectRadius,
                WidthRequest = o.ObjectSize,
                HeightRequest = o.ObjectSize
            };
            
            AbsoluteLayout.SetLayoutBounds(box, new Rect(o.X, o.Y, o.ObjectSize, o.ObjectSize));
            AbsoluteLayout.SetLayoutFlags(box, AbsoluteLayoutFlags.PositionProportional);
            GameLayout.Children.Add(box);
        }

        public void DrawDiscOnScreen(Disc d)
        {
          
            DiscUIElement = new BoxView
            {
                Color = d.DiscColor,
                BackgroundColor = Colors.Transparent,
                WidthRequest = d.ObjectSize,
                HeightRequest = d.ObjectSize,
                CornerRadius = d.ObjectRadius
            };

          
            AbsoluteLayout.SetLayoutBounds(DiscUIElement, new Rect(d.X, d.Y, d.ObjectSize, d.ObjectSize));
            AbsoluteLayout.SetLayoutFlags(DiscUIElement, AbsoluteLayoutFlags.PositionProportional);

            GameLayout.Children.Add(DiscUIElement);
        }
    }
}
