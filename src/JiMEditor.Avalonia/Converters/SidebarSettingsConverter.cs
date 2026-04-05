// Ported from: JiMEditor/Common/Converters.cs — SettingsConverter (WPF) for SidebarListView toolbar enablement (2026-04-05).
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using JiME;

namespace JiMEditor.Avalonia.Converters;

/// <summary>
/// Enables remove / duplicate / export / settings when the sidebar selection is a real item (mirrors WPF SettingsConverter using SelectedValue / dataName).
/// </summary>
public sealed class SidebarSettingsConverter : IValueConverter
{
	public object? Convert( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		CanUseSettingsOnItem( value );

	public object? ConvertBack( object? value, Type targetType, object? parameter, CultureInfo culture ) =>
		throw new NotSupportedException();

	public static bool CanUseSettingsOnItem( object? value )
	{
		if ( value == null )
			return false;
		var name = GetDataName( value );
		return name.Length > 0 && name != "None" && name != "Random Event Trigger";
	}

	static string GetDataName( object value ) => value switch
	{
		ICommonData c => c.dataName ?? "",
		IInteraction i => i.dataName ?? "",
		Translation t => t.dataName ?? "",
		_ => ""
	};
}
