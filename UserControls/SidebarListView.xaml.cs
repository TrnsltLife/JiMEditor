using System;
using System.Windows.Controls;
using System.ComponentModel;

namespace JiME.UserControls
{
	/// <summary>
	/// Interaction logic for SidebarListView.xaml
	/// </summary>
	public partial class SidebarListView : UserControl, INotifyPropertyChanged
	{
		public string Title { get; set; }
		public Array ListData { get; set; }

		public EventHandler onAddEvent, onRemoveEvent, onSettingsEvent, onDuplicateEvent;
		public bool _showDuplicateButton = false;
		public string _duplicateButtonVisibility = "Collapsed";

        public event Action<string> NewItemSelectedEvent;

		public bool ShowDuplicateButton
		{
			get => _showDuplicateButton;
			set
			{
				_showDuplicateButton = value;
				_duplicateButtonVisibility = _showDuplicateButton ? "Visible" : "Collapsed";
				PropChanged("ShowDuplicateButton");
			}
		}

		public string DuplicateButtonVisibility
        {
			get => _duplicateButtonVisibility;
			set
			{
				_duplicateButtonVisibility = value;
				PropChanged("DuplicateButtonVisiblity");
			}
        }

		public SidebarListView()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void AddInteraction_Click( object sender, System.Windows.RoutedEventArgs e )
		{
			onAddEvent?.Invoke( sender, e );
		}

		private void Settings_Click( object sender, System.Windows.RoutedEventArgs e )
		{
			onSettingsEvent?.Invoke( sender, e );
		}

		private void RemoveInteraction_Click( object sender, System.Windows.RoutedEventArgs e )
		{
			onRemoveEvent?.Invoke( sender, e );
		}

        private void DataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var newSelection = e.AddedItems[0];
                if (newSelection is IInteraction i)
                {
                    NewItemSelectedEvent?.Invoke(i.dataName);
                }
                else if (newSelection is ICommonData c)
                {
                    NewItemSelectedEvent?.Invoke(c.dataName);
                }
            }
        }

        private void Duplicate_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			onDuplicateEvent?.Invoke(sender, e);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void PropChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
