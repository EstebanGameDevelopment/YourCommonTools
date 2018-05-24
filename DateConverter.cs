using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourCommonTools
{
	public class DateConverter
	{
		private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		// -------------------------------------------
		/* 
		 * GetTimestamp
		 */
		public static long GetTimestamp()
		{
			return (long)((DateTime.UtcNow - Jan1St1970).TotalSeconds);
		}

		// -------------------------------------------
		/* 
		 * GetDaysFromSeconds
		 */
		public static int GetDaysFromSeconds(long _seconds)
		{
			return (int)(((_seconds / 60) / 60) / 24);
		}

		// -------------------------------------------
		/* 
		 * TimeStampToDateTime
		 */
		public static string TimeStampToDateTimeString(double _unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(_unixTimeStamp).ToLocalTime();
			return dtDateTime.Month + "/" + dtDateTime.Day + "/" + dtDateTime.Year;
		}

		// -------------------------------------------
		/* 
		 * TimeStampToDateTime
		 */
		public static System.DateTime TimeStampToDateTime(double _unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(_unixTimeStamp).ToLocalTime();
			return dtDateTime;
		}

		// -------------------------------------------
		/* 
		 * TimeStampToDateTime
		 */
		public static double DateTimeToTimeStamp(System.DateTime _dateTime)
		{
			// Unix timestamp is seconds past epoch
			return (TimeZoneInfo.ConvertTimeToUtc(_dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
		}

		// -------------------------------------------
		/* 
		 * GetTimeDifferenceInHours
		 */
		public static int GetTimeDifferenceInHours(long _oldTimestamp)
		{
			// Unix timestamp is seconds past epoch
			long seconds = GetTimestamp() - _oldTimestamp;
			int minutes = (int)(seconds / 60);
			int hours = (int)(minutes / 60);
			return hours;
		}
		
	}
	
}
