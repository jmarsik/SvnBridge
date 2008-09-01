using System;
using System.Net;
using System.Threading;
using Attach;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Cache;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace Tests
{
    public class MockFramework : AttachFramework
    {
        public Results Attach(Delegate method, Exception exception)
        {
            return base.Attach(method, Return.Exception(exception));
        }

        public Results Attach(Delegate method, object returnValue)
        {
            if (returnValue is Return)
            {
                return base.Attach(method, (Return)returnValue);
            }
            else
            {
                return base.Attach(method, Return.Value(returnValue));
            }
        }

        public Results Attach(Delegate method)
        {
            return base.Attach(method, Return.Nothing);
        }
    }

    public class MyMocks : MockFramework
    {
        public delegate void Associate(int workItemId, int changeSetId);
        public delegate void SetWorkItemFixed(int workItemId);
        public delegate void CopyItem(string activityId, string path, string targetPath);
        public delegate bool DeleteItem(string activityId, string path);
        public delegate FolderMetaData GetChangedItems(string path, int versionFrom, int versionTo, UpdateReportData reportData);
        public delegate ItemMetaData GetItemInActivity(string activityId, string path);
        public delegate ItemMetaData GetItems(int version, string path, Recursion recursion);
        public delegate int GetLatestVersion();
        public delegate LogItem GetLog(string path, int versionFrom, int versionTo, Recursion recursion, int maxCount);
        public delegate bool IsDirectory(int version, string path);
        public delegate bool ItemExists(string path, int version);
        public delegate void MakeActivity(string activityId);
        public delegate void MakeCollection(string activityId, string path);
        public delegate MergeActivityResponse MergeActivity(string activityId);
        public delegate byte[] ReadFile(ItemMetaData item);
        public delegate void ReadFileAsync(ItemMetaData item);
        public delegate void SetCredentials(NetworkCredential credentials);
        public delegate void SetProperty(string activityId, string path, string property, string value);
        public delegate int StreamRead(byte[] buffer, int offset, int count);
        public delegate bool WriteFile(string activityId, string path, byte[] fileData);

        public Results Attach(DeleteItem method, bool returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(ItemExists method, bool returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(ItemExists method, Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }

        public Results Attach(ItemExists method, Return action)
        {
            return base.Attach(method, action);
        }

        public Results Attach(GetLatestVersion method, int returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(IsDirectory method, bool returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(IsDirectory method, Return action)
        {
            return base.Attach(method, action);
        }

        public Results Attach(MakeActivity method)
        {
            return base.Attach((Delegate)method);
        }

        public Results Attach(MakeActivity method, Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }

        public Results Attach(MakeCollection method)
        {
            return base.Attach((Delegate)method);
        }

        public Results Attach(MakeCollection method, Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }

        public Results Attach(MergeActivity method, MergeActivityResponse returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(MergeActivity method, Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }

        public Results Attach(GetItemInActivity method, ItemMetaData returnValue)
        {
            return base.Attach((Delegate)method, (ItemMetaData)returnValue);
        }

        public Results Attach(GetItems method, ItemMetaData returnValue)
        {
            return base.Attach((Delegate)method, (ItemMetaData)returnValue);
        }


        public Results AttachReadFile(ReadFile method, byte[] returnValue)
        {
            return base.Attach((Delegate)method, (byte[])returnValue);
        }


        public Results Attach(ReadFileAsync method, FileData returnValue)
        {
            return base.Attach((Delegate)method, Return.DelegateResult(delegate(object[] parameters)
            {
                FutureFile file = new FutureFile(delegate
                {
                    return returnValue;
                });
                ((ItemMetaData)parameters[0]).Data = file;
                ((ItemMetaData) parameters[0]).DataLoaded = true;
                return null;
            }));
        }

        public Results Attach(SetCredentials method)
        {
            return base.Attach((Delegate)method);
        }

        public Results Attach(WriteFile method, bool returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(GetLog method, LogItem returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(SetProperty method)
        {
            return base.Attach((Delegate)method);
        }

        public Results Attach(GetChangedItems method, FolderMetaData returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(StreamRead method, Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }

        public Results Attach(CopyItem method)
        {
            return base.Attach((Delegate)method);
        }

        public Results Attach(Associate method)
        {
            return base.Attach((Delegate)method);
        }

        public Results Attach(SetWorkItemFixed method)
        {
            return base.Attach((Delegate)method);
        }
    }
}
