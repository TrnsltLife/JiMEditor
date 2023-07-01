using System.Windows.Documents;
using System;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Newtonsoft.Json;
using JiME.UserControls;
using System.Windows.Media;

namespace JiME
{
	public class TranslationItem : INotifyPropertyChanged, IComparable<TranslationItem>
	{

		string _key, _text;
		bool _translationOK;

		//The following items are just used during editing the translation list and translation text to keep track of state
		[JsonIgnore]
		bool _superfluous; //item exists in the translation but not in the defaultTranslation
		[JsonIgnore]
		bool _added; //item added during editing
        [JsonIgnore]
		bool _deleted; //item deleted during eidting
		[JsonIgnore]
		bool _updatedWhileEditing; //item updated during editing (text changed)


        [JsonIgnore]
		public Brush Background
        {
			get {
				return new SolidColorBrush(
					superfluous ? Colors.Gray :
					translationOK ? Color.FromRgb(0x46, 0x46, 0x4a) : Colors.DarkOrange); }
        }

		public Guid GUID { get; set; }

		public string key
		{
			get => _key;
			set
			{
				if (value != _key)
				{
					_key = value;
					NotifyPropertyChanged("key");
				}
			}
		}

		public string text
		{
			get => _text;
			set
			{
				if (value != _text)
				{
					_text = value;
					NotifyPropertyChanged("text");
				}
			}
		}

		public bool translationOK
		{
			get => _translationOK;
			set
			{
				if (value != _translationOK)
				{
					_translationOK = value;
					NotifyPropertyChanged("translationOK");
				}
			}
		}

        [JsonIgnore]
		public bool superfluous
		{
			get => _superfluous;
			set
			{
				if (value != _superfluous)
				{
					_superfluous = value;
					NotifyPropertyChanged("superfluous");
				}
			}
		}

		[JsonIgnore]
		public bool added
		{
			get => _added;
			set
			{
				if (value != _added)
				{
					_added = value;
					NotifyPropertyChanged("added");
				}
			}
		}

		[JsonIgnore]
		public bool deleted
		{
			get => _deleted;
			set
			{
				if (value != _deleted)
				{
					_deleted = value;
					NotifyPropertyChanged("deleted");
				}
			}
		}

		[JsonIgnore]
		public bool updatedWhileEditing
		{
			get => _updatedWhileEditing;
			set
			{
				if (value != _updatedWhileEditing)
				{
					_updatedWhileEditing = value;
					NotifyPropertyChanged("updatedWhileEditing");
				}
			}
		}

		[JsonIgnore]
		public FlowDocument TextFlowDocument
        {
			//Transform the text to display icons properly in a TextBlock
			get => CreateFlowDocumentFromSimpleHtml(text);
        }

		private FlowDocument CreateFlowDocumentFromSimpleHtml(string html)
		{
			if (html == null) { return null; }
			return RichTextBoxIconEditor.CreateFlowDocumentFromSimpleHtml(html, "", "FontFamily=\"Segoe UI\" FontSize=\"12\"");
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public TranslationItem() 
		{
			key = Guid.NewGuid().ToString();
		}

		public TranslationItem(string key)
        {
			this.key = key;
			this.text = "";
			this.translationOK = false;
        }

		public TranslationItem(string key, string text)
		{
			this.key = key;
			this.text = text;
			this.translationOK = false;
		}

		public TranslationItem(string key, string text, bool translationOK)
		{
			this.key = key;
			this.text = text;
			this.translationOK = translationOK;
		}

		public TranslationItem Clone()
        {
			TranslationItem item = new TranslationItem();
			item.key = this.key;
			item.text = this.text;
			item.translationOK = this.translationOK;
			return item;
        }

		public void NotifyPropertyChanged( string propName )
		{
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}

        public int CompareTo(TranslationItem other)
        {
			if(other == null) { return key.CompareTo(null); }
            return key.CompareTo(other.key);
        }
    }
}
