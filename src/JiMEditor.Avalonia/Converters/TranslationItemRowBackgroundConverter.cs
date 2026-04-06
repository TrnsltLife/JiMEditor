using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using JiME;

namespace JiMEditor.Avalonia.Converters;

public sealed class TranslationItemRowBackgroundConverter : IValueConverter
{
	public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture )
	{
		if ( value is TranslationItem ti )
		{
			if ( ti.superfluous )
				return new SolidColorBrush( Color.Parse( "#808080" ) );
			if ( ti.translationOK )
				return new SolidColorBrush( Color.Parse( "#228B22" ) );
			return new SolidColorBrush( Color.Parse( "#FF8C00" ) );
		}

		return Brushes.Transparent;
	}

	public object? ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		throw new NotSupportedException();
}
