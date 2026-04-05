using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiME
{
	public class MonsterModifierListConverter : JsonConverter
	{
		public override bool CanWrite => true;
		public override bool CanRead => true;
		public override bool CanConvert( Type objectType )
		{
			return objectType == typeof( MonsterModifier );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			ObservableCollection<MonsterModifier> modifierList = (ObservableCollection<MonsterModifier>)value;
			List<int> idList = new List<int>();
			foreach (MonsterModifier modifier in modifierList)
            {
				idList.Add(modifier.id);
			}
			JToken t = JToken.FromObject(idList);
			t.WriteTo(writer);
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			var jsonObject = JArray.Load( reader );
			MonsterModifier modifier = null;
			List<int> idList = new List<int>();
			ObservableCollection<MonsterModifier> modifierList = new ObservableCollection<MonsterModifier>();

			foreach (var item in jsonObject)
			{
				modifier = MonsterModifier.FromID(item.Value<int>());
				modifierList.Add(modifier);
			}

			return modifierList;
		}
	}
}
