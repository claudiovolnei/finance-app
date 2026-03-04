using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;
using Plugin.Fingerprint;

namespace Finance.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Required by Plugin.Fingerprint on Android so it can display the biometric prompt.
        CrossFingerprint.SetCurrentActivityResolver(() => this);

        if (Window is not null)
        {
            WindowCompat.SetDecorFitsSystemWindows(Window, true);
        }
    }
}
