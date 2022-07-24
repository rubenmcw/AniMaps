using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.Content;
//using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Managers;
//using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;
using Google.AR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntime
{
    [Activity(Label = "AniMaps", MainLauncher = true, Icon = "@drawable/animaps_logo")]
    public class MainActivity : Activity
    {
        public class NotificationEventArgs : EventArgs
        {
            public string Title { get; set; }
            public string Message { get; set; }
        }
        protected override void OnCreate(Bundle bundle)
        {

            ApiKeyManager.ArcGISDeveloperApiKey = "AAPK7585a902340b429d91f44d904deff5d39sR35U77N41SEL1LucBfpwoZv1YTce1sPkWuUEH-LWxyoB4740OhTytFgFTgmz1V";
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.HomeScreen);

            try
            {  
                Button viewMapButton = FindViewById<Button>(Resource.Id.viewAniMap);
                Button takePictureButton = FindViewById<Button>(Resource.Id.takePicture);
                Button viewNewMap = FindViewById<Button>(Resource.Id.navToMap);
                viewNewMap.Click += (s, e) => navToNew();
                viewMapButton.Click += (s, e) => navToMap();
                takePictureButton.Click += (s, e) => navToCam();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void navToNew()
        {
            var newActivity = new Intent(this, typeof(FeatureLayerQuery));
            StartActivity(newActivity);
        }
        private void navToCam()
        {
            var camActivity = new Intent(this, typeof(CollectDataAR));
            StartActivity(camActivity);
        }
        private void navToMap()
        {
            var mapActivity = new Intent(this, typeof(ShowPopup));
            StartActivity(mapActivity);
        }

        protected override void OnResume()
        {
            // Garbage collect when sample is closed.
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            Java.Lang.JavaSystem.Gc();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            Java.Lang.JavaSystem.Gc();
            base.OnResume();
        }
        
    }
}