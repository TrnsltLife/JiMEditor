﻿using System;
using System.Collections.Generic;
using JiME.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiME
{
	public class CollectionConverter : JsonConverter
	{
		public override bool CanWrite => true;
		public override bool CanRead => true;
		public override bool CanConvert( Type objectType )
		{
			return objectType == typeof( string );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			List<Collection> collectionList = (List<Collection>)value;
			List<string> nameList = new List<string>();
			collectionList.ForEach(it => nameList.Add(it.Name));
			JToken t = JToken.FromObject(nameList);
			t.WriteTo(writer);
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			var jsonObject = JArray.Load( reader );
			Collection collection = null;
			List<string> nameList = new List<string>();
			List<Collection> collectionList = new List<Collection>();

			foreach ( var item in jsonObject )
			{
				collection = Collection.FromName(item.Value<string>());
				collectionList.Add( collection );
			}

			return collectionList;
		}
	}
}