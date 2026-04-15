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
		if ( _interaction is RewardInteraction reward )
		{
			RewardsHeadingText.Text = "Lore / XP / Threat Reduction Rewards";
			RewardsBodyText.Text = "Reduction rewards are applied when this Reward Event activates.";
			LoreRewardTextBox.Text = reward.rewardLore.ToString( CultureInfo.InvariantCulture );
			XpRewardTextBox.Text = reward.rewardXP.ToString( CultureInfo.InvariantCulture );
			ThreatRewardTextBox.Text = reward.rewardThreat.ToString( CultureInfo.InvariantCulture );
			return;
		}

		RewardsHeadingText.Text = "Lore / XP / Threat Rewards";
		RewardsBodyText.Text = "Rewards are given after the Event is finished activating.";
		LoreRewardTextBox.Text = _interaction.loreReward.ToString( CultureInfo.InvariantCulture );
		XpRewardTextBox.Text = _interaction.xpReward.ToString( CultureInfo.InvariantCulture );
		ThreatRewardTextBox.Text = _interaction.threatReward.ToString( CultureInfo.InvariantCulture );
	}

	public void Commit()
	{
		if ( _interaction == null )
			return;
		if ( _interaction is RewardInteraction reward )
		{
			reward.rewardLore = int.TryParse( LoreRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var rl ) ? rl : 0;
			reward.rewardXP = int.TryParse( XpRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var rx ) ? rx : 0;
			reward.rewardThreat = int.TryParse( ThreatRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var rt ) ? rt : 0;
			return;
		}
		_interaction.loreReward = int.TryParse( LoreRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var l ) ? l : 0;
		_interaction.xpReward = int.TryParse( XpRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var x ) ? x : 0;
		_interaction.threatReward = int.TryParse( ThreatRewardTextBox.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var t ) ? t : 0;
	}
}
