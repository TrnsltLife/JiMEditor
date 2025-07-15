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
	public class TranslationItemForExport : IComparable<TranslationItemForExport>
	{

		string _key, _original, _text;


		public string key
		{
			get => _key;
			set
			{
				if (value != _key)
				{
					_key = value;
				}
			}
		}

		public string original
		{
			get => _original;
			set
			{
				if (value != _original)
				{
					_original = value;
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
				}
			}
		}

		public TranslationItemForExport()
        {
			key = Guid.NewGuid().ToString();
		}

		public TranslationItemForExport(string key, string original, string text)
		{
			this.key = key;
			this.original = original;
			this.text = text;
		}

		public TranslationItemForExport Clone()
        {
			TranslationItemForExport item = new TranslationItemForExport();
			item.key = this.key;
			item.original = this.original;
			item.text = this.text;
			return item;
        }

        public int CompareTo(TranslationItemForExport other)
        {
			if(other == null) { return key.CompareTo(null); }
            return key.CompareTo(other.key);
        }
    }
}
