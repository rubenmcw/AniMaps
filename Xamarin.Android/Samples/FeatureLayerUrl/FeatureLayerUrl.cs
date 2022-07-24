// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;
using System;

namespace ArcGISRuntime //.Samples.FeatureLayerUrl
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature layer (feature service)",
        category: "Layers",
        description: "Show features from an online feature service.",
        instructions: "Run the sample and view the feature service as an operational layer on top of the basemap. Zoom and pan around the map to see the features in greater detail.",
        tags: new[] { "feature table", "layer", "layers", "service" })]
    public class FeatureLayerUrl : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;
        private Map myMap;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Feature layer (feature service)";

            // Create the UI, setup the control references and execute initialization 
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

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
        private async Task displayWebMap()
        {
            // Create a portal. If a URI is not specified, www.arcgis.com is used by default.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();

            // Get the portal item for a web map using its unique item id.
            //PortalItem mapItem = await PortalItem.CreateAsync(portal, "41281c51f9de45edaf1c8ed44bb10e30");
            PortalItem mapItem = await PortalItem.CreateAsync(portal, "4974ce8cecec453296d20e81f95c2db4");
            // Create the map from the item.
            Map map = new Map(mapItem);

            // To display the map, set the MapViewModel.Map property, which is bound to the map view.
            this.myMap = map;
            _myMapView.Map = myMap;
        }
    }
}