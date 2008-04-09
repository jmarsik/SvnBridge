using System;
using System.Collections.Generic;
using System.Threading;
using SvnBridge.SourceControl.Dto;

namespace SvnBridge.SourceControl
{
	public static class ActivityRepository
	{
		private static readonly Dictionary<string, Activity> activities = new Dictionary<string, Activity>();
		private static readonly ReaderWriterLock rwLock = new ReaderWriterLock();

		public static void Create(string activityId)
		{
			rwLock.AcquireWriterLock(-1);
			try
			{
				activities[activityId] = new Activity();
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
	}
}