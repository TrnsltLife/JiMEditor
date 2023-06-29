using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using JiME.Models;

namespace JiME
{
	public class ThreatInteraction : InteractionBase, INotifyPropertyChanged, ICommonData
	{
		override protected void DefineTranslationAccessors()
		{
			translationKeyParents = "enemy";
			base.DefineTranslationAccessors();
		}

		string _triggerDefeatedName;
		bool[] _includedEnemies = new bool[Collection.MONSTERS().Length].Fill(false);
		int _basePoolPoints;
		DifficultyBias _difficultyBias;

		public string triggerDefeatedName
		{
			get { return _triggerDefeatedName; }
			set
			{
				_triggerDefeatedName = value;
				NotifyPropertyChanged( "triggerDefeatedName" );
			}
		}
		public int basePoolPoints
		{
			get => _basePoolPoints;
			set
			{
				_basePoolPoints = value;
				NotifyPropertyChanged( "basePoolPoints" );
			}
		}
		public bool[] includedEnemies
		{
			get => _includedEnemies;
			set
			{
				_includedEnemies = value;
				NotifyPropertyChanged( "includedEnemies" );
			}
		}
		public DifficultyBias difficultyBias
		{
			get => _difficultyBias;
			set
			{
				_difficultyBias = value;
				NotifyPropertyChanged( "difficultyBias" );
			}
		}

		public ObservableCollection<Monster> monsterCollection { get; set; }

		public ThreatInteraction( string name ) : base( name )
		{
			interactionType = InteractionType.Threat;

			triggerDefeatedName = "None";
			includedEnemies = new bool[Collection.MONSTERS().Length].Fill( false );
			//include the enemies from the Core Set by default - commented out
			//for (int i = 0; i < 7; i++) { includedEnemies[i] = true; }
			basePoolPoints = 10;
			difficultyBias = DifficultyBias.Medium;

			monsterCollection = new ObservableCollection<Monster>();
		}

		public ThreatInteraction Clone()
		{
			ThreatInteraction interact = new ThreatInteraction("");
			base.CloneInto(interact);
			interact.triggerDefeatedName = this.triggerDefeatedName;
			interact.includedEnemies = (bool[])this.includedEnemies.Clone();
			interact.basePoolPoints = this.basePoolPoints;
			interact.difficultyBias = this.difficultyBias;
			interact.triggerDefeatedName = this.triggerDefeatedName;
			interact.monsterCollection = new ObservableCollection<Monster>();
			foreach(var monster in this.monsterCollection)
            {
				interact.monsterCollection.Add(monster.Clone());
            }
			return interact;
		}

		new public void RenameTrigger( string oldName, string newName )
		{
			base.RenameTrigger( oldName, newName );

			if ( triggerDefeatedName == oldName )
				triggerDefeatedName = newName;
		}

		public void AddMonster( Monster m )
		{
			monsterCollection.Add( m );
		}

		//Help handle a situation where an old file type had less enemies available than we do now.
		public void ResizeIncludedEnemies()
        {
			bool[] newArray = new bool[Collection.MONSTERS().Length].Fill(false);
			for(int i=0; i<_includedEnemies.Length; i++)
            {
				newArray[i] = _includedEnemies[i];
            }
			_includedEnemies = newArray;
        }
	}
}
