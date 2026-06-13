using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using JiME.Procedural;

namespace JiMEditor.Avalonia.Converters;

public sealed class ProcLogTypeToBrushConverter : IValueConverter
{
	public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		value is ProceduralGeneratorContext.LogType type
			? type switch
			{
				ProceduralGeneratorContext.LogType.Warning => Brushes.Yellow,
				ProceduralGeneratorContext.LogType.Error => Brushes.Red,
				_ => Brushes.White
			}
			: Brushes.White;

	public object? ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		throw new NotSupportedException();
}
