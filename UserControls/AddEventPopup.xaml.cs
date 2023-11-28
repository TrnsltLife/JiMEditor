using System.Windows;
using System.Windows.Controls;
using System;
using System.ComponentModel;
using System.Windows.Input;
using JiME.Views;

namespace JiME.UserControls
{
	/// <summary>
	/// Interaction logic for AddEventPopup.xaml
	/// </summary>
	public partial class AddEventPopup : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty ScenarioProperty =
			DependencyProperty.Register("Scenario", typeof(Scenario),
			typeof(AddEventPopup), new FrameworkPropertyMetadata
			(null, new PropertyChangedCallback(OnScenarioChanged)));


		Scenario scenario;
		public Scenario Scenario
		{
			get => scenario;
			set
			{
				if (scenario != value)
				{
					scenario = value;
					PropChanged("Scenario");
				}
			}
		}

		public static void OnScenarioChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			AddEventPopup aep = (AddEventPopup)obj;
			aep.Scenario = (Scenario)args.NewValue;
		}



		Action<InteractionBase> actionToHandleCreatedEvent;
		public Action<InteractionBase> ActionToHandleCreatedEvent
		{
			get => actionToHandleCreatedEvent;
			set
			{
				if (actionToHandleCreatedEvent != value)
				{
					actionToHandleCreatedEvent = value;
					PropChanged("actionToHandleCreatedEvent");
				}
			}
		}
		public static void OnActionToHandleCreatedEventChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			AddEventPopup aep = (AddEventPopup)obj;
			aep.ActionToHandleCreatedEvent = (Action<InteractionBase>)args.NewValue;
		}       
		
		public static readonly DependencyProperty ActionToHandleCreatedEventProperty =
			DependencyProperty.Register("ActionToHandleCreatedEvent", typeof(Action<InteractionBase>),
			typeof(AddEventPopup), new FrameworkPropertyMetadata
			(null, new PropertyChangedCallback(OnActionToHandleCreatedEventChanged)));


		public void HandleCreatedEvent(InteractionBase ib)
		{
			if (ActionToHandleCreatedEvent != null)
			{
				ActionToHandleCreatedEvent.Invoke(ib);
			}
		}

		public AddEventPopup()
		{
			Debug.Log("AddEventPopup constructor");
			InitializeComponent();
			//DataContext = this;

		}

		protected override void OnInitialized(EventArgs e)
        {
			base.OnInitialized(e);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void PropChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void addInteraction_Click(object sender, RoutedEventArgs e)
		{
			ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
			cm.PlacementTarget = sender as Button;
			cm.IsOpen = true;
		}

		//Interaction popup commands
		private void CommandNewTextInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TextInteractionWindow ew = new TextInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				Debug.Log("Adding new event: " + ew.interaction.dataName);
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewTextInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewBranchInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			BranchInteractionWindow ew = new BranchInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewBranchInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewThreatInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ThreatInteractionWindow ew = new ThreatInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewThreatInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewTestInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TestInteractionWindow ew = new TestInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewTestInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewDecisionInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DecisionInteractionWindow ew = new DecisionInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewDecisionInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewMultiInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MultiEventWindow ew = new MultiEventWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewMultiInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewPersistentInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			PersistentInteractionWindow ew = new PersistentInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewPersistentInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewConditionalInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ConditionalInteractionWindow ew = new ConditionalInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewConditionalInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewDialogInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogInteractionWindow ew = new DialogInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewDialogInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewReplaceTokenInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ReplaceTokenInteractionWindow ew = new ReplaceTokenInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewReplaceTokenInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
		private void CommandNewRewardInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			RewardInteractionWindow ew = new RewardInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewRewardInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CommandNewItemInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ItemInteractionWindow ew = new ItemInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewItemInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CommandNewTitleInteraction_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ItemInteractionWindow ew = new ItemInteractionWindow(scenario);
			if (ew.ShowDialog() == true)
			{
				scenario.AddInteraction(ew.interaction);
				HandleCreatedEvent(ew.interaction);
			}
		}
		private void CommandNewTitleInteraction_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
	}
}
