// Ported pattern: JiMEditor interaction editors — config presets (2026-04-13).
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace JiMEditor.Avalonia.EventDialog;

public static class EventDialogConfigFactory
{
	public static EventDialogConfig Dialog() =>
		new()
		{
			CustomTabTitle = "Dialog",
			ForbiddenDefaultEventName = "New Dialog Event",
			ThreatCallout = EventSectionVis.Visible,
			EventNameGroup = EventSectionVis.Visible,
			Rewards = EventSectionVis.Visible,
			Token = EventSectionVis.Visible,
			Triggers = EventSectionVis.Visible,
			EventText = EventSectionVis.Visible,
			FlavorText = EventSectionVis.Visible,
			FlavorReadOnly = true,
			FlavorBodyText =
				"Dialog Events don't use Flavor Text. When its Token is interacted with, the Event Text is shown along with up to 3 dialog choices the players can use their action on."
		};

	public static EventDialogConfig Reward() =>
		new()
		{
			CustomTabTitle = "Rewards",
			ForbiddenDefaultEventName = "New Reward Event",
			ThreatCallout = EventSectionVis.Visible,
			EventNameGroup = EventSectionVis.Visible,
			Instructions = EventSectionVis.Visible,
			Rewards = EventSectionVis.Visible,
			Token = EventSectionVis.Collapsed,
			Triggers = EventSectionVis.Visible,
			EventText = EventSectionVis.Visible,
			FlavorText = EventSectionVis.Visible,
			FlavorReadOnly = true,
			EventBodyText = "The Event Text for this Event is optional. Leave it blank to NOT SHOW any text after it activates.",
			FlavorBodyText = "This Event doesn't use Flavor Text.",
			ShowCustomTab = false,
			InstructionsFactory = CreateRewardInstructions,
			TokenSectionOverrideFactory = CreateRewardTokenInfo
		};

	public static EventDialogConfig Persistent() =>
		new()
		{
			CustomTabTitle = "Persistent Event",
			ForbiddenDefaultEventName = "New Persistent Event",
			ThreatCallout = EventSectionVis.Visible,
			EventNameGroup = EventSectionVis.Visible,
			Rewards = EventSectionVis.Collapsed,
			Token = EventSectionVis.Visible,
			Triggers = EventSectionVis.Collapsed,
			EventText = EventSectionVis.Disabled,
			FlavorText = EventSectionVis.Visible,
			FlavorReadOnly = true,
			EventBodyText = "The Event Text and Flavor Text are not used for Persistent Events. Instead, when players activate this Event's Token, the delegated Event is activated along with its own Flavor and Event Text.",
			FlavorBodyText = "Persistent Events delegate token interaction text to another Event.",
			TriggerSectionOverrideFactory = CreatePersistentTriggerInfo
		};

	public static EventDialogConfig Conditional() =>
		new()
		{
			CustomTabTitle = "Conditional Event",
			ForbiddenDefaultEventName = "New Conditional Event",
			ThreatCallout = EventSectionVis.Visible,
			EventNameGroup = EventSectionVis.Visible,
			Rewards = EventSectionVis.Collapsed,
			Token = EventSectionVis.Collapsed,
			Triggers = EventSectionVis.Collapsed,
			EventText = EventSectionVis.Disabled,
			FlavorText = EventSectionVis.Collapsed,
			FlavorReadOnly = true,
			EventBodyText = "Because of the nature of Conditional Events, they do not have Event Text.",
			FlavorBodyText = "Conditional Events do not use Flavor Text.",
			TokenSectionOverrideFactory = CreateConditionalTokenInfo,
			TriggerSectionOverrideFactory = CreateConditionalTriggerInfo
		};

	static Control CreateRewardInstructions()
	{
		var text = new TextBlock
		{
			Classes = { "subText" },
			TextWrapping = TextWrapping.Wrap
		};
		text.Inlines.Add( new Run( "The Reward Event simply gives the players the reward(s) you set below when it activates. It can't be assigned to a Token as a Token Interaction, so it has no Flavor Text. It can only activate after being Triggered (" ) );
		text.Inlines.Add( new Run( "Triggered By" ) { FontWeight = FontWeight.Bold } );
		text.Inlines.Add( new Run( " in the General Properties tab). This Event is silent - it does not show Event Text after activating." ) );

		return new Border
		{
			Classes = { "callout" },
			Child = text
		};
	}

	static Control CreateRewardTokenInfo()
	{
		var panel = new StackPanel { Spacing = 4 };
		panel.Children.Add( new TextBlock { Classes = { "heading" }, Text = "Token Interaction" } );
		panel.Children.Add(
			new TextBlock
			{
				Classes = { "subText" },
				TextWrapping = TextWrapping.Wrap,
				Text = "Reward Events are activated by a Trigger and cannot be assigned to an explorable Token on a tile."
			} );
		return new Border
		{
			Classes = { "callout" },
			Child = panel
		};
	}

	static Control CreatePersistentTriggerInfo() =>
		CreateSimpleCallout(
			"Triggered By and Set Trigger After:",
			"Since Persistent Events delegate the player's Action to another Event, there are no 'Triggered By' or 'Trigger After...' Triggers." );

	static Control CreateConditionalTokenInfo() =>
		CreateSimpleCallout(
			"Token Interaction",
			"Conditional Events are always active in the Scenario and cannot be assigned to an explorable Token on a tile." );

	static Control CreateConditionalTriggerInfo() =>
		CreateSimpleCallout(
			"Triggered By and Set Trigger After:",
			"Conditional Events are always active and do nothing more than listen for other Triggers. Because of this, they cannot be Triggered to activate or fire a Trigger after becoming activated." );

	static Control CreateSimpleCallout( string heading, string body )
	{
		var panel = new StackPanel { Spacing = 4 };
		panel.Children.Add( new TextBlock { Classes = { "heading" }, Text = heading } );
		panel.Children.Add( new TextBlock { Classes = { "subText" }, TextWrapping = TextWrapping.Wrap, Text = body } );
		return new Border { Classes = { "callout" }, Child = panel };
	}
}
