using System;
using Avalonia;
using Avalonia.Controls;

namespace JiMEditor.Avalonia;

/// <summary>Process-wide UI/runtime flags set when the Avalonia application starts.</summary>
public static class ApplicationEnvironment
{
	public static bool IsHeadless { get; private set; }

	public static bool SupportsKeyboardFocus => !IsHeadless && !Design.IsDesignMode;

	/// <summary>Called from <see cref="App.OnFrameworkInitializationCompleted"/>.</summary>
	public static void Initialize()
	{
	}

	/// <summary>Mark headless mode (parity UI tests call this from <c>ParityTestAppBuilder</c>).</summary>
	public static void MarkHeadless() => IsHeadless = true;

	public static void TryFocus( Control? control, bool selectAllText = false )
	{
		if ( !SupportsKeyboardFocus || control is null )
			return;

		control.Focus();
		if ( selectAllText && control is TextBox textBox )
			textBox.SelectAll();
	}
}
