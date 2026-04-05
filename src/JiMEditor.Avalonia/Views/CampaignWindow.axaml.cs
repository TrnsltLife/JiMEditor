// Ported from: JiMEditor/CampaignWindow — minimal Avalonia shell (2026-04-04). Scenario open from list TODO-Avalonia-Reenable.
using Avalonia.Controls;
using JiME;

namespace JiMEditor.Avalonia.Views;

public partial class CampaignWindow : Window
{
	public Campaign campaign { get; private set; } = null!;

	public CampaignWindow()
	{
		InitializeComponent();
	}

	public CampaignWindow( Campaign c ) : this()
	{
		campaign = c;
		DataContext = campaign;
		Title = string.IsNullOrWhiteSpace( c.campaignName ) ? "Campaign" : c.campaignName;
	}
}
