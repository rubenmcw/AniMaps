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
using ArcGISRuntimeXamarin.Samples.ARToolkit.Controls;
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Android.Text;

namespace ArcGISRuntime
{
	public class RenderUniqueValues : Activity 
	{

		// Hold a reference to the map view
		private MapView _myMapView;
		private LinearLayout _layerListView;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			Title = "Animal Ranges";

			CreateLayout();
			Initialize();
		}

		private void Initialize()
        {

			// Create new Map with basemap
			Map myMap = new Map(BasemapStyle.ArcGISTopographic);

			// Create uri to the used feature service
			Uri serviceUri = new Uri("https://services8.arcgis.com/LLNIdHmmdjO2qQ5q/arcgis/rest/services/AllAnimalRange_joined_final3/FeatureServer/0");

			// Create service feature table
			ServiceFeatureTable rangeFeatureTable = new ServiceFeatureTable(serviceUri);

			// Create a new feature layer using the service feature table
			FeatureLayer rangesLayer = new FeatureLayer(rangeFeatureTable);

			// Create a new unique value renderer
			UniqueValueRenderer rangeRenderer = new UniqueValueRenderer();

			// Add the "Common Name" field to the renderer
			rangeRenderer.FieldNames.Add("Common_Name");

			// Define a line symbol to use for the range fill symbols
			SimpleLineSymbol rangeOutlineSymbol = new SimpleLineSymbol(
				SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 0.7);

			// Define distinct fill symbols for a few regions (use the same outline symbol)
			SimpleFillSymbol redTailedHawkFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.Blue, rangeOutlineSymbol);
			SimpleFillSymbol californiaGroundSquirrelFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.LawnGreen, rangeOutlineSymbol);
			SimpleFillSymbol coyoteFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.SandyBrown, rangeOutlineSymbol);
			SimpleFillSymbol desertCottontailFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.Red, rangeOutlineSymbol);
			SimpleFillSymbol esbLizardFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.LightBlue, rangeOutlineSymbol);
			SimpleFillSymbol foxSquirrelFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.Azure, rangeOutlineSymbol);
			SimpleFillSymbol gopherSnakeFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.WhiteSmoke, rangeOutlineSymbol);
			SimpleFillSymbol muleDeerFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.RosyBrown, rangeOutlineSymbol);
			SimpleFillSymbol alligatorLizardFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.PeachPuff, rangeOutlineSymbol);
			SimpleFillSymbol rattlesnakeFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Solid, System.Drawing.Color.Yellow, rangeOutlineSymbol);


			// Add values to the renderer: define the label, description, symbol, and attribute value for each
			regionRenderer.UniqueValues.Add(
				new UniqueValue("Red-Tailed Hawk", "Red-Tailed Hawk", redTailedHawkFillSymbol, "Red-Tailed Hawk"));
			regionRenderer.UniqueValues.Add(
				new UniqueValue("California Ground Squirrel", "California Ground Squirrel", californiaGroundSquirrelFillSymbol, "California Ground Squirrel"));
			regionRenderer.UniqueValues.Add(
				new UniqueValue("Coyote", "Coyote", coyoteFillSymbol, "Coyote"));
			regionRenderer.UniqueValues.Add(
				new UniqueValue("Desert Cottontail", "Desert Cottontail", desertCottontailFillSymbol, "Desert Cottontail"));
			regionRenderer.UniqueValues.Add(S
				new UniqueValue("Eastern Side-Blotched Lizard", "Eastern Side-Blotched Lizard", esbLizardFillSymbol, "Eastern Side-Blotched Lizard"));
			regionRenderer.UniqueValues.Add(
				new UniqueValue("Fox Squirrel", "Fox Squirrel", foxSquirrelFillSymbol, "Fox Squirrel"));
			regionRenderer.UniqueValues.Add(
				new UniqueValue("Gopher Snake", "Gopher Snake", gopherSnakeFillSymbol, "Gopher Snake"));
			regionRenderer.UniqueValues.Add(
				new UniqueValue("Mule Deer", "Mule Deer", muleDeerFillSymbol, "Mule Deer"));
			regionRenderer.UniqueValues.Add(
				new UniqueValue("Southern Alligator Lizard", "Southern Alligator Lizard", alligatorLizardFillSymbol, "Southern Alligator Lizard"));
			regionRenderer.UniqueValues.Add(
				new UniqueValue("Western Rattlesnake", "Western Rattlesnake", rattlesnakeFillSymbol, "Western Rattlesnake"));

			// Set the default region fill symbol (transparent with no outline) for regions not explicitly defined in the renderer
			SimpleFillSymbol defaultFillSymbol = new SimpleFillSymbol(
				SimpleFillSymbolStyle.Cross, System.Drawing.Color.Gray, null);
			rangeRenderer.DefaultSymbol = defaultFillSymbol;
			rangeRenderer.DefaultLabel = "Other";

			// Apply the unique value renderer to the states layer
			rangesLayer.Renderer = rangeRenderer;

			// Add created layer to the map
			myMap.OperationalLayers.Add(rangesLayer);

			// Assign the map to the MapView
			_myMapView.Map = myMap;
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
	}
}
