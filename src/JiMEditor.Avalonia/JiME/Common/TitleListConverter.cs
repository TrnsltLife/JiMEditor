using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiME
{
	public class TitleListConverter : JsonConverter
	{
		public override bool CanWrite => true;
		public override bool CanRead => true;
		public override bool CanConvert( Type objectType )
		{
			return objectType == typeof( Item );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			ObservableCollection<Title> titleList = (ObservableCollection<Title>)value;
			List<int> idList = new List<int>();
			foreach (Title title in titleList)
            {
				idList.Add(title.id);
			}
			JToken t = JToken.FromObject(idList);
			t.WriteTo(writer);
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			var jsonObject = JArray.Load( reader );
			Title title = null;
			List<int> idList = new List<int>();
			ObservableCollection<Title> titleList = new ObservableCollection<Title>();

			foreach (var obj in jsonObject)
			{
				title = Titles.FromID(obj.Value<int>());
				titleList.Add(title);
			}

			return titleList;
		}
	}
}
