﻿using System;
using System.Collections.Generic;
using JiME.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiME
{
	public class ActivationsListConverter : JsonConverter
	{
		public override bool CanWrite => true;
		public override bool CanRead => true;
		public override bool CanConvert( Type objectType )
		{
			return objectType == typeof( string );
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			List<MonsterActivations> activationsList = (List<MonsterActivations>)value;
			List<MonsterActivations> filteredList = new List<MonsterActivations>();
			activationsList.FindAll(it => it.id >= 1000).ForEach(it => filteredList.Add(it));
			JToken t = JToken.FromObject(filteredList);
			t.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jsonObject = JArray.Load(reader);
			MonsterActivations activations = null;

			List<MonsterActivations> convertedList = new List<MonsterActivations>();
			List<MonsterActivations> activationsList = new List<MonsterActivations>();

			foreach (var item in jsonObject)
			{
				activations = item.ToObject<MonsterActivations>();
				convertedList.Add(activations);
			}

			return convertedList;
		}
	}
}