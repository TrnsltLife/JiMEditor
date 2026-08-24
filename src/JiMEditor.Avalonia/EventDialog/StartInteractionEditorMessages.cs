// Ported from: JiMEditor/MainWindow.xaml.cs Starting Position guards (2026-08-23).
using JiME;

namespace JiMEditor.Avalonia.EventDialog;

/// <summary>WPF copy for the built-in Starting Position event, which has no editor.</summary>
public static class StartInteractionEditorMessages
{
	public const string CannotEditBody =
		"You can't edit the Starting Position event. But you can place the Start token on the Starting Tile in Tile Editor -> Token Editor.";
	public const string CannotEditTitle = "Cannot Edit Starting Position Event";

	public const string CannotDuplicateBody = "You can't duplicate the Starting Position interaction.";
	public const string CannotDuplicateTitle = "Cannot Duplicate Starting Position Event";

	public const string CannotImportBody = "You can't import a Starting Position event.";
	public const string CannotImportTitle = "Cannot Edit Starting Position Event";

	public static bool TryWarnCannotEdit( IInteraction? interaction )
	{
		if ( interaction is not StartInteraction )
			return false;
		MessageBox.Show( CannotEditBody, CannotEditTitle, MessageBoxButton.OK, MessageBoxImage.Information );
		return true;
	}

	public static bool TryWarnCannotDuplicate( IInteraction? interaction )
	{
		if ( interaction is not StartInteraction )
			return false;
		MessageBox.Show( CannotDuplicateBody, CannotDuplicateTitle, MessageBoxButton.OK, MessageBoxImage.Information );
		return true;
	}

	public static bool TryWarnCannotImport( IInteraction? interaction )
	{
		if ( interaction is not StartInteraction )
			return false;
		MessageBox.Show( CannotImportBody, CannotImportTitle, MessageBoxButton.OK, MessageBoxImage.Information );
		return true;
	}
}
