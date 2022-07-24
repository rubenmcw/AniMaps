// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Toolkit.UI.Controls;
using Esri.ArcGISRuntime.UI.Controls;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;
using System;
using System.Linq;

namespace ArcGISRuntime //Xamarin.Samples.ShowPopup
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Show popup",
        category: "Layers",
        description: "Show predefined popups from a web map.",
        instructions: "Tap on the features to prompt a popup that displays information about the feature.",
        tags: new[] { "feature", "feature layer", "popup", "toolkit", "web map" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("ShowPopup.xml")]
    public class ShowPopup : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private PopupViewer _popupViewer;
        private TextView _textView;
        private ToggleButton toggle;
        private Map myMap;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Show popup";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            //Map = new Map(BasemapStyle.ArcGISTopographic);
            myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create and set initial map location
            MapPoint initialLocation = new MapPoint(
                -13176752, 4090404, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 300000);
            //ToggleButton button = FindViewById<ToggleButton>(Resource.Id.toggle);
            toggle.Click += (o, e) => {
                // Perform action on clicks
                if (toggle.Checked)
                {
               
                    PopupViewer viewer = FindViewById<PopupViewer>(Resource.Id.popupViewer);
                    viewer.Visibility = Android.Views.ViewStates.Gone;


                }

                else
                {
                  
                    PopupViewer viewer = FindViewById<PopupViewer>(Resource.Id.popupViewer);
                    viewer.Visibility = Android.Views.ViewStates.Visible;
                }
                   
            };

            // Create uri to the used feature service
            Uri serviceUri = new Uri(
                "https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/arcgis/rest/services/TestAnimalLayer/FeatureServer/6");

            // Create new FeatureLayer from service uri and
            //FeatureLayer geologyLayer = new FeatureLayer(serviceUri);

            // Add created layer to the map
            //myMap.OperationalLayers.Add(geologyLayer);

            // Assign the map to the MapView
            //_myMapView.Map = myMap;
            MapViewModel();
        }
        
        public void MapViewModel()
        {

            _ = displayWebMap();

        }

        private async void MapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the feature layer from the map.
                FeatureLayer incidentLayer = _myMapView.Map.OperationalLayers.First() as FeatureLayer;

                // Identify the tapped on feature.
                IdentifyLayerResult result = await _myMapView.IdentifyLayerAsync(incidentLayer, e.Position, 12, true);

                if (result?.Popups?.FirstOrDefault() is Popup popup)
                {
                    if (toggle.Checked)
                    {
                        // Remove the instructions label.
                        //_textView.Visibility = Android.Views.ViewStates.Gone;
                        //_popupViewer.Visibility = Android.Views.ViewStates.Gone;
                        toggle.Checked = false;

                    }
         
                    // Remove the instructions label.
                    _textView.Visibility = Android.Views.ViewStates.Gone;
                    _popupViewer.Visibility = Android.Views.ViewStates.Visible;

                    // Create a new popup manager for the popup.
                    _popupViewer.PopupManager = new PopupManager(popup);

                    QueryParameters queryParams = new QueryParameters
                    {
                        // Set the geometry to selection envelope for selection by geometry.
                        Geometry = new Envelope((MapPoint)popup.GeoElement.Geometry, 6, 6)
                    };

                    // Select the features based on query parameters defined above.
                    await incidentLayer.SelectFeaturesAsync(queryParams, SelectionMode.New);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            //_myMapView = new MapView(this);
            //layout.AddView(_myMapView);

            // Show the layout in the app
            //SetContentView(layout);
            // Create a new vertical layout for the app.
            SetContentView(Resource.Layout.ShowPopup);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _popupViewer = FindViewById<PopupViewer>(Resource.Id.popupViewer);
            _textView = FindViewById<TextView>(Resource.Id.instructionsLabel);
            toggle = FindViewById<ToggleButton>(Resource.Id.toggle);
            toggle.Checked = true;
            

            // Add event handlers.
            _myMapView.GeoViewTapped += MapViewTapped;
        }
        private async Task displayWebMap()
        {
            // Create a portal. If a URI is not specified, www.arcgis.com is used by default.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();

            // Get the portal item for a web map using its unique item id.
            //PortalItem mapItem = await PortalItem.CreateAsync(portal, "41281c51f9de45edaf1c8ed44bb10e30");
            PortalItem mapItem = await PortalItem.CreateAsync(portal, "74291969e043466996340f9db4ad2263");
           

            // Create the map from the item.
            Map map = new Map(mapItem);

            // To display the map, set the MapViewModel.Map property, which is bound to the map view.
            this.myMap = map;
            _myMapView.Map = myMap;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unhook event handlers.
            _myMapView.GeoViewTapped -= MapViewTapped;
        }
    }
}