using System;
using System.Collections.Generic;
using System.Net;
using Attach;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace Tests
{
    public class MyMocks : MockFramework
    {
        public delegate bool ItemExists(string path, int version);
        public Results Attach(ItemExists method, bool returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public Results Attach(ItemExists method,
                              Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }

        public Results Attach(ItemExists method,
                              MultipleReturnValues returnValues)
        {
            return base.Attach((Delegate)method, returnValues);
        }

        public delegate int GetLatestVersion();

        public Results Attach(GetLatestVersion method,
                              int returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public delegate bool IsDirectory(int version,
                                         string path);

        public Results Attach(IsDirectory method,
                              bool returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public delegate void MakeActivity(string activityId);

        public Results Attach(MakeActivity method)
        {
            return base.Attach((Delegate)method);
        }

        public Results Attach(MakeActivity method,
                              Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }

        public delegate void MakeCollection(string activityId, string path);

        public Results Attach(MakeCollection method)
        {
            return base.Attach((Delegate)method);
        }

        public Results Attach(MakeCollection method, Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }

        public delegate MergeActivityResponse MergeActivity(string activityId);

        public Results Attach(MergeActivity method,
                              MergeActivityResponse returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public delegate ItemMetaData GetItems(int version,
                                              string path,
                                              Recursion recursion);

        public Results Attach(GetItems method,
                              ItemMetaData returnValue)
        {
            return base.Attach((Delegate)method, (ItemMetaData)returnValue);
        }

        public delegate byte[] ReadFile(ItemMetaData item);

        public Results Attach(ReadFile method,
                              byte[] returnValue)
        {
            return base.Attach((Delegate)method, (byte[])returnValue);
        }

        public Results Attach(ReadFile method,
                              MultipleReturnValues returnValues)
        {
            return base.Attach((Delegate)method, returnValues);
        }

        public delegate void SetCredentials(NetworkCredential credentials);

        public Results Attach(SetCredentials method)
        {
            return base.Attach((Delegate)method);
        }

        public delegate void WriteFileToActivity(string activityId,
                                                 string path,
                                                 byte[] fileData);

        public Results Attach(WriteFileToActivity method)
        {
            return base.Attach((Delegate)method);
        }

        public delegate LogItem GetLog(string path,
                                       int versionFrom,
                                       int versionTo,
                                       Recursion recursion,
                                       int maxCount);

        public Results Attach(GetLog method,
                              LogItem returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public delegate void SetProperty(string activityId,
                                         string path,
                                         string property,
                                         string value);

        public Results Attach(SetProperty method)
        {
            return base.Attach((Delegate)method);
        }

        public delegate FolderMetaData GetChangedItems(string path,
                                                       int versionFrom,
                                                       int versionTo,
                                                       UpdateReportData reportData);

        public Results Attach(GetChangedItems method,
                              FolderMetaData returnValue)
        {
            return base.Attach((Delegate)method, (object)returnValue);
        }

        public delegate int StreamRead(byte[] buffer, int offset, int count);

        public Results Attach(StreamRead method, Exception throwException)
        {
            return base.Attach((Delegate)method, throwException);
        }
    }
}