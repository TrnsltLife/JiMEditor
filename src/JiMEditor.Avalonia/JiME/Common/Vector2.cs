// Ported for Mac MVP: replaces System.Windows.Vector / Point for .jime X/Y fields (2026-08-09).
using System;

namespace JiME
{
	/// <summary>
	/// Portable 2D vector with JSON shape <c>{ "X": …, "Y": … }</c> matching WPF Vector/Point serialization.
	/// Properties are settable so Newtonsoft.Json can round-trip the same as WPF Vector.
	/// </summary>
	public struct Vector2 : IEquatable<Vector2>
	{
		public double X { get; set; }
		public double Y { get; set; }

		public Vector2( double x, double y )
		{
			X = x;
			Y = y;
		}

		public static Vector2 Zero => new( 0, 0 );

		public bool Equals( Vector2 other ) => X.Equals( other.X ) && Y.Equals( other.Y );
		public override bool Equals( object? obj ) => obj is Vector2 other && Equals( other );
		public override int GetHashCode() => HashCode.Combine( X, Y );
		public override string ToString() => $"{X},{Y}";

		public static bool operator ==( Vector2 left, Vector2 right ) => left.Equals( right );
		public static bool operator !=( Vector2 left, Vector2 right ) => !left.Equals( right );

		public static Vector2 operator +( Vector2 a, Vector2 b ) => new( a.X + b.X, a.Y + b.Y );
		public static Vector2 operator -( Vector2 a, Vector2 b ) => new( a.X - b.X, a.Y - b.Y );
	}
}
