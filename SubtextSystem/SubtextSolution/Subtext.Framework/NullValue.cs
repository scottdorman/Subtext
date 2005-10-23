using System;

namespace Subtext.Framework
{
	/// <summary>
	/// Constants used to represent null value type.
	/// </summary>
	public sealed class NullValue
	{
		private NullValue()
		{
		}

		/// <summary>Represents a null integer.</summary>
		public const int NullInt32 = Int32.MinValue;

		/// <summary>Represents a null double.</summary>
		public const double NullDouble = double.MinValue;

		/// <summary>Represents a null DateTime</summary>
		public static DateTime NullDateTime
		{
			get
			{
				return DateTime.MinValue;
			}
		}

		/// <summary>
		/// Determines whether the specified value is null.
		/// </summary>
		/// <param name="integer">The value.</param>
		/// <returns>
		/// 	<c>true</c> if the specified value is null; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNull(int integer)
		{
			return integer == NullInt32;
		}

		/// <summary>
		/// Determines whether the specified number is null.
		/// </summary>
		/// <param name="number">The number.</param>
		/// <returns>
		/// 	<c>true</c> if the specified number is null; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNull(double number)
		{
			return number == NullDouble;
		}

		/// <summary>
		/// Determines whether the specified date time is null.
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		/// <returns>
		/// 	<c>true</c> if the specified date time is null; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNull(DateTime dateTime)
		{
			return dateTime == NullDateTime;
		}
	}
}
