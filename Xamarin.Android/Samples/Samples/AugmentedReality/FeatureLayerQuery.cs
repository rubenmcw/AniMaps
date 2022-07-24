using Android;
using Android.App;
using Android.Util;
using Android.Content;
using Android.Content.PM;
//using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
//using ArcGISRuntimeXamarin.Samples.ARToolkit.Controls;
//using Esri.ArcGISRuntime.ARToolkit;
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
//using Surface = Esri.ArcGISRuntime.Mapping.Surface;
//using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Android.Text;
//using Esri.ArcGISRuntime.Portal;

namespace ArcGISRuntime
{
    
    public class FeatureLayerQuery : Activity
	{
        private string _animalObservationsURL = "https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/arcgis/rest/services/AllAnimalRange_joined_final3/FeatureServer/0";
       

		private MapView _myMapView;

        private Map myMap;

		private EditText _queryTextBox;

		private ServiceFeatureTable _featureTable;

		private FeatureLayer _featureLayer;

		protected override void OnCreate(Bundle bundle) {

            base.OnCreate(bundle);
            Title = "Animal Finder";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();

        }

		private void Initialize() {

            // Create new Map with basemap
             myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create and set initial map location
            MapPoint initialLocation = new MapPoint(
                -11000000, 5000000, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 100000000);

            // Create feature table using a url
            _featureTable = new ServiceFeatureTable(new Uri(_animalObservationsURL));

            // Create feature layer using this feature table
            _featureLayer = new FeatureLayer(_featureTable)
            {
                // Set the Opacity of the Feature Layer
                Opacity = 0.6,
                // Work around service setting
                MaxScale = 10
            };

            // Create a new renderer for the States Feature Layer.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Transparent, lineSymbol);

            // Set States feature layer renderer
            _featureLayer.Renderer = new SimpleRenderer(fillSymbol);

            // Add feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);

            // Update the selection color
            _myMapView.SelectionProperties.Color = Color.Cyan;

            // Assign the map to the MapView
            _myMapView.Map = myMap;
            
        }
       

        private async void OnQueryClicked(object sender, EventArgs e) {

            // Remove any previous feature selections that may have been made
            _featureLayer.ClearSelection();

            // Begin query process
            await QueryStateFeature(_queryTextBox.Text);

        }

		private async Task QueryStateFeature(string stateName) {

            // Create dialog to display alert information
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            try
            {
                // Create a query parameters that will be used to Query the feature table
                QueryParameters queryParams = new QueryParameters
                {
                    // Construct and assign the where clause that will be used to query the feature table
                    WhereClause = "upper(Common_Name) LIKE '%" + stateName.Trim().ToUpper() + "%'"
                };

                // Query the feature table
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Cast the QueryResult to a List so the results can be interrogated.
                List<Feature> features = queryResult.ToList();

                if (features.Any())
                {
                    // Create an envelope.
                    EnvelopeBuilder envBuilder = new EnvelopeBuilder(SpatialReferences.WebMercator);

                    // Loop over each feature from the query result.
                    foreach (Feature feature in features)
                    {
                        // Add the extent of each matching feature to the envelope.
                        envBuilder.UnionOf(feature.Geometry.Extent);

                        // Select each feature.
                        _featureLayer.SelectFeature(feature);
                    }

                    // Zoom to the extent of the selected feature(s).
                    await _myMapView.SetViewpointGeometryAsync(envBuilder.ToGeometry(), 50);
                }
                else
                {
                    alert.SetTitle("Animal Not Found!");
                    alert.SetMessage("Please search an animal in the database");
                    alert.Show();
                }
            }
        
            catch (Exception ex)
            {
                alert.SetTitle("Sample Error");
                alert.SetMessage(ex.Message);
                alert.Show();
            }
        }

		private void CreateLayout() {

            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create new Text box that will take the query parameter
            _queryTextBox = new EditText(this)
            {
                InputType = InputTypes.ClassText | InputTypes.TextVariationNormal
            };
            _queryTextBox.SetMaxLines(1);

            // Create Button that will start the Feature Query
            Button queryButton = new Button(this)
            {
                Text = "Search"
            };
            queryButton.Click += OnQueryClicked;

            // Create and add a help label
            TextView helpLabel = new TextView(this)
            {
                Text = "Enter the name of the animal you are looking, then press 'Search'"
            };
            layout.AddView(helpLabel);

            // Add TextBox to the layout
            layout.AddView(_queryTextBox);

            // Add Button to the layout
            layout.AddView(queryButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);

        }
	}
}
