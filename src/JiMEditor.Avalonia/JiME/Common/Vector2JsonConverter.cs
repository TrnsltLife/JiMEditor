// Ported for Mac MVP: WPF System.Windows.Vector serialized as "X,Y" strings in .jime (2026-08-13).
using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiME
{
	/// <summary>
	/// Matches WPF <c>System.Windows.Vector</c> JSON: a string <c>"X,Y"</c> (InvariantCulture numbers).
	/// Also accepts object <c>{ "X", "Y" }</c> and array <c>[X, Y]</c> for robustness.
	/// </summary>
	public sealed class Vector2JsonConverter : JsonConverter<Vector2>
	{
		public override Vector2 ReadJson( JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer )
		{
			if ( reader.TokenType == JsonToken.Null )
				return default;

			if ( reader.TokenType == JsonToken.String )
			{
				var s = (string?)reader.Value;
				if ( string.IsNullOrWhiteSpace( s ) )
					return default;
				return ParseCommaSeparated( s );
			}

			if ( reader.TokenType == JsonToken.StartObject )
			{
				var obj = JObject.Load( reader );
				return new Vector2(
					obj.GetValue( "X", StringComparison.OrdinalIgnoreCase )?.Value<double>() ?? 0,
					obj.GetValue( "Y", StringComparison.OrdinalIgnoreCase )?.Value<double>() ?? 0 );
			}

			if ( reader.TokenType == JsonToken.StartArray )
			{
				var arr = JArray.Load( reader );
				if ( arr.Count < 2 )
					throw new JsonSerializationException( "Vector2 array must have at least two elements." );
				return new Vector2( arr[0].Value<double>(), arr[1].Value<double>() );
			}

			throw new JsonSerializationException( $"Unexpected token {reader.TokenType} when parsing Vector2." );
		}

		public override void WriteJson( JsonWriter writer, Vector2 value, JsonSerializer serializer )
		{
			// Preserve WPF Vector TypeConverter output so companion / WPF editors keep reading the same shape.
			writer.WriteValue( string.Format( CultureInfo.InvariantCulture, "{0},{1}", value.X, value.Y ) );
		}

		static Vector2 ParseCommaSeparated( string s )
		{
			// WPF VectorConverter (en-US): ListSeparator is ',' between components; numbers use Invariant-style '.'.
			var parts = s.Split( ',' );
			if ( parts.Length != 2 )
				throw new JsonSerializationException( $"Invalid Vector2 string '{s}'. Expected \"X,Y\"." );

			if ( !double.TryParse( parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var x )
				|| !double.TryParse( parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var y ) )
			{
				throw new JsonSerializationException( $"Invalid Vector2 string '{s}'. Expected numeric \"X,Y\"." );
			}

			return new Vector2( x, y );
		}
	}
}
