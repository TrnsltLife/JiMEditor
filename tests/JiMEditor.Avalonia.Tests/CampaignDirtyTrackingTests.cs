using JiME;
using JiMEditor.Avalonia;
using Xunit;

namespace JiMEditor.Avalonia.Tests;

public sealed class CampaignDirtyTrackingTests
{
	[Fact]
	public void MarkDirty_sets_isDirty_and_is_idempotent()
	{
		var campaign = new Campaign();
		Assert.False( campaign.isDirty );

		campaign.MarkDirty();
		Assert.True( campaign.isDirty );

		campaign.MarkDirty();
		Assert.True( campaign.isDirty );
	}

	[Fact]
	public void PauseDirtyTracking_blocks_MarkDirty()
	{
		var campaign = new Campaign();
		using ( campaign.PauseDirtyTracking() )
			campaign.MarkDirty();
		Assert.False( campaign.isDirty );

		campaign.MarkDirty();
		Assert.True( campaign.isDirty );
	}

	[Fact]
	public void ClearDirty_resets_flag()
	{
		var campaign = new Campaign();
		campaign.MarkDirty();
		campaign.ClearDirty();
		Assert.False( campaign.isDirty );
	}

	[Fact]
	public void Collection_tracker_marks_dirty_on_scenario_and_trigger_lists()
	{
		var campaign = new Campaign();
		using ( CampaignDirty.Attach( campaign ) )
		{
			campaign.scenarioCollection.Add( new CampaignItem { scenarioName = "One", fileName = "one.jime" } );
			Assert.True( campaign.isDirty );

			campaign.ClearDirty();
			campaign.triggerCollection.Add( new Trigger( "camp [CMPN]" ) );
			Assert.True( campaign.isDirty );
		}
	}

	[Fact]
	public void isDirty_is_not_serialized()
	{
		var campaign = new Campaign { campaignName = "Test" };
		campaign.MarkDirty();
		var json = Newtonsoft.Json.JsonConvert.SerializeObject( campaign );
		Assert.DoesNotContain( "isDirty", json, StringComparison.OrdinalIgnoreCase );
	}
}
