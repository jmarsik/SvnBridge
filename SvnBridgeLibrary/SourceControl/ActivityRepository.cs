using System;
using System.Collections.Generic;
using System.Threading;
using SvnBridge.SourceControl.Dto;

namespace SvnBridge.SourceControl
{
	public static class ActivityRepository
	{
		private static readonly Dictionary<string, DateTime> activitiesTimeStamps = new Dictionary<string, DateTime>();
		private static readonly Dictionary<string, Activity> activities = new Dictionary<string, Activity>();
		private static readonly ReaderWriterLock rwLock = new ReaderWriterLock();

		private static readonly Timer timer = new Timer(ActivitiesCleanup, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

		private static void ActivitiesCleanup(object state)
		{
			//this is here to prevent a compiler warning about the timer variable not used
			//it does absolutely nothing and has no meaning whatsoever
			timer.GetHashCode();

			rwLock.AcquireWriterLock(-1);
			try
			{
				foreach (KeyValuePair<string, DateTime> pair in new Dictionary<string, DateTime>(activitiesTimeStamps))
				{
					//It is not likely that a commit would last more than 24 hours
					if((DateTime.Now-pair.Value).TotalHours > 24)
						Delete(pair.Key);
				}
			}
			finally
			{
				rwLock.ReleaseWriterLock();
			}
		}

		public static void Create(string activityId)
		{
			rwLock.AcquireWriterLock(-1);
			try
			{
				activities[activityId] = new Activity();
				activitiesTimeStamps[activityId] = DateTime.Now;
			}
			finally
			{
				rwLock.ReleaseWriterLock();
			}
		}

		public static void Delete(string activityId)
		{
			rwLock.AcquireWriterLock(-1);
			try
			{
				activities.Remove(activityId);
				activitiesTimeStamps.Remove(activityId);
			}
			finally
			{
				rwLock.ReleaseWriterLock();
			}
		}

		public static void Use(string activityId, Action<Activity> action)
		{
			rwLock.AcquireReaderLock(-1);
			try
			{
				Activity activity = activities[activityId];
				lock(activity)
				{
					action(activity);
				}
			}
			finally 
			{
				rwLock.ReleaseReaderLock();
			}
		}

		public static bool Exists(string activityId)
		{
			rwLock.AcquireReaderLock(-1);
			try
			{
				return activities.ContainsKey(activityId);
			}
			finally
			{
				rwLock.ReleaseReaderLock();
			}
		}
	}
}