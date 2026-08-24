// Ported from: JiMEditor/Common/ErrorChecker.cs (2026-08-23) — fills Errors and is shown from MainWindow.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JiME
{
	public class ErrorChecker : INotifyPropertyChanged
	{
		[Flags]
		enum EventRefRules
		{
			None = 0,
			RequireTokenInteraction = 1,
			DisallowStart = 2,
			DisallowGroup = 4,
		}

		public const string DisabledCollectionsHeading = "Components from Collections not selected for this Scenario";
		public const string DisabledCollectionsSettingsLink = "Enable additional Collections in Settings";
		public const string DisabledCollectionsPruneLink = "Remove all disabled Collection components";
		public const string TokenLimitsHeading = "Tiles that ignore token limits";

		string _Errors = "None Found";
		string _errorCount = "Warnings (0):";
		IReadOnlyList<ErrorCheckWarning> _warningItems = Array.Empty<ErrorCheckWarning>();

		public string Errors
		{
			get => _Errors;
			set
			{
				if ( value != _Errors )
				{
					_Errors = value;
					PropChanged( "Errors" );
				}
			}
		}

		public string errorCount
		{
			get => _errorCount;
			set
			{
				if ( value != _errorCount )
				{
					_errorCount = value;
					PropChanged( "errorCount" );
				}
			}
		}

		public IReadOnlyList<ErrorCheckWarning> WarningItems
		{
			get => _warningItems;
			private set
			{
				_warningItems = value;
				PropChanged( "WarningItems" );
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public ErrorChecker()
		{
			Errors = "None Found";
			errorCount = "Warnings (0):";
		}

		public void AddWarning( string s )
		{
			Errors += "\r\n" + s;
		}

		public void ClearErrors()
		{
			Errors = "None Found";
			errorCount = "Warnings (0):";
			WarningItems = Array.Empty<ErrorCheckWarning>();
		}

		public void CheckErrors( Scenario scenario )
		{
			var warnings = new List<ErrorCheckWarning>();
			var triggers = new HashSet<string>(
				scenario.triggersObserver.Select( t => t.dataName ),
				StringComparer.Ordinal );
			var events = scenario.interactionObserver
				.GroupBy( i => i.dataName, StringComparer.Ordinal )
				.ToDictionary( g => g.Key, g => g.First(), StringComparer.Ordinal );

			CheckDupeNames( scenario, warnings );
			CheckDuplicateThreatThresholds( scenario, warnings );
			CheckEventReferences( scenario, warnings, triggers, events );
			CheckTokenLinks( scenario, warnings, triggers, events );
			CheckChapterAndTileTriggers( scenario, warnings, triggers );
			CheckEmptyChapters( scenario, warnings );
			CheckDefaultPlayerTexts( scenario, warnings );
			CheckTokenLimits( scenario, warnings );
			CheckDisabledCollections( scenario, warnings );

			WarningItems = warnings;
			errorCount = $"Warnings ({warnings.Count}):";
			Errors = FormatErrors( warnings );
		}

		static string FormatErrors( List<ErrorCheckWarning> warnings )
		{
			var general = Texts( warnings, ErrorCheckSection.General );
			var tokenLimits = Texts( warnings, ErrorCheckSection.TokenLimits );
			var collections = Texts( warnings, ErrorCheckSection.DisabledCollections );
			if ( general.Count == 0 && tokenLimits.Count == 0 && collections.Count == 0 )
				return "None Found";

			var lines = new List<string>( general );
			AppendSection( lines, TokenLimitsHeading, tokenLimits );
			if ( collections.Count > 0 )
			{
				if ( lines.Count > 0 )
					lines.Add( "" );
				lines.Add( DisabledCollectionsHeading );
				lines.Add( DisabledCollectionsSettingsLink );
				lines.Add( DisabledCollectionsPruneLink );
				lines.AddRange( collections );
			}

			return string.Join( "\r\n", lines );
		}

		static List<string> Texts( List<ErrorCheckWarning> warnings, ErrorCheckSection section ) =>
			warnings.Where( w => w.Section == section ).Select( w => w.Text ).ToList();

		static void AppendSection( List<string> lines, string heading, List<string> items )
		{
			if ( items.Count == 0 )
				return;
			if ( lines.Count > 0 )
				lines.Add( "" );
			lines.Add( heading );
			lines.AddRange( items );
		}

		static bool IsPlaceholderName( string? name ) =>
			string.IsNullOrWhiteSpace( name ) || name == "None";

		static ErrorCheckSpan T( string text ) => new( text );
		static ErrorCheckSpan L( string text, ErrorCheckNav nav ) => new( text, nav );
		static ErrorCheckSpan EventOwner( string name ) =>
			name == StartInteraction.DEFAULT_NAME
				? T( $"Event \"{name}\"" )
				: L( $"Event \"{name}\"", new ErrorCheckNav( ErrorCheckTargetKind.Event, name ) );
		static ErrorCheckSpan ChapterOwner( string name ) =>
			L( $"Tile block \"{name}\"", new ErrorCheckNav( ErrorCheckTargetKind.TileBlock, name ) );
		static ErrorCheckSpan NamedLink( string type, string name )
		{
			var kind = type switch
			{
				"Event" => ErrorCheckTargetKind.Event,
				"Trigger" => ErrorCheckTargetKind.Trigger,
				"Objective" => ErrorCheckTargetKind.Objective,
				"Resolution" => ErrorCheckTargetKind.Resolution,
				"Threat" => ErrorCheckTargetKind.Threat,
				"Tile Block" => ErrorCheckTargetKind.TileBlock,
				_ => ErrorCheckTargetKind.Event
			};
			return L( $"\"{name}\"", new ErrorCheckNav( kind, name ) );
		}

		void CheckDupeNames( Scenario s, List<ErrorCheckWarning> warnings )
		{
			var seen = new HashSet<string>( StringComparer.Ordinal );
			void Consider( string? name, string type )
			{
				if ( IsPlaceholderName( name ) )
					return;
				// Same-type only. Triggers, events, objectives, and tile blocks live in
				// separate collections and are resolved by field context, not a global name table.
				var key = type + "\0" + name;
				if ( !seen.Add( key ) )
					warnings.Add( new ErrorCheckWarning(
						T( $"Duplicate {type} name " ),
						NamedLink( type, name! ),
						T( "." ) ) );
			}

			foreach ( var item in s.interactionObserver )
				Consider( item.dataName, "Event" );
			foreach ( var item in s.triggersObserver )
				Consider( item.dataName, "Trigger" );
			foreach ( var item in s.objectiveObserver )
				Consider( item.dataName, "Objective" );
			foreach ( var item in s.resolutionObserver )
				Consider( item.dataName, "Resolution" );
			foreach ( var item in s.chapterObserver )
				Consider( item.dataName, "Tile Block" );
		}

		void CheckDuplicateThreatThresholds( Scenario s, List<ErrorCheckWarning> warnings )
		{
			if ( s.threatNotUsed )
				return;

			var seen = new HashSet<int>();
			var reported = new HashSet<int>();
			foreach ( var threat in s.threatObserver )
			{
				if ( !seen.Add( threat.threshold ) && reported.Add( threat.threshold ) )
					warnings.Add( new ErrorCheckWarning(
						T( "Duplicate Threat " ),
						L( $"Threshold {threat.threshold}", new ErrorCheckNav( ErrorCheckTargetKind.Threat ) ),
						T( "." ) ) );
			}
		}

		void CheckEventReferences(
			Scenario s,
			List<ErrorCheckWarning> warnings,
			HashSet<string> triggers,
			Dictionary<string, IInteraction> events )
		{
			const EventRefRules tokenTarget =
				EventRefRules.RequireTokenInteraction | EventRefRules.DisallowStart | EventRefRules.DisallowGroup;
			const EventRefRules anyEventExceptStart = EventRefRules.DisallowStart;

			foreach ( var interaction in s.interactionObserver )
			{
				if ( interaction.dataName == "None" )
					continue;
				var owner = EventOwner( interaction.dataName );

				CheckTrigger( warnings, triggers, owner, "Triggered By", interaction.triggerName );
				if ( interaction is InteractionBase ibase )
					CheckTrigger( warnings, triggers, owner, "Set Trigger After", ibase.triggerAfterName );

				switch ( interaction )
				{
					case DecisionInteraction d:
						CheckTrigger( warnings, triggers, owner, "Choice 1 trigger", d.choice1Trigger );
						CheckTrigger( warnings, triggers, owner, "Choice 2 trigger", d.choice2Trigger );
						if ( d.isThreeChoices )
							CheckTrigger( warnings, triggers, owner, "Choice 3 trigger", d.choice3Trigger );
						break;
					case DialogInteraction d:
						CheckTrigger( warnings, triggers, owner, "Choice 1 trigger", d.c1Trigger );
						CheckTrigger( warnings, triggers, owner, "Choice 2 trigger", d.c2Trigger );
						CheckTrigger( warnings, triggers, owner, "Choice 3 trigger", d.c3Trigger );
						break;
					case TestInteraction t:
						CheckTrigger( warnings, triggers, owner, "Pass trigger", t.successTrigger );
						CheckTrigger( warnings, triggers, owner, "Fail trigger", t.failTrigger );
						break;
					case ConditionalInteraction c:
						CheckTrigger( warnings, triggers, owner, "Finished trigger", c.finishedTrigger );
						if ( c.triggerList != null )
						{
							foreach ( var name in c.triggerList )
								CheckTrigger( warnings, triggers, owner, "Condition trigger", name );
						}
						break;
					case PersistentTokenInteraction p:
						CheckTrigger( warnings, triggers, owner, "Alternative Flavor Text Trigger", p.alternativeTextTrigger );
						if ( IsPlaceholderName( p.eventToActivate ) )
							warnings.Add( new ErrorCheckWarning(
								owner,
								T( " has no Delegate Event." ) ) );
						else
							CheckEvent( warnings, events, owner, "Delegate Event", p.eventToActivate, tokenTarget );
						break;
					case ThreatInteraction th:
						CheckTrigger( warnings, triggers, owner, "Enemies Defeated trigger", th.triggerDefeatedName );
						break;
					case MultiEventInteraction m:
						if ( m.triggerList != null )
						{
							foreach ( var name in m.triggerList )
								CheckTrigger( warnings, triggers, owner, "Multi-Event trigger", name );
						}
						if ( m.eventList != null )
						{
							foreach ( var name in m.eventList )
								CheckEvent( warnings, events, owner, "Multi-Event event", name, anyEventExceptStart );
						}
						break;
					case BranchInteraction b:
						CheckTrigger( warnings, triggers, owner, "Story Branch test trigger", b.triggerTest );
						CheckTrigger( warnings, triggers, owner, "Story Branch (true) trigger", b.triggerIsSetTrigger );
						CheckTrigger( warnings, triggers, owner, "Story Branch (false) trigger", b.triggerNotSetTrigger );
						CheckEvent( warnings, events, owner, "Story Branch (true) event", b.triggerIsSet, anyEventExceptStart );
						CheckEvent( warnings, events, owner, "Story Branch (false) event", b.triggerNotSet, anyEventExceptStart );
						break;
					case ReplaceTokenInteraction r:
						CheckEvent( warnings, events, owner, "Event To Replace", r.eventToReplace, tokenTarget );
						CheckEvent( warnings, events, owner, "Replace With Event", r.replaceWithEvent, tokenTarget );
						break;
					case ItemInteraction item:
						CheckTrigger( warnings, triggers, owner, "Fallback trigger", item.fallbackTrigger );
						break;
					case TitleInteraction title:
						CheckTrigger( warnings, triggers, owner, "Fallback trigger", title.fallbackTrigger );
						break;
				}
			}
		}

		void CheckTokenLinks(
			Scenario s,
			List<ErrorCheckWarning> warnings,
			HashSet<string> triggers,
			Dictionary<string, IInteraction> events )
		{
			const EventRefRules tokenEvent =
				EventRefRules.RequireTokenInteraction | EventRefRules.DisallowGroup;

			foreach ( var chapter in s.chapterObserver )
			{
				foreach ( var tile in chapter.tileObserver.OfType<BaseTile>() )
				{
					foreach ( var token in tile.tokenList )
					{
						var owner = L(
							$"Token on tile {tile.idNumber} in tile block \"{chapter.dataName}\"",
							new ErrorCheckNav( ErrorCheckTargetKind.Token, id: token.GUID ) );
						CheckTrigger( warnings, triggers, owner, "Triggered By", token.triggeredByName );
						CheckEvent( warnings, events, owner, "Event", token.triggerName, tokenEvent );
					}
				}
			}
		}

		void CheckChapterAndTileTriggers( Scenario s, List<ErrorCheckWarning> warnings, HashSet<string> triggers )
		{
			foreach ( var chapter in s.chapterObserver )
			{
				var owner = ChapterOwner( chapter.dataName );
				CheckTrigger( warnings, triggers, owner, "Triggered By", chapter.triggeredBy );
				CheckTrigger( warnings, triggers, owner, "Trigger", chapter.triggerName );
				CheckTrigger( warnings, triggers, owner, "Trigger On First Exploration", chapter.exploreTrigger );
				CheckTrigger( warnings, triggers, owner, "Trigger After Exploring All Tiles", chapter.exploredAllTilesTrigger );

				foreach ( var tile in chapter.tileObserver )
				{
					if ( tile is BaseTile baseTile )
					{
						var tileOwner = L(
							$"Tile {baseTile.idNumber} in tile block \"{chapter.dataName}\"",
							new ErrorCheckNav( ErrorCheckTargetKind.Tile, id: baseTile.GUID ) );
						CheckTrigger( warnings, triggers, tileOwner, "Explored trigger", baseTile.triggerName );
					}
					else if ( tile is BattleTile battle )
					{
						var battleLoc = L(
							$"Battle tile {battle.idNumber} in tile block \"{chapter.dataName}\"",
							new ErrorCheckNav( ErrorCheckTargetKind.TileBlock, chapter.dataName ) );
						CheckTrigger( warnings, triggers, battleLoc, "Trigger", battle.triggerName );
						CheckTrigger( warnings, triggers, battleLoc, "Token trigger", battle.tokenTrigger );
					}
				}
			}
		}

		static void CheckTrigger(
			List<ErrorCheckWarning> warnings,
			HashSet<string> triggers,
			ErrorCheckSpan owner,
			string field,
			string? name )
		{
			if ( IsPlaceholderName( name ) )
				return;
			if ( !triggers.Contains( name! ) )
				warnings.Add( new ErrorCheckWarning(
					owner,
					T( $" {field} points at missing trigger " ),
					T( $"\"{name}\"" ),
					T( "." ) ) );
		}

		static void CheckEvent(
			List<ErrorCheckWarning> warnings,
			Dictionary<string, IInteraction> events,
			ErrorCheckSpan owner,
			string field,
			string? name,
			EventRefRules rules )
		{
			if ( IsPlaceholderName( name ) )
				return;
			if ( !events.TryGetValue( name!, out var interaction ) )
			{
				warnings.Add( new ErrorCheckWarning(
					owner,
					T( $" {field} points at missing event " ),
					T( $"\"{name}\"" ),
					T( "." ) ) );
				return;
			}

			var eventLink = L( $"\"{name}\"", new ErrorCheckNav( ErrorCheckTargetKind.Event, name ) );
			if ( rules.HasFlag( EventRefRules.DisallowStart )
			     && ( interaction.interactionType == InteractionType.Start || name == StartInteraction.DEFAULT_NAME ) )
			{
				warnings.Add( new ErrorCheckWarning(
					owner,
					T( $" {field} points at Starting Position, which cannot be used as a target Event." ) ) );
				return;
			}

			if ( rules.HasFlag( EventRefRules.RequireTokenInteraction ) && !interaction.isTokenInteraction )
			{
				warnings.Add( new ErrorCheckWarning(
					owner,
					T( $" {field} points at event " ),
					eventLink,
					T( " which is not a Token Interaction." ) ) );
				return;
			}

			if ( rules.HasFlag( EventRefRules.DisallowGroup ) && InteractionGroupNaming.HasGroupSuffix( name ) )
				warnings.Add( new ErrorCheckWarning(
					owner,
					T( $" {field} points at event " ),
					eventLink,
					T( " which is a Token Interaction Group (GRP) event." ) ) );
		}

		void CheckEmptyChapters( Scenario s, List<ErrorCheckWarning> warnings )
		{
			foreach ( var chapter in s.chapterObserver )
			{
				if ( chapter.tileObserver.Count == 0 )
					warnings.Add( new ErrorCheckWarning(
						ChapterOwner( chapter.dataName ),
						T( " has no tiles." ) ) );
			}
		}

		void CheckDefaultPlayerTexts( Scenario s, List<ErrorCheckWarning> warnings )
		{
			if ( StartsWithDefault( Page0( s.introBookData ), "Default Introduction text." ) )
				warnings.Add( new ErrorCheckWarning(
					L( "Introduction Text", new ErrorCheckNav( ErrorCheckTargetKind.Settings ) ),
					T( " still uses the default placeholder." ) ) );

			foreach ( var resolution in s.resolutionObserver )
			{
				if ( resolution.isEmpty || IsPlaceholderName( resolution.dataName ) )
					continue;
				if ( StartsWithDefault( Page0( resolution ), "Default resolution text." ) )
					warnings.Add( new ErrorCheckWarning(
						L( $"Resolution \"{resolution.dataName}\"", new ErrorCheckNav( ErrorCheckTargetKind.Resolution, resolution.dataName ) ),
						T( " still uses the default Resolution Text." ) ) );
			}

			foreach ( var objective in s.objectiveObserver )
			{
				if ( objective.isEmpty || IsPlaceholderName( objective.dataName ) )
					continue;
				var owner = L(
					$"Objective \"{objective.dataName}\"",
					new ErrorCheckNav( ErrorCheckTargetKind.Objective, objective.dataName ) );
				if ( StartsWithDefault( objective.objectiveReminder, "Default Objective Reminder" ) )
					warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Objective Reminder." ) ) );
				if ( !objective.skipSummary
				     && StartsWithDefault( Page0( objective.textBookData ), "Default Objective Text" ) )
					warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Objective Summary." ) ) );
			}

			foreach ( var chapter in s.chapterObserver )
			{
				if ( chapter.noFlavorText )
					continue;
				if ( StartsWithDefault( Page0( chapter.flavorBookData ), "This optional text is shown" ) )
					warnings.Add( new ErrorCheckWarning(
						ChapterOwner( chapter.dataName ),
						T( " still uses the default Exploration Text." ) ) );
			}

			foreach ( var interaction in s.interactionObserver )
			{
				if ( interaction is ICommonData { isEmpty: true } || IsPlaceholderName( interaction.dataName ) )
					continue;
				if ( interaction is StartInteraction )
					continue;

				var owner = EventOwner( interaction.dataName );
				if ( UsesFlavorText( interaction )
				     && interaction is InteractionBase flavorBase
				     && StartsWithDefault( Page0( flavorBase.textBookData ), "Default Flavor Text" ) )
					warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Flavor Text." ) ) );

				if ( UsesEventText( interaction )
				     && interaction is InteractionBase eventBase
				     && StartsWithDefault( Page0( eventBase.eventBookData ), "Default Event Text" ) )
					warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Event Text." ) ) );

				switch ( interaction )
				{
					case TestInteraction test:
						if ( StartsWithDefault( Page0( test.passBookData ), "Default Pass Text" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Pass Text." ) ) );
						if ( !test.isCumulative
						     && StartsWithDefault( Page0( test.failBookData ), "Default Fail Text" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Fail Text." ) ) );
						if ( test.isCumulative
						     && StartsWithDefault( Page0( test.progressBookData ), "Default Progress Text" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Progress Text." ) ) );
						break;
					case PersistentTokenInteraction persistent:
						if ( StartsWithDefault( Page0( persistent.alternativeBookData ), "This text will be shown" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Alternative Flavor Text." ) ) );
						break;
					case DecisionInteraction decision:
						if ( IsExactDefault( decision.choice1, "Choice One" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Choice 1 text." ) ) );
						if ( IsExactDefault( decision.choice2, "Choice Two" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Choice 2 text." ) ) );
						if ( decision.isThreeChoices && IsExactDefault( decision.choice3, "Choice Three" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Choice 3 text." ) ) );
						break;
					case DialogInteraction dialog:
						if ( IsExactDefault( dialog.choice1, "Choice 1" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Choice 1 text." ) ) );
						if ( IsExactDefault( dialog.choice2, "Choice 2" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Choice 2 text." ) ) );
						if ( IsExactDefault( dialog.choice3, "Choice 3" ) )
							warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Choice 3 text." ) ) );
						break;
				}
			}

			foreach ( var group in s.activationsObserver )
			{
				if ( group.id < MonsterActivations.START_OF_CUSTOM_ACTIVATIONS )
					continue;
				var owner = L(
					$"Enemy Attack Group \"{group.dataName}\"",
					new ErrorCheckNav( ErrorCheckTargetKind.Activation, group.dataName ) );
				foreach ( var item in group.activations )
				{
					if ( StartsWithDefault( item.text, "New attack text." ) )
						warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default Attack Text." ) ) );
					if ( StartsWithDefault( item.effect, "New after effect (optional)." ) )
						warnings.Add( new ErrorCheckWarning( owner, T( " still uses the default After Effect." ) ) );
				}
			}
		}

		static bool UsesFlavorText( IInteraction interaction )
		{
			if ( !interaction.isTokenInteraction )
				return false;
			return interaction switch
			{
				ConditionalInteraction => false,
				PersistentTokenInteraction => false,
				RewardInteraction => false,
				DialogInteraction => false,
				TextInteraction t => !t.hasActivated,
				ReplaceTokenInteraction r => !r.noText,
				_ => true
			};
		}

		static bool UsesEventText( IInteraction interaction ) =>
			interaction switch
			{
				ConditionalInteraction => false,
				PersistentTokenInteraction => false,
				MultiEventInteraction m => !m.isSilent,
				TextInteraction t => !t.hasActivated,
				ReplaceTokenInteraction r => !r.noText,
				_ => true
			};

		static string Page0( TextBookData? book ) =>
			book?.pages != null && book.pages.Count > 0 ? book.pages[0] ?? "" : "";

		static bool StartsWithDefault( string? text, string prefix ) =>
			!string.IsNullOrWhiteSpace( text )
			&& text.TrimStart().StartsWith( prefix, StringComparison.Ordinal );

		static bool IsExactDefault( string? text, string expected ) =>
			string.Equals( text?.Trim(), expected, StringComparison.Ordinal );

		void CheckTokenLimits( Scenario s, List<ErrorCheckWarning> warnings )
		{
			foreach ( var chapter in s.chapterObserver )
			{
				foreach ( var tile in chapter.tileObserver.OfType<BaseTile>() )
				{
					var used = tile.tokenList.Count;
					if ( used <= tile.tokenCount )
						continue;
					warnings.Add( new ErrorCheckWarning(
						ErrorCheckSection.TokenLimits,
						L(
							$"Tile {tile.idNumber} in tile block \"{chapter.dataName}\"",
							new ErrorCheckNav( ErrorCheckTargetKind.Tile, id: tile.GUID ) ),
						T( $" has {used} tokens (limit {tile.tokenCount})." ) ) );
				}
			}
		}

		void CheckDisabledCollections( Scenario s, List<ErrorCheckWarning> warnings )
		{
			foreach ( var interaction in s.interactionObserver )
			{
				if ( interaction.dataName == "None" )
					continue;
				var owner = EventOwner( interaction.dataName );

				switch ( interaction )
				{
					case ItemInteraction item:
						foreach ( var it in item.itemList )
						{
							if ( it == null || it.id == 0 || it.dataName == "None" )
								continue;
							var coll = Collection.FromID( it.collection );
							var kind = it.slotId == Slot.MOUNT ? "Mount" : "Item";
							WarnIfCollectionDisabled( warnings, s, owner, coll, $"uses {kind} \"{it.dataName}\"" );
						}
						break;
					case TitleInteraction title:
						foreach ( var t in title.titleList )
						{
							if ( t == null || t.id == 0 || t.dataName == "None" )
								continue;
							var coll = Collection.FromID( t.collection );
							WarnIfCollectionDisabled( warnings, s, owner, coll, $"uses Title \"{t.dataName}\"" );
						}
						break;
					case ThreatInteraction threat:
						var monsters = Collection.MONSTERS();
						for ( var i = 0; i < threat.includedEnemies.Length && i < monsters.Length; i++ )
						{
							if ( !threat.includedEnemies[i] )
								continue;
							WarnIfMonsterDisabled( warnings, s, owner, monsters[i] );
						}
						foreach ( var m in threat.monsterCollection )
							WarnIfMonsterDisabled( warnings, s, owner, m );
						break;
				}

				if ( interaction is InteractionBase ib
				     && interaction.interactionType != InteractionType.Start
				     && ib.isTokenInteraction )
					CheckRestrictedTokenType( warnings, s, owner, ib.tokenType, ib.terrainType );
			}

			foreach ( var chapter in s.chapterObserver )
			{
				foreach ( var tile in chapter.tileObserver )
				{
					if ( tile is BaseTile baseTile )
					{
						var tileColl = Collection.FromTileNumber( baseTile.idNumber ) ?? baseTile.collection;
						var tileOwner = L(
							$"Tile {baseTile.idNumber} in tile block \"{chapter.dataName}\"",
							new ErrorCheckNav( ErrorCheckTargetKind.TileEditor, chapter.dataName ) );
						WarnIfCollectionDisabled( warnings, s, tileOwner, tileColl, "is" );

						foreach ( var token in baseTile.tokenList )
						{
							var tokenOwner = L(
								$"Token on tile {baseTile.idNumber} in tile block \"{chapter.dataName}\"",
								new ErrorCheckNav( ErrorCheckTargetKind.Token, id: token.GUID ) );
							CheckRestrictedTokenType( warnings, s, tokenOwner, token.tokenType, token.terrainType );
						}
					}
					else if ( tile is BattleTile battle )
					{
						var battleColl = Collection.FromTileNumber( battle.idNumber );
						var battleOwner = L(
							$"Battle tile {battle.idNumber} in tile block \"{chapter.dataName}\"",
							new ErrorCheckNav( ErrorCheckTargetKind.TileEditor, chapter.dataName ) );
						WarnIfCollectionDisabled( warnings, s, battleOwner, battleColl, "is" );
					}
				}
			}
		}

		static void WarnIfMonsterDisabled( List<ErrorCheckWarning> warnings, Scenario s, ErrorCheckSpan owner, Monster monster )
		{
			if ( monster == null )
				return;
			var coll = Collection.FromMonsterTypeId( monster.id );
			WarnIfCollectionDisabled( warnings, s, owner, coll, $"uses Monster \"{MonsterDisplayName( monster )}\"" );
		}

		static string MonsterDisplayName( Monster monster )
		{
			if ( !string.IsNullOrWhiteSpace( monster.dataName ) )
				return monster.dataName;
			if ( Enum.IsDefined( typeof( MonsterType ), monster.id ) )
				return ( (MonsterType)monster.id ).ToString();
			return $"Enemy type {monster.id}";
		}

		static void CheckRestrictedTokenType(
			List<ErrorCheckWarning> warnings,
			Scenario s,
			ErrorCheckSpan owner,
			TokenType tokenType,
			TerrainType terrainType )
		{
			switch ( tokenType )
			{
				case TokenType.DifficultGround:
					if ( !s.collectionObserver.Any( c => c.DifficultGround ) )
						warnings.Add( new ErrorCheckWarning(
							ErrorCheckSection.DisabledCollections,
							owner,
							T( " uses Difficult Ground, which needs Shaded Paths or Unfurling War." ) ) );
					break;
				case TokenType.Fortified:
					if ( !s.collectionObserver.Any( c => c.Fortified ) )
						warnings.Add( new ErrorCheckWarning(
							ErrorCheckSection.DisabledCollections,
							owner,
							T( " uses Fortified, which needs Unfurling War." ) ) );
					break;
				case TokenType.Terrain:
					WarnIfCollectionDisabled(
						warnings,
						s,
						owner,
						Collection.FromTerrainType( terrainType ),
						$"uses Terrain: {terrainType}" );
					break;
			}
		}

		static bool CollectionEnabled( Scenario s, Collection? coll ) =>
			coll == null || ReferenceEquals( coll, Collection.NONE ) || s.IsCollectionEnabled( coll );

		static void WarnIfCollectionDisabled(
			List<ErrorCheckWarning> warnings,
			Scenario s,
			ErrorCheckSpan owner,
			Collection? coll,
			string usedWhat )
		{
			if ( CollectionEnabled( s, coll ) )
				return;
			warnings.Add( new ErrorCheckWarning(
				ErrorCheckSection.DisabledCollections,
				owner,
				T( $" {usedWhat} from {coll!.Name}." ) ) );
		}

		/// <summary>
		/// Returns pass/fail(bool), msgbox text(string), msgbox title(string)
		/// </summary>
		public static Tuple<bool, string, string> CheckInteraction( IInteraction interaction )
		{
			return new Tuple<bool, string, string>( true, "", "" );
		}
	}
}
