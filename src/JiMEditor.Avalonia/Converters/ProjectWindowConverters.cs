// Ported from: JiMEditor/Common/Converters.cs — ProjectTypeConverter, BoolToScrollbarVisibilityConverter (2026-04-04)

using System;
using System.Globalization;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using JiME;

namespace JiMEditor.Avalonia.Converters;

public class ProjectTypeToImageConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not ProjectType pt)
			return null;
		var name = pt == ProjectType.Campaign ? "ring.png" : "ring-silver.png";
		var uri = new Uri($"avares://JiMEditor.Avalonia/Assets/{name}");
		return new Bitmap(AssetLoader.Open(uri));
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}

public class BoolToScrollBarVisibilityConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value is true ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}
