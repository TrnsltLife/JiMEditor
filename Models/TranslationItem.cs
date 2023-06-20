using System.Windows.Documents;
using System;
using System.Windows.Markup;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Newtonsoft.Json;
using JiME.UserControls;

namespace JiME
{
	public class TranslationItem : INotifyPropertyChanged
	{

		string _key, _text;
		bool _translationOK;
		bool _superfluous;
		bool _added;
		bool _deleted;
		[JsonIgnore]
		bool _updatedWhileEditing = false;

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
			this.translationOK = true;
        }

		public TranslationItem(string key, string text)
		{
			this.key = key;
			this.text = text;
			this.translationOK = true;
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
	}
}
