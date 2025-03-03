using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiME
{
	public class InteractionConverter : JsonConverter
	{
		public override bool CanWrite => false;
		public override bool CanRead => true;
		public override bool CanConvert( Type objectType )
		{
			return objectType == typeof( IInteraction );
		}

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			throw new InvalidOperationException( "Use default serialization." );
			//throw new NotImplementedException();
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			var item = JValue.Load(reader);
			//var item = new JValue(reader.Value);
			var interaction = default( IInteraction );

			switch ( item["interactionType"].Value<int>() )
			{
				case 0:
					interaction = item.ToObject<TextInteraction>();
					break;
				case 1:
					interaction = item.ToObject<ThreatInteraction>();
					break;
				case 2:
					interaction = item.ToObject<TestInteraction>();
					break;
				case 3:
					interaction = item.ToObject<DecisionInteraction>();
					break;
				case 4:
					interaction = item.ToObject<BranchInteraction>();
					break;
				case 5:
					interaction = item.ToObject<DarknessInteraction>();
					break;
				case 6:
					interaction = item.ToObject<MultiEventInteraction>();
					break;
				case 7:
					interaction = item.ToObject<PersistentTokenInteraction>();
					break;
				case 8:
					interaction = item.ToObject<ConditionalInteraction>();
					break;
				case 9:
					interaction = item.ToObject<DialogInteraction>();
					break;
				case 10:
					interaction = item.ToObject<ReplaceTokenInteraction>();
					break;
				case 11:
					interaction = item.ToObject<RewardInteraction>();
					break;
				case 12:
					interaction = item.ToObject<ItemInteraction>();
					break;
				case 13:
					interaction = item.ToObject<TitleInteraction>();
					break;
				case 14:
					interaction = item.ToObject<StartInteraction>();
					break;
				case 15:
					interaction = item.ToObject<CorruptionInteraction>();
					break;
				default:
					throw new Exception( "IInteraction not recognized:\r\n" + item.ToString() );
			}

			return interaction;
		}

	}
}
