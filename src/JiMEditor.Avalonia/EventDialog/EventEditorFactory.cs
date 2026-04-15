// Ported pattern: JiMEditor MainWindow.OpenInteractionEditor — central routing (2026-04-13).
using System.Threading.Tasks;
using Avalonia.Controls;
using JiMEditor.Avalonia.Views.Events;
using JiME;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace JiMEditor.Avalonia.EventDialog;

/// <summary>Opens the Avalonia editor for a supported <see cref="IInteraction"/> type.</summary>
public static class EventEditorFactory
{
	/// <summary>
	/// Opens the editor for <paramref name="interaction"/>; runs translation post-pass like WPF <see cref="Translatable.HandleWindow"/>.
	/// Returns <see cref="BaseEventDialogWindow.Accepted"/> (OK vs cancel).
	/// </summary>
	public static async Task<bool> TryEditInteractionAsync( Window owner, Scenario scenario, IInteraction interaction )
	{
		switch ( interaction )
		{
			case DialogInteraction d:
			{
				var originalValues = d.CaptureStartingValues();
				var originalPrefix = d.TranslationKeyPrefix();
				var originalKeyName = d.TranslationKeyName();
				var w = new DialogInteractionWindow( scenario, d, showCancelButton: false );
				await w.ShowDialog( owner );
				d.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				return w.Accepted;
			}
			case RewardInteraction r:
			{
				var originalValues = r.CaptureStartingValues();
				var originalPrefix = r.TranslationKeyPrefix();
				var originalKeyName = r.TranslationKeyName();
				var w = new RewardInteractionWindow( scenario, r, showCancelButton: false );
				await w.ShowDialog( owner );
				r.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				return w.Accepted;
			}
			case PersistentTokenInteraction p:
			{
				var originalValues = p.CaptureStartingValues();
				var originalPrefix = p.TranslationKeyPrefix();
				var originalKeyName = p.TranslationKeyName();
				var w = new PersistentInteractionWindow( scenario, p, showCancelButton: false );
				await w.ShowDialog( owner );
				p.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				return w.Accepted;
			}
			case ConditionalInteraction c:
			{
				var originalValues = c.CaptureStartingValues();
				var originalPrefix = c.TranslationKeyPrefix();
				var originalKeyName = c.TranslationKeyName();
				var w = new ConditionalInteractionWindow( scenario, c, showCancelButton: false );
				await w.ShowDialog( owner );
				c.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
				return w.Accepted;
			}
			default:
				MessageBox.Show(
					"This event type does not have an Avalonia editor yet.",
					"Not implemented",
					MessageBoxButton.OK,
					MessageBoxImage.Information );
				return false;
		}
	}

	/// <summary>Show new-dialog flow (Cancel visible). Caller adds <paramref name="created"/> to the scenario only when this returns true.</summary>
	public static async Task<(bool accepted, DialogInteraction created)> TryCreateDialogInteractionAsync( Window owner, Scenario scenario )
	{
		var created = new DialogInteraction( "New Dialog Event" );
		var originalValues = created.CaptureStartingValues();
		var originalPrefix = created.TranslationKeyPrefix();
		var originalKeyName = created.TranslationKeyName();
		var w = new DialogInteractionWindow( scenario, created, showCancelButton: true );
		await w.ShowDialog( owner );
		created.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
		return ( w.Accepted, created );
	}

	/// <summary>Show new-reward flow (Cancel visible). Caller adds <paramref name="created"/> only when accepted.</summary>
	public static async Task<(bool accepted, RewardInteraction created)> TryCreateRewardInteractionAsync( Window owner, Scenario scenario )
	{
		var created = new RewardInteraction( "New Reward Event" );
		var originalValues = created.CaptureStartingValues();
		var originalPrefix = created.TranslationKeyPrefix();
		var originalKeyName = created.TranslationKeyName();
		var w = new RewardInteractionWindow( scenario, created, showCancelButton: true );
		await w.ShowDialog( owner );
		created.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
		return ( w.Accepted, created );
	}

	public static async Task<(bool accepted, PersistentTokenInteraction created)> TryCreatePersistentInteractionAsync( Window owner, Scenario scenario )
	{
		var created = new PersistentTokenInteraction( "New Persistent Event" );
		var originalValues = created.CaptureStartingValues();
		var originalPrefix = created.TranslationKeyPrefix();
		var originalKeyName = created.TranslationKeyName();
		var w = new PersistentInteractionWindow( scenario, created, showCancelButton: true );
		await w.ShowDialog( owner );
		created.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
		return ( w.Accepted, created );
	}

	public static async Task<(bool accepted, ConditionalInteraction created)> TryCreateConditionalInteractionAsync( Window owner, Scenario scenario )
	{
		var created = new ConditionalInteraction( "New Conditional Event" );
		var originalValues = created.CaptureStartingValues();
		var originalPrefix = created.TranslationKeyPrefix();
		var originalKeyName = created.TranslationKeyName();
		var w = new ConditionalInteractionWindow( scenario, created, showCancelButton: true );
		await w.ShowDialog( owner );
		created.ApplyTranslationUpdatesAfterEditorDialog( scenario.translationObserver, originalValues, originalPrefix, originalKeyName );
		return ( w.Accepted, created );
	}
}
