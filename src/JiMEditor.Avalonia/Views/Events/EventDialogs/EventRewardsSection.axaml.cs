// Ported from: JiMEditor/Views/Interactions/DialogInteractionWindow.xaml — rewards row (2026-04-13).
using System.Globalization;
using Avalonia.Controls;
using JiME;

namespace JiMEditor.Avalonia.Views.Events.EventDialogs;

public partial class EventRewardsSection : UserControl
{
	IInteraction? _interaction;

	public EventRewardsSection()
	{
		InitializeComponent();
	}

	public void Attach( IInteraction interaction ) => _interaction = interaction;

	public void Hydrate()
	{
		if ( _interaction == null )
			return;
		LoreRewardTextBox.Text = _interaction.loreReward.ToString( CultureInfo.InvariantCulture );
		XpRewardTextBox.Text = _interaction.xpReward.ToString( CultureInfo.InvariantCulture );
		ThreatRewardTextBox.Text = _interaction.threatReward.ToString( CultureInfo.InvariantCulture );
	}

	public void Commit()
	{
		if ( _interaction == null )
			return;
		_interaction.loreReward = int.TryParse( LoreRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var l ) ? l : 0;
		_interaction.xpReward = int.TryParse( XpRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var x ) ? x : 0;
		_interaction.threatReward = int.TryParse( ThreatRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var t ) ? t : 0;
	}
}
