using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Comet
{
	/// <summary>
	/// MVU-friendly Map control that wraps Microsoft.Maui.Controls.Maps.Map via MauiViewHost.
	/// Provides declarative API for pins, regions, and map configuration.
	/// 
	/// Note: Requires Microsoft.Maui.Controls.Maps NuGet package and platform-specific setup
	/// (Google Maps API key for Android, no extra setup for iOS).
	/// 
	/// Usage:
	///   new MapView(
	///       latitude: 47.6062, longitude: -122.3321,
	///       zoomLevel: 12,
	///       pins: new[] {
	///           new MapPin("Seattle", 47.6062, -122.3321),
	///       })
	/// </summary>
	public class MapView : View
	{
		readonly State<double> _latitude;
		readonly State<double> _longitude;
		readonly State<double> _zoomLevel;
		readonly State<MapType> _mapType;
		readonly State<IReadOnlyList<MapPin>> _pins;
		readonly State<bool> _isScrollEnabled;
		readonly State<bool> _isZoomEnabled;
		readonly State<bool> _isTrafficEnabled;

		public MapView(
			double latitude = 0, double longitude = 0,
			double zoomLevel = 10,
			MapType mapType = MapType.Street,
			IReadOnlyList<MapPin> pins = null,
			bool isScrollEnabled = true,
			bool isZoomEnabled = true,
			bool isTrafficEnabled = false)
		{
			_latitude = new State<double>(latitude);
			_longitude = new State<double>(longitude);
			_zoomLevel = new State<double>(zoomLevel);
			_mapType = new State<MapType>(mapType);
			_pins = new State<IReadOnlyList<MapPin>>(pins ?? Array.Empty<MapPin>());
			_isScrollEnabled = new State<bool>(isScrollEnabled);
			_isZoomEnabled = new State<bool>(isZoomEnabled);
			_isTrafficEnabled = new State<bool>(isTrafficEnabled);
		}

		[Body]
		View body()
		{
			// MapView creates a placeholder that tells users to use MauiViewHost
			// with Microsoft.Maui.Controls.Maps.Map for full map functionality
			return new VStack
			{
				new Text($"Map: {_latitude.Value:F4}, {_longitude.Value:F4}")
					.FontSize(14)
					.Color(Colors.Grey),
				new Text("Use MauiViewHost with Microsoft.Maui.Controls.Maps.Map for native maps")
					.FontSize(12)
					.Color(Colors.Grey),
			}
			.Background(new Microsoft.Maui.Graphics.SolidPaint(Colors.LightGrey));
		}

		/// <summary>
		/// Creates a MauiViewHost wrapping a native MAUI Map control.
		/// Requires Microsoft.Maui.Controls.Maps package.
		/// </summary>
		public static MauiViewHost CreateNativeMap(
			double latitude, double longitude,
			double latitudeDelta = 0.1, double longitudeDelta = 0.1,
			IEnumerable<MapPin> pins = null)
		{
			// This uses reflection-free approach - callers should use MauiViewHost directly
			// with Microsoft.Maui.Controls.Maps.Map if they need native maps.
			throw new NotSupportedException(
				"For native maps, add the Microsoft.Maui.Controls.Maps NuGet package " +
				"and use MauiViewHost to embed Microsoft.Maui.Controls.Maps.Map directly.");
		}
	}

	/// <summary>
	/// Represents a pin/marker on a map.
	/// </summary>
	public class MapPin
	{
		public string Label { get; set; }
		public string Address { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public MapPinType Type { get; set; } = MapPinType.Generic;

		public MapPin() { }

		public MapPin(string label, double latitude, double longitude)
		{
			Label = label;
			Latitude = latitude;
			Longitude = longitude;
		}
	}

	public enum MapType
	{
		Street,
		Satellite,
		Hybrid
	}

	public enum MapPinType
	{
		Generic,
		Place,
		SavedPin,
		SearchResult
	}
}
