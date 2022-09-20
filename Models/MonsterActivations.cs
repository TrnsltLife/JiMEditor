﻿using JiME.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JiME
{
	/// <summary>
	/// cf. https://boardgamegeek.com/thread/2469108/demystifying-enemies-project-documenting-enemy-sta
	/// </summary>
	public class MonsterActivations : INotifyPropertyChanged, ICommonData
	{

		string _dataName;
		int _id;
		Collection _collection = Collection.NONE;
		ObservableCollection<MonsterActivationItem> _activations = new ObservableCollection<MonsterActivationItem>();

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

		public ObservableCollection<MonsterActivationItem> activations
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

		public MonsterActivations(int id)
		{
			this.id = id;
			this.dataName = "New Enemy Activations";
		}

		public MonsterActivations(DefaultActivations act)
		{
			id = act.id;
			dataName = act.name;
			collection = act.collection;
			activations = new ObservableCollection<MonsterActivationItem>();
			for(int i=0; i<act.activations.Length; i++)
            {
				activations.Add(new MonsterActivationItem(act.activations[i]));
            }
		}

		public void NotifyPropertyChanged( string propName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}

		public MonsterActivations Clone(int newId)
        {
			MonsterActivations newActivations = new MonsterActivations { id = newId, dataName = "Copy of " + this.dataName, collection = this.collection };
			newActivations.activations = new ObservableCollection<MonsterActivationItem>();
			for(int i=0; i<activations.Count; i++)
            {
                newActivations.activations.Add(activations[i].Clone());
            }
			return newActivations;
        }

		public void RenumberActivations()
		{
			int i = 1;
			foreach (var act in activations)
			{
				act.id = i;
				i++;
			}
			NotifyPropertyChanged("activations");
		}
	}
}
