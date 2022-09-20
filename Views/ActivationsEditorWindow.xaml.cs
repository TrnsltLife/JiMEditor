using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using JiME.Models;

namespace JiME.Views
{
	/// <summary>
	/// Interaction logic for ActivationsEditorWindow.xaml
	/// </summary>
	public partial class ActivationsEditorWindow : Window, INotifyPropertyChanged
	{
		string oldName;

		public Scenario scenario { get; set; }
		public MonsterActivations activations { get; set; }
		bool closing = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public ActivationsEditorWindow( Scenario s, MonsterActivations activ = null )
		{
			InitializeComponent();
			DataContext = this;

			scenario = s;
			cancelButton.Visibility = activ == null ? Visibility.Visible : Visibility.Collapsed;
			lockIcon.Visibility = activ == null ? Visibility.Collapsed : activ.id < 1000 ? Visibility.Visible : Visibility.Collapsed;
			nameTB.IsEnabled = activ == null ? true : activ.id < 2000 ? false : true;
			activations = activ ?? new MonsterActivations();

			oldName = activations.dataName;
		}

		bool TryClosing()
		{
			//check for dupe name
			if ( activations.dataName == "New Enemy Activations" || scenario.activationsObserver.Count( x => x.dataName == activations.dataName ) > 1 )
			{
				MessageBox.Show( "Give this Enemy Activations group a unique name.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error );
				return false;
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

			//TODO?
			//scenario.UpdateActivationsReferences( oldName, activations );

			closing = true;
			DialogResult = true;
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			closing = true;
			DialogResult = false;
		}

		private void Window_ContentRendered( object sender, System.EventArgs e )
		{
			nameTB.Focus();
			nameTB.SelectAll();
		}

		private void AddActivationButton_Click( object sender, RoutedEventArgs e )
		{
			MonsterActivationItem mai = new MonsterActivationItem(activations.activations.Count + 1);
			MonsterActivationItemEditorWindow maie = new MonsterActivationItemEditorWindow(scenario, activations, mai, true);
			if (maie.ShowDialog() == true)
			{
				activations.activations.Add(mai);
				NotifyItemChanged(mai);
			}
		}

		private void EditButton_Click( object sender, RoutedEventArgs e )
		{
			MonsterActivationItem mai = ( (Button)sender ).DataContext as MonsterActivationItem;
			MonsterActivationItemEditorWindow maie = new MonsterActivationItemEditorWindow(scenario, activations, mai, false);
			maie.ShowDialog();
			NotifyItemChanged(mai);
		}

		private void NotifyItemChanged(MonsterActivationItem mai)
        {
			mai.NotifyPropertyChanged("damage");
			mai.NotifyPropertyChanged("fear");
			mai.UpdateValid();
		}

		private void DeleteButton_Click( object sender, RoutedEventArgs e )
		{
			MonsterActivationItem mai = ((Button)sender).DataContext as MonsterActivationItem;
			activations.activations.Remove(mai);
			activations.RenumberActivations();
		}

		void PropChanged( string name )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
		}

		public void NotifyPropertyChanged(string propName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		private void nameTB_TextChanged( object sender, TextChangedEventArgs e )
		{
			activations.dataName = ( (TextBox)sender ).Text;
		}
	}
}
