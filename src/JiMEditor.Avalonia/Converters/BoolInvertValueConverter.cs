using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace JiMEditor.Avalonia.Converters;

/// <summary>Inverts bool (e.g. enable summary edit when skipSummary is false).</summary>
public sealed class BoolInvertValueConverter : IValueConverter
{
	public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		value is bool b ? !b : value;

	public object? ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		throw new NotSupportedException();
}
