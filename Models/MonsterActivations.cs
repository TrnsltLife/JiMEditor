using JiME.Models;
using System;
using System.ComponentModel;
using System.Linq;

namespace JiME
{
	/// <summary>
	/// cf. https://boardgamegeek.com/thread/2469108/demystifying-enemies-project-documenting-enemy-sta
	/// </summary>
	public class MonsterActivations : INotifyPropertyChanged, ICommonData
	{

		string _dataName;
		int _id;
		Collection _collection;
		MonsterActivationItem[] _activations;

		public Guid GUID { get; set; }

		public int id
		{
			get => _id;
			set
			{
				if (value != _id)
				{
					_id = value;
					NotifyPropertyChanged("id");
				}
			}
		}

		public string dataName
		{
			get => _dataName;
			set
			{
				if (value != _dataName)
				{
					_dataName = value;
					NotifyPropertyChanged("dataName");
				}
			}
		}

		public bool isEmpty { get; set; }
		public string triggerName { get; set; }

		public Collection collection
		{
			get => _collection;
			set
			{
				if (value != _collection)
				{
					_collection = value;
					NotifyPropertyChanged("collection");
				}
			}
		}

		public MonsterActivationItem[] activations
		{
			get => _activations;
			set
			{
				if (value != _activations)
				{
					_activations = value;
					NotifyPropertyChanged("activations");
				}
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public MonsterActivations() { }

		public MonsterActivations(DefaultActivations act)
		{
			id = act.id;
			dataName = act.name;
			collection = act.collection;
			activations = new MonsterActivationItem[act.activations.Length];
			for(int i=0; i<act.activations.Length; i++)
            {
				activations[i] = new MonsterActivationItem(act.activations[i]);
            }
		}

		public void NotifyPropertyChanged( string propName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}

		public MonsterActivations Clone(int newId)
        {
			MonsterActivations newActivations = new MonsterActivations { id = newId, dataName = "Copy of " + this.dataName, collection = this.collection };
			newActivations.activations = new MonsterActivationItem[activations.Length];
			for(int i=0; i<activations.Length; i++)
            {
                newActivations.activations[i] = activations[i].Clone();
            }
			return newActivations;
        }
	}
}
