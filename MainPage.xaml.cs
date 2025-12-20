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

            // Initialize game objects
            myDisc = new Disc(0.5, 0.5, 80, (Color)Application.Current.Resources["Tertiary"]);
            myObstacle = new Obstacle(0.7, 0.7, 80, (Color)Application.Current.Resources["Danger"]);


            // Setup Accelerometer if supported
            if (Accelerometer.Default.IsSupported)
            {
                try
                {
                    
                    Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                    // Start Accelerometer with Game speed (about 20ms intervals)
                    Accelerometer.Default.Start(SensorSpeed.Game);

                    System.Diagnostics.Debug.WriteLine("INFO: Accelerometer running successfully!");
                }

                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: failed while running Accelerometer {ex.Message} ");
                }
            }
            // if not supported, log a warning
            else
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Accelerometer is not supported on this device!");
            }
        }
        // Layout SizeChanged event handler to get actual size and initialize game objects positions
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

                // Mark layout as initialized
                IsLayoutInitialized = true;
                Debug.WriteLine($"INFO: Layout initialized with size {LayoutWidth}x{LayoutHeight}");
            }
        }

        // Accelerometer ReadingChanged event handler to update game state
        private void Accelerometer_ReadingChanged(object? sender, AccelerometerChangedEventArgs e)
        {
            // Ensure layout is initialized before processing
            if (!IsLayoutInitialized) return;

            // Get accelerometer data
            var data = e.Reading;

            // Deadzone filtering (±0.02)
            float x_reading = Math.Abs(data.Acceleration.X) > 0.02 ? data.Acceleration.X : 0;
            float y_reading = Math.Abs(data.Acceleration.Y) > 0.02 ? data.Acceleration.Y : 0;

            // 1. PREDICTION: calculate predicted position based on current reading
            myDisc.MakePrediction(x_reading, y_reading);

            // 2. COLLISION DETECTION AND RESOLUTION with the obstacle
            myDisc.ResolveCollision(myObstacle, LayoutWidth, LayoutHeight);

            // 3. COMMIT: commit the move if no collision detected
            myDisc.CommitMove();

            // Recalculate DIU center for UI update
            myDisc.RecalculateDUICenter(LayoutWidth, LayoutHeight);

            // 4. UI UPDATE
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
                    // Unsubscribe from the event
                    Accelerometer.Default.ReadingChanged -= Accelerometer_ReadingChanged;
                    Debug.WriteLine("INFO: Accelerometer stopped.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR: failed while stopping Accelerometer {ex.Message} ");
                }
            }

        }
        // Method to draw obstacle on the screen
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
        // Method to draw disc on the screen
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
