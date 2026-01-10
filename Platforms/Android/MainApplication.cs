using Android.App;
using Android.Runtime;
[assembly: UsesPermission(Android.Manifest.Permission.HighSamplingRateSensors)]
[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]

namespace TiltControl
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
