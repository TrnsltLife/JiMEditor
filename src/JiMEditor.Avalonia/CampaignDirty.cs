// Coarse unsaved-edit tracking for Campaign Manager: collections + editor input controls.
using System;
using System.Collections.Specialized;
using Avalonia.Controls;
using JiME;

namespace JiMEditor.Avalonia;

/// <summary>
/// Marks <see cref="Campaign.isDirty"/> from campaign list mutations.
/// Field edits use <see cref="ScenarioDirty.WatchInputs(Window, Action)"/>.
/// </summary>
public static class CampaignDirty
{
	static CampaignDirtyTracker? _active;

	public static Campaign? CurrentCampaign => _active?.Campaign;

	public static CampaignDirtyTracker Attach( Campaign campaign )
	{
		_active?.Dispose();
		var tracker = new CampaignDirtyTracker( campaign );
		_active = tracker;
		return tracker;
	}

	internal static void ClearActive( CampaignDirtyTracker tracker )
	{
		if ( _active == tracker )
			_active = null;
	}

	public static void WatchInputs( Window window, Campaign campaign )
	{
		if ( campaign is null )
			return;
		ScenarioDirty.WatchInputs( window, campaign.MarkDirty );
	}
}

/// <summary>
/// Listens to campaign scenario and trigger lists. Dispose when Campaign Manager closes.
/// </summary>
public sealed class CampaignDirtyTracker : IDisposable
{
	readonly NotifyCollectionChangedEventHandler _markDirty;
	bool _disposed;

	public Campaign Campaign { get; }

	internal CampaignDirtyTracker( Campaign campaign )
	{
		Campaign = campaign;
		_markDirty = ( _, _ ) => campaign.MarkDirty();
		campaign.scenarioCollection.CollectionChanged += _markDirty;
		campaign.triggerCollection.CollectionChanged += _markDirty;
	}

	public void Dispose()
	{
		if ( _disposed )
			return;
		_disposed = true;
		CampaignDirty.ClearActive( this );
		Campaign.scenarioCollection.CollectionChanged -= _markDirty;
		Campaign.triggerCollection.CollectionChanged -= _markDirty;
	}
}
