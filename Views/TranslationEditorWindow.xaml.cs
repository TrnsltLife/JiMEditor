using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for TranslationEditorWindow.xaml
	/// </summary>
	public partial class TranslationEditorWindow : Window, INotifyPropertyChanged
	{
		string oldLangCode;

		public Scenario scenario { get; set; }
		public Translation translation { get; set; }
		public Translation translationInitialState { get; set; }
		public Dictionary<string, TranslationItem> defaultTranslation;
		bool closing = false;
		bool isNew = false;
		Dictionary<string, TranslationItem> translationDict;

		public event PropertyChangedEventHandler PropertyChanged;

		public TranslationEditorWindow( Scenario s, Dictionary<string, TranslationItem> defaultTranslation, Translation translation = null, bool isNew = true )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			cancelButton.Visibility = translation == null ? Visibility.Visible : Visibility.Collapsed;
			//lockIcon.Visibility = activ == null ? Visibility.Collapsed : activ.id < MonsterActivations.START_OF_CUSTOM_ACTIVATIONS ? Visibility.Visible : Visibility.Collapsed;
			this.translationInitialState = translation ?? new Translation();
			this.defaultTranslation = defaultTranslation;
			TranslationUnion();

			oldLangCode = translation.dataName;

			cancelButton.Visibility = isNew ? Visibility.Visible : Visibility.Collapsed;
			this.isNew = isNew;
		}

		public void TranslationUnion()
        {
			translationDict = new Dictionary<string, TranslationItem>(); //hold the keys from the language translation for easy lookup as we add in keys from the default translation that are missing in the language translation
			translation = new Translation(translationInitialState.dataName);
			translation.langName = translationInitialState.langName;

			//First copy all the TranslationItems from the language translation into the translation list
			foreach(var item in translationInitialState.translationItems)
            {
				TranslationItem clonedItem = item.Clone();
				clonedItem.superfluous = !defaultTranslation.ContainsKey(item.key); //If it's not in the defaultTranslation, we really don't need this item anymore. But it might have a translated string the creator still needs to reference so don't delete it automatically.
				translation.translationItems.Add(clonedItem);
				translationDict.Add(item.key, clonedItem);
            }

			//Next copy all the TranslationItems from defaultTranslation into translation, if that key isn't already in translation. This represents strings that haven't been translated yet.
			foreach(var entity in defaultTranslation)
            {
				if(!translationDict.ContainsKey(entity.Key))
                {
					//Clone the defaultTranslation item with the key and text
					TranslationItem clonedItem = entity.Value.Clone();
					clonedItem.translationOK = false;
					clonedItem.text = ""; //actually, don't set the text
					translation.translationItems.Add(clonedItem);
					translationDict.Add(entity.Key, clonedItem);
				}
			}

			//Make sure all the items are sorted alphabetically
			List<TranslationItem> sortedList = translation.translationItems.ToList();
			sortedList.Sort();
			translation.translationItems.Clear();
			foreach (var item in sortedList)
			{
				translation.translationItems.Add(item);
			}
		}

		bool TryClosing()
		{
			//Console.WriteLine("TranslationEditorWindow TryClosing with translation.dataName " + translation.dataName + " matching count: " + scenario.translationObserver.Count(it => it.dataName == translation.dataName));
			//check for dupe name
			if ( translation.dataName == "" || scenario.translationObserver.Count( it => it.dataName == translation.dataName ) > (isNew ? 0 : 1) )
			{
				MessageBox.Show( "Give this Translation a unique language code.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
			}
			else if ( translation.langName == "")
            {
				MessageBox.Show("Give this Translation a language name so the players can tell what language it is.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			return true;
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			if ( !closing )
				e.Cancel = true;
		}

		private void OkButton_Click( object sender, RoutedEventArgs e )
		{
			if ( !TryClosing() )
				return;

			closing = true;
			DialogResult = true;

			//Collect the data to put back into translationInitialState
			translationInitialState.dataName = translation.dataName;
			translationInitialState.langName = translation.langName;
			translationInitialState.translationItems.Clear();
			foreach (var item in translation.translationItems)
			{
				if(defaultTranslation.ContainsKey(item.key) && item.text == defaultTranslation[item.key].text && !item.translationOK) { continue; }
				else if ((item.superfluous && item.text.Trim() == "")) { continue; }
				else if ((item.added && item.text.Trim() == "")) { continue; }
				else if (item.text.Trim() == "") { continue; }
				else if (item.deleted) { continue; }
				else
				{
					translationInitialState.translationItems.Add(item);
				}
			}
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			closing = true;
			DialogResult = false;
		}

		private void Window_ContentRendered( object sender, System.EventArgs e )
		{
			langCodeCB.Focus();
		}

		private void AddTranslationButton_Click( object sender, RoutedEventArgs e )
		{
			/*
				TranslationItem ti = new TranslationItem();
				MonsterActivationItemEditorWindow maie = new MonsterActivationItemEditorWindow(scenario, activations, mai, true);
				if (maie.ShowDialog() == true)
				{
					translation.translationItems.Add(ti);
					NotifyItemChanged(ti);
				}
			*/
		}

		private void EditButton_Click( object sender, RoutedEventArgs e )
		{
			TranslationItem ti = ((Button)sender).DataContext as TranslationItem;
			TranslationItemEditorWindow tew = new TranslationItemEditorWindow(ti, defaultTranslation.ContainsKey(ti.key) ? defaultTranslation[ti.key] : new TranslationItem(ti.key, ""));
			tew.ShowDialog();
			NotifyItemChanged(ti);
			/*
				MonsterActivationItem mai = ( (Button)sender ).DataContext as MonsterActivationItem;
				MonsterActivationItemEditorWindow maie = new MonsterActivationItemEditorWindow(scenario, activations, mai, false);
				maie.ShowDialog();
				NotifyItemChanged(mai);
			*/
		}

		private void NotifyItemChanged(TranslationItem ti)
        {
			ti.NotifyPropertyChanged("text");
			ti.NotifyPropertyChanged("translationOK");
		}

		private void DeleteButton_Click( object sender, RoutedEventArgs e )
		{
			
			var ret = MessageBox.Show("Are you sure you want to delete this translation string?\n\nTHIS CANNOT BE UNDONE.", "Delete Translation String", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (ret == MessageBoxResult.Yes)
			{
				
				TranslationItem ti = ((Button)sender).DataContext as TranslationItem;
				translationDict.Remove(ti.key);
				translation.translationItems.Remove(ti);
			}
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public void NotifyPropertyChanged(string propName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		private void langCodeCB_TextChanged( object sender, TextChangedEventArgs e )
		{
			translation.dataName = ( (TextBox)sender ).Text;
		}
	}
}
