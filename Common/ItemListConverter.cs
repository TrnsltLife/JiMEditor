using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiME
{
	public class ItemListConverter : JsonConverter
	{
		public override bool CanWrite => true;
		public override bool CanRead => true;
		public override bool CanConvert( Type objectType )
		{
			return objectType == typeof( Item );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			ObservableCollection<Item> itemList = (ObservableCollection<Item>)value;
			List<int> idList = new List<int>();
			foreach (Item item in itemList)
            {
				idList.Add(item.id);
			}
			JToken t = JToken.FromObject(idList);
			t.WriteTo(writer);
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			var jsonObject = JArray.Load( reader );
			Item item = null;
			List<int> idList = new List<int>();
			ObservableCollection<Item> itemList = new ObservableCollection<Item>();

			foreach (var obj in jsonObject)
			{
				item = Items.FromID(obj.Value<int>());
				itemList.Add(item);
			}

			return itemList;
		}
	}
}
