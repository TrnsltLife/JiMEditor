using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using JiME;

namespace JiMEditor.Avalonia.Converters;

public sealed class SelectedIndexToBoolConverter : IValueConverter
{
	public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		value is int idx && idx >= 0;

	public object? ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		throw new NotImplementedException();
}

public sealed class TileCollectionTextConverter : IValueConverter
{
	public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
	{
		if ( value == null )
			return "";

		int tileNumber = 0;
		if ( value is HexTile hex )
			tileNumber = hex.idNumber;
		else if ( value is int i )
			tileNumber = i;
		else
			return "";

		var collection = Collection.FromTileNumber( tileNumber );
		return collection == null ? $"{tileNumber}" : $"{tileNumber}{collection.FontCharacter}";
	}

	public object? ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		throw new NotImplementedException();
}

public sealed class BoolToSelectionBrushConverter : IValueConverter
{
	static readonly SolidColorBrush SelectedBrush = new( Colors.MediumSeaGreen );
	static readonly SolidColorBrush UnselectedBrush = new( Color.FromRgb( 70, 70, 74 ) );

	public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		value is bool b && b ? SelectedBrush : UnselectedBrush;

	public object? ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		throw new NotImplementedException();
}
