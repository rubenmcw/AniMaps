using Android;
using Android.App;
using Android.Util;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
//using ArcGISRuntimeXamarin.Samples.ARToolkit.Controls;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Java.Nio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntime 
{
   
    public class CollectDataAR : Activity, IDialogInterfaceOnCancelListener
    {
       
        // Hold references to UI controls.
        private ARSceneView _arView;
        private string CHANNEL_ID;
        private TextView _helpLabel;
        private Button _addButton;
        private View _calibrationView;


        // Scene content.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;
        private Scene _scene;

        // Track when user is changing between AR and GPS localization.
        private bool _changingScale;

        // Create a new copmletion source for the prompt.
        private TaskCompletionSource<int> _healthCompletionSource;
        private TaskCompletionSource<string> nameCompletionSource;

      
        private ServiceFeatureTable _featureTable = new ServiceFeatureTable(new Uri("https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/ArcGIS/rest/services/Collection_Layer_2/FeatureServer/0"));

        // Graphics for tapped points in the scene.
        private GraphicsOverlay _graphicsOverlay;
        private SimpleMarkerSceneSymbol _tappedPointSymbol = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Diamond, System.Drawing.Color.Orange, 0.5, 0.5, 0.5, SceneSymbolAnchorPosition.Center);

        // Custom location data source that enables calibration and returns values relative to mean sea level rather than the WGS84 ellipsoid.
        private MSLAdjustedARLocationDataSource _locationDataSource;

        // Calibration state fields.
        private bool _isCalibrating;
        private double _altitudeOffset;

        // Permissions and permission request.
        private readonly string[] _requestedPermissions = { Manifest.Permission.AccessFineLocation };
        private const int requestCode = 35;

        private void RequestPermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, _requestedPermissions[0]) == Permission.Granted)
            {
                Initialize();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, _requestedPermissions, CollectDataAR.requestCode);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == CollectDataAR.requestCode && grantResults[0] == Permission.Granted)
            {
                Initialize();
            }
            else
            {
                Toast.MakeText(this, "Location permissions needed for this sample", ToastLength.Short).Show();
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private bool IsCalibrating
        {
            get
            {
                return _isCalibrating;
            }
            set
            {
                _isCalibrating = value;
                if (_isCalibrating)
                {
                    // Show the surface semitransparent for calibration.
                    _scene.BaseSurface.Opacity = 0.5;

                    // Enable scene interaction.
                    _arView.InteractionOptions.IsEnabled = true;
                    _calibrationView.Visibility = ViewStates.Visible;
                }
                else
                {
                    // Hide the scene when not calibrating.
                    _scene.BaseSurface.Opacity = 0;

                    // Disable scene interaction.
                    _arView.InteractionOptions.IsEnabled = false;
                    _calibrationView.Visibility = ViewStates.Gone;
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            CreateNotificationChannel();
            base.OnCreate(savedInstanceState);

            Title = "Collect data in AR";

            // Create the layout.
            CreateLayout();

            // Request permissions for location.
            RequestPermissions();
        }

        private void CreateLayout()
        {
            // Load the view.
            SetContentView(ArcGISRuntime.Resource.Layout.CollectDataAR);

            // Set up control references.
            _arView = FindViewById<ARSceneView>(ArcGISRuntime.Resource.Id.arView);
            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.helpLabel);
            //_calibrateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.calibrateButton);
            _addButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.addTreeButton);
            //_roamingButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.roamingButton);
            //_localButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.localButton);
            //_calibrationView = FindViewById(ArcGISRuntime.Resource.Id.calibrationView);
            //_headingSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.headingJoystick);
            //_altitudeSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.altitudeJoystick);

            // Disable plane rendering and visualization.
            //_arView.ArSceneView.PlaneRenderer.Enabled = false;
            //_arView.ArSceneView.PlaneRenderer.Visible = false;

            // Configure button click events.
            _addButton.Click += AddButtonPressed;
            //_calibrateButton.Click += (o, e) => IsCalibrating = !IsCalibrating;
            //_roamingButton.Click += (o, e) => RealScaleValueChanged(true);
            //_localButton.Click += (o, e) => RealScaleValueChanged(false);

            // Configure calibration sliders.
            //_headingSlider.DeltaProgressChanged += HeadingSlider_DeltaProgressChanged;
            //_altitudeSlider.DeltaProgressChanged += AltitudeSlider_DeltaProgressChanged;
        }
       
        public void callNotify()
        {
            // Instantiate the builder and set notification elements:
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("Dangerous Animal Alert!")
                .SetContentText("A dangerous animal has been spotted in your area!")
                .SetSmallIcon(Resource.Drawable.animaps_logo);

            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }
            CHANNEL_ID = "4";
          
            var channel = new NotificationChannel(CHANNEL_ID, "Dangerous Animal Alert", NotificationImportance.Default)
            {
                Description = "A dangerous animal has been spotted in your area!"
                //Description = channelDescription
            };

            var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

    

        private void Initialize()
        {
            // Create the custom location data source and configure the AR scene view to use it.
            _locationDataSource = new MSLAdjustedARLocationDataSource(this);
            _locationDataSource.AltitudeMode = MSLAdjustedARLocationDataSource.AltitudeAdjustmentMode.NmeaParsedMsl;
            _arView.LocationDataSource = _locationDataSource;

            // Create the scene and show it.
            _scene = new Scene(BasemapStyle.ArcGISImageryStandard);
            _arView.Scene = _scene;

            // Create and add the elevation surface.
            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            _arView.Scene.BaseSurface = _elevationSurface;

            // Hide the surface in AR.
            _elevationSurface.NavigationConstraint = NavigationConstraint.None;
            _elevationSurface.Opacity = 0;

            // Configure the space and atmosphere effects for AR.
            _arView.SpaceEffect = SpaceEffect.None;
            _arView.AtmosphereEffect = AtmosphereEffect.None;

            // Add a graphics overlay for displaying points in AR.
            _graphicsOverlay = new GraphicsOverlay();
            _graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _graphicsOverlay.Renderer = new SimpleRenderer(_tappedPointSymbol);
            _arView.GraphicsOverlays.Add(_graphicsOverlay);

            // Add the exisiting features to the scene.
            FeatureLayer treeLayer = new FeatureLayer(_featureTable);
            treeLayer.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _arView.Scene.OperationalLayers.Add(treeLayer);

            // Add the event for the user tapping the screen.
            _arView.GeoViewTapped += arViewTapped;

            // Disable scene interaction.
            _arView.InteractionOptions = new SceneViewInteractionOptions() { IsEnabled = false };

            // Enable the calibrate button.
            //_calibrateButton.Enabled = true;
        }

        private void arViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Don't add features when calibrating the AR view.
            if (_isCalibrating)
            {
                return;
            }

            // Try to get the real-world position of that tapped AR plane.
            MapPoint planeLocation = _arView.ARScreenToLocation(e.Position);

            // Remove any existing graphics.
            _graphicsOverlay.Graphics.Clear();

            // Check if a Map Point was identified.
            if (planeLocation != null)
            {
                // Add a graphic at the tapped location.
                _graphicsOverlay.Graphics.Add(new Graphic(planeLocation));
                _addButton.Enabled = true;
                _helpLabel.Text = "Placed relative to ARCore plane";
            }
            else
            {
                ShowMessage("Didn't find anything, try again.", "Error");
                _addButton.Enabled = false;
            }
        }

        private async void AddButtonPressed(object sender, EventArgs e)
        {
            // Check if the user has already tapped a point.
            if (!_graphicsOverlay.Graphics.Any())
            {
                ShowMessage("Didn't find any animals, try again.", "Error");
                return;
            }
            try
            {
                // Prevent the user from changing the tapped feature.
                _arView.GeoViewTapped -= arViewTapped;

                // Prompt the user for the health value of the tree.
                int healthValue = await GetTreeHealthValue();

                string animalName = await GetAnimalName();

                // Get the camera image for the frame.
                Image capturedImage = _arView.ArSceneView.ArFrame.AcquireCameraImage();

                if (capturedImage != null)
                {
                    // Create a new ArcGIS feature and add it to the feature service.
                    await doFeature(capturedImage, healthValue, animalName);
                }
                else
                {
                    ShowMessage("Didn't get image for tap.", "Error");
                }
                //await doFeature(healthValue);
            }
            // This exception is thrown when the user cancels out of the prompt.
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                // Restore the event listener for adding new features.
                _arView.GeoViewTapped += arViewTapped;
            }
        }
       
        private async Task<string> GetAnimalName()
        { 
            EditText et = new EditText(this);
            AlertDialog.Builder ad = new AlertDialog.Builder(this);
            ad.SetTitle("Enter animal species: ");
            ad.SetView(et);
            ad.SetPositiveButton("OK", (sender, e) => { nameCompletionSource.TrySetResult(et.Text); });
       
            ad.Show();

            // Get the selected terminal.
            nameCompletionSource = new TaskCompletionSource<string>();
            string selectedIndex = await nameCompletionSource.Task;
            return selectedIndex;

        }
       


        private async Task<int> GetTreeHealthValue()
        {
            // Create UI for animal threat selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Is this animal a threat to public safety?");
            builder.SetItems(new string[] { "Yes", "No"}, Choose_Click);
            builder.SetOnCancelListener(this);
            builder.Show();
            string tag = "myapp";

            // Get the selected terminal.
            _healthCompletionSource = new TaskCompletionSource<int>();
            int selectedIndex = await _healthCompletionSource.Task;

            // Return a tree health value based on the users selection.
            switch (selectedIndex)
            {
                case 0: // is a threat.
                    //string tag = "myapp";
                    //callNotify();
                    //Log.Warn(tag, "2");
                    return 2;

                case 1: // is not a threat.
                    //callNotify();
                    //Log.Warn(tag, "1");
                    return 1;

                default:
                    return 0;
            }
        }

        private void Choose_Click(object sender, DialogClickEventArgs e)
        {
            _healthCompletionSource.TrySetResult(e.Which);
        }

        public void OnCancel(IDialogInterface dialog)
        {
            _healthCompletionSource.TrySetCanceled();
        }

        private async Task doFeature(Image capturedImage, int healthValue, string animalName)
       // private async Task doFeature(int healthValue)
        {
            _helpLabel.Text = "Adding feature...";

            try
            {
                
                // Get the geometry of the feature.
                MapPoint featurePoint = _graphicsOverlay.Graphics.First().Geometry as MapPoint;
                ArcGISFeature feature = (ArcGISFeature)_featureTable.CreateFeature();
                DateTime date = DateTime.Now; // will give the date time for today
                string sdate = date.ToString();
 
             
             
                feature.Geometry = featurePoint;

                // Set feature attributes.
                feature.SetAttributeValue("CollectionDate", sdate);
                feature.SetAttributeValue("AnimalSpecies", animalName);
               

                feature.SetAttributeValue("Threat", healthValue.ToString()) ;

                
                // Convert the Image from ARCore into a JPEG byte array.
                byte[] attachmentData = await ConvertImageToJPEG(capturedImage);

                //send image to AI

                //return name of animal
                //    |
                //    V

                animalName.Replace(" ", "_");
                //Log.Warn("helo: ", animalName);
                string fileName = animalName + ".jpg";

                // Add the attachment.
                // The contentType string is the MIME type for JPEG files, image/jpeg.
                await feature.AddAttachmentAsync(fileName, "image/jpeg", attachmentData);

                if(healthValue == 2)
                {
                    callNotify();
                }

                // Add the newly created feature to the feature table.
                await _featureTable.AddFeatureAsync(feature);
                //feature.Refresh();

                // Apply the edits to the service feature table.
                await _featureTable.ApplyEditsAsync();

                Toast.MakeText(this, "Added Animal successfully!", ToastLength.Long).Show();

                //Toast.MakeText(getContext(), "This is my Toast message!",Toast.LENGTH_LONG).show();

                // Reset the user interface.
                _helpLabel.Text = "Tap to create a feature";
                _graphicsOverlay.Graphics.Clear();
                _addButton.Enabled = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ShowMessage("Could not add animal to feature", "Error");
                _helpLabel.Text = "Animal not added";
            }
        }

        // This method uses code from this StackOverflow thread. https://stackoverflow.com/a/51521388
        private async Task<byte[]> ConvertImageToJPEG(Image capturedImage)
        {
            // Convert the image into a byte array.
            ByteBuffer yBuffer = capturedImage.GetPlanes()[0].Buffer;
            ByteBuffer uBuffer = capturedImage.GetPlanes()[1].Buffer;
            ByteBuffer vBuffer = capturedImage.GetPlanes()[2].Buffer;

            int ySize = yBuffer.Remaining();
            int uSize = uBuffer.Remaining();
            int vSize = vBuffer.Remaining();

            // Make a byte array large enough to store the buffers from all three planes.
            byte[] nv21ByteArray = new byte[ySize + uSize + vSize];

            // Load the byte array using the byte buffers.
            yBuffer.Get(nv21ByteArray, 0, ySize);
            vBuffer.Get(nv21ByteArray, ySize, vSize);
            uBuffer.Get(nv21ByteArray, ySize + vSize, uSize);

            // Rotate the NV21 byte array 90 degrees to the right.
            byte[] rotatedNV21 = RotateNV21ImageRight(nv21ByteArray, capturedImage.Width, capturedImage.Height);

            // Create a YuvImage using the NV21 image
            Android.Graphics.YuvImage yuv = new Android.Graphics.YuvImage(rotatedNV21, Android.Graphics.ImageFormatType.Nv21, capturedImage.Height, capturedImage.Width, null);

            // Convert the YuvImage into a jpeg.
            System.IO.Stream jpegStream = new System.IO.MemoryStream();
            await yuv.CompressToJpegAsync(new Android.Graphics.Rect(0, 0, yuv.Width, yuv.Height), 100, jpegStream);

            // Store the jpeg data in a byte array.
            jpegStream.Position = 0;
            byte[] jpegArray = new byte[jpegStream.Length];
            jpegStream.Read(jpegArray, 0, jpegArray.Length);
            return jpegArray;
        }

        // This method uses code modified from this Stack Overflow thread. https://stackoverflow.com/a/31425229
        private byte[] RotateNV21ImageRight(byte[] input, int width, int height)
        {
            byte[] output = new byte[input.Length];
            int frameSize = width * height;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    int yIn = j * width + i;
                    int uIn = frameSize + (j >> 1) * width + (i & ~1);
                    int vIn = uIn + 1;

                    int iOut = height - j - 1;

                    int yOut = i * height + iOut;
                    int uOut = frameSize + (i >> 1) * height + (iOut & ~1);
                    int vOut = uOut + 1;

                    output[yOut] = (byte)(0xff & input[yIn]);
                    output[uOut] = (byte)(0xff & input[uIn]);
                    output[vOut] = (byte)(0xff & input[vIn]);
                }
            }
            return output;
        }

        protected override async void OnPause()
        {
            base.OnPause();
            await _arView.StopTrackingAsync();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            // Resume AR tracking.
            await _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
        }

        private void ShowMessage(string message, string title, bool closeApp = false)
        {
            // Show a message and then exit after if needed.
            AlertDialog dialog = new AlertDialog.Builder(this).SetMessage(message).SetTitle(title).Create();
            if (closeApp)
            {
                dialog.SetButton("OK", (o, e) =>
                {
                    Finish();
                });
            }
            dialog.Show();
        }
    }
}