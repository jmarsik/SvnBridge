using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;
using SvnBridge.RegistrationWebSvc;
using SvnBridge.RepositoryWebSvc;
using SvnBridge.Utility;
using System.Web.Services.Protocols;

namespace SvnBridge.TfsLibrary
{
    public class TfsLibraryService
    {
        static string _repositoryUrl;
        static string _downloadUrlPrefix;
        static string _uploadUrl;

        ICredentials SetupCredentials(string url, NetworkCredential credentials)
        {
            ICredentials newCredentials = credentials;
            if (newCredentials == null)
            {
                if (url.ToLower().Contains("codeplex.com"))
                {
                    CredentialCache cache = new CredentialCache();
                    cache.Add(new Uri(url), "Basic", new NetworkCredential("anonymous", null));
                    newCredentials = cache;
                }
                else
                {
                    newCredentials = CredentialCache.DefaultNetworkCredentials;
                }
            }
            return newCredentials;
        }

        string GetServiceInterfaceUrl(string tfsUrl,
                                      NetworkCredential credentials,
                                      string entryType,
                                      string interfaceName)
        {
            Registration registration = new Registration();
            registration.Url = tfsUrl + "/Services/v1.0/Registration.asmx";
            registration.Credentials = SetupCredentials(registration.Url, credentials);
            registration.Timeout = 15 * 60 * 1000;
            registration.UserAgent = "CodePlexClient";
            
            RegistrationEntry[] entries;
            try
            {
                entries = registration.GetRegistrationEntries(null);
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;

                if (response == null || response.StatusCode != HttpStatusCode.Unauthorized)
                    throw;

                throw new NetworkAccessDeniedException(ex);
            }

            string url = null;
            foreach (RegistrationEntry entry in entries)
            {
                if (entry.Type == entryType)
                {
                    foreach (ServiceInterface iface in entry.ServiceInterfaces)
                    {
                        if (iface.Name == interfaceName)
                        {
                            url = tfsUrl + "/" + iface.Url;
                        }
                    }
                }
            }
            return url;
        }

        string GetRepositoryUrl(string tfsUrl,
                                NetworkCredential credentials)
        {
            if (_repositoryUrl == null)
            {
                _repositoryUrl = GetServiceInterfaceUrl(tfsUrl, credentials, "VersionControl", "ISCCProvider");
            }
            return _repositoryUrl;
        }

        string GetDownloadUrlPrefix(string tfsUrl,
                                    NetworkCredential credentials)
        {
            if (_downloadUrlPrefix == null)
            {
                _downloadUrlPrefix = GetServiceInterfaceUrl(tfsUrl, credentials, "VersionControl", "Download");
            }
            return _downloadUrlPrefix;
        }

        string GetUploadUrl(string tfsUrl,
                            NetworkCredential credentials)
        {
            if (_uploadUrl == null)
            {
                _uploadUrl = GetServiceInterfaceUrl(tfsUrl, credentials, "VersionControl", "Upload");
            }
            return _uploadUrl;
        }

        Repository GetRepository(string tfsUrl, NetworkCredential credentials)
        {
            Repository repository = new Repository();
            repository.Url = GetRepositoryUrl(tfsUrl, credentials);
            repository.Credentials = SetupCredentials(repository.Url, credentials);
            repository.Timeout = 15 * 60 * 1000;
            repository.UserAgent = "CodePlexClient";

            repository.UnsafeAuthenticatedConnectionSharing = true;
            repository.ConnectionGroupName = GetUsername(credentials);

            return repository;
        }

        string GetUsername(NetworkCredential credentials)
        {
            string username = null;
            if (credentials == null)
            {
                username = WindowsIdentity.GetCurrent().Name;
            }
            else if (string.IsNullOrEmpty(credentials.Domain))
            {
                username = credentials.UserName;
            }
            else
            {
                username = credentials.Domain + "\\" + credentials.UserName;
            }
            return username;
        }

        public void CreateWorkspace(string tfsUrl,
                                    NetworkCredential credentials,
                                    string workspaceName,
                                    string workspaceComment)
        {
            Workspace workspace = new Workspace();
            workspace.name = workspaceName;
            workspace.Comment = workspaceComment;
            workspace.computer = SystemInformation.ComputerName;
            workspace.owner = GetUsername(credentials);

            Repository repository = GetRepository(tfsUrl, credentials);
            try
            {
                repository.CreateWorkspace(workspace);
            }
            catch (SoapException ex)
            {
                if (ex.Message.StartsWith("TF14044:"))
                {
                    throw new NetworkAccessDeniedException(ex);
                }
                throw;
            }
        }

        public void AddWorkspaceMapping(string tfsUrl,
                                        NetworkCredential credentials,
                                        string workspaceName,
                                        string serverPath,
                                        string localPath)
        {
            Repository repository = GetRepository(tfsUrl, credentials);
            string username = GetUsername(credentials);
            Workspace workspace = repository.QueryWorkspace(workspaceName, username);

            WorkingFolder folder = new WorkingFolder();
            folder.item = serverPath;
            folder.local = Path.GetFullPath(localPath);
            folder.type = WorkingFolderType.Map;

            if (workspace.Folders == null)
            {
                workspace.Folders = new WorkingFolder[1];
            }
            else
            {
                WorkingFolder[] oldFolders = workspace.Folders;
                workspace.Folders = new WorkingFolder[workspace.Folders.Length + 1];
                oldFolders.CopyTo(workspace.Folders, 0);
            }
            workspace.Folders[workspace.Folders.Length - 1] = folder;

            repository.UpdateWorkspace(workspaceName, username, workspace);
        }

        public void UpdateLocalVersions(string tfsUrl,
                                        NetworkCredential credentials,
                                        string workspaceName,
                                        List<LocalVersionUpdate> updates)
        {
            Repository repository = GetRepository(tfsUrl, credentials);
            repository.UpdateLocalVersion(workspaceName, GetUsername(credentials), updates.ToArray());
        }

        public void PendChanges(string tfsUrl,
                                NetworkCredential credentials,
                                string workspaceName,
                                IEnumerable<PendRequest> requests)
        {
            Repository repository = GetRepository(tfsUrl, credentials);

            ChangeRequest[] adds;
            ChangeRequest[] edits;
            ChangeRequest[] deletes;
            string username = GetUsername(credentials);

            PendRequestsToChangeRequests(requests, out adds, out edits, out deletes);

            PendChangesHelper(repository, workspaceName, username, adds);
            PendChangesHelper(repository, workspaceName, username, edits);
            PendChangesHelper(repository, workspaceName, username, deletes);
        }

        void PendRequestsToChangeRequests(IEnumerable<PendRequest> requests,
                                          out ChangeRequest[] addRequests,
                                          out ChangeRequest[] editRequests,
                                          out ChangeRequest[] deleteRequests)
        {
            List<ChangeRequest> adds = new List<ChangeRequest>();
            List<ChangeRequest> edits = new List<ChangeRequest>();
            List<ChangeRequest> deletes = new List<ChangeRequest>();

            foreach (PendRequest request in requests)
                if (request.RequestType == PendRequestType.Add)
                    adds.Add(PendRequestToChangeRequest(request));
                else if (request.RequestType == PendRequestType.Edit)
                    edits.Add(PendRequestToChangeRequest(request));
                else if (request.RequestType == PendRequestType.Delete)
                    deletes.Add(PendRequestToChangeRequest(request));

            addRequests = adds.ToArray();
            editRequests = edits.ToArray();
            deleteRequests = deletes.ToArray();
        }

        ChangeRequest PendRequestToChangeRequest(PendRequest pendRequest)
        {
            ChangeRequest result = new ChangeRequest();
            result.item = new ItemSpec();

            switch (pendRequest.RequestType)
            {
                case PendRequestType.Add:
                    result.type = (ItemType)Enum.Parse(typeof(ItemType), pendRequest.ItemType.ToString());
                    result.req = RequestType.Add;
                    result.item.item = pendRequest.LocalName;
                    result.enc = pendRequest.CodePage;
                    break;

                case PendRequestType.Edit:
                    result.req = RequestType.Edit;
                    result.item.item = pendRequest.LocalName;
                    result.@lock = LockLevel.None;
                    break;

                case PendRequestType.Delete:
                    result.req = RequestType.Delete;
                    result.item.item = pendRequest.LocalName;
                    break;

                default:
                    throw new ArgumentException("Unexpected request type " + pendRequest.RequestType.ToString(), "pendRequest");
            }

            return result;
        }

        List<Failure> FilterFailures(IEnumerable<Failure> failures)
        {
            List<Failure> realFailures = new List<Failure>();

            foreach (Failure failure in failures)
                if (failure.Warnings == null || failure.Warnings.Length == 0)
                    realFailures.Add(failure);

            return realFailures;
        }

        void PendChangesHelper(Repository webSvc,
                               string workspaceName,
                               string username,
                               ChangeRequest[] changes)
        {
            if (changes.Length == 0)
                return;

            Failure[] failures;

            webSvc.PendChanges(workspaceName, username, changes, out failures);

            List<Failure> realFailures = FilterFailures(failures);
            if (realFailures.Count > 0)
                throw new Exception("Failed pending changes");
        }

        public void UploadFile(string tfsUrl,
                               NetworkCredential credentials,
                               string workspaceName,
                               byte[] fileData,
                               string serverPath)
        {
            string uploadUrl = GetUploadUrl(tfsUrl, credentials);
            string username = GetUsername(credentials);
            long fileSize = fileData.Length;

            WebTransferFormData formData = new WebTransferFormData();
            formData.Add("item", serverPath);
            formData.Add("wsname", workspaceName);
            formData.Add("wsowner", username);
            formData.Add("filelength", fileSize.ToString());
            formData.Add("hash", Helper.GetMd5Checksum(fileData));
            formData.Add("range", string.Format("bytes=0-{0}/{1}", fileSize - 1, fileSize));
            formData.AddFile("item", fileData);

            PostForm(uploadUrl, credentials, formData);
        }

        void PostForm(string url,
                      NetworkCredential credentials,
                      WebTransferFormData formData)
        {
            HttpWebRequest request = (HttpWebRequest)SetupWebRequest(WebRequest.Create(url), credentials);
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + formData.Boundary;

            using (Stream stream = request.GetRequestStream())
                formData.Render(stream);

            request.GetResponse().Close();
        }

        static string groupName = Guid.NewGuid().ToString();

        HttpWebRequest SetupWebRequest(WebRequest request, NetworkCredential credentials)
        {
            HttpWebRequest result = (HttpWebRequest)request;
            result.ServicePoint.ConnectionLimit = 15;
            result.ServicePoint.UseNagleAlgorithm = false;
            result.SendChunked = false;
            result.Pipelined = false;
            result.KeepAlive = true;
            result.PreAuthenticate = false;
            result.UnsafeAuthenticatedConnectionSharing = true;
            result.ConnectionGroupName = groupName;
            result.UserAgent = "CodePlexClient";
            result.Credentials = SetupCredentials(result.RequestUri.ToString(), credentials);
            return result;
        }

        Stream GetResponseStream(WebResponse response)
        {
            if (string.Compare(response.ContentType, "application/gzip", true) != 0)
                return response.GetResponseStream();

            Stream stream = null;

            try
            {
                stream = response.GetResponseStream();
                return new GZipStream(stream, CompressionMode.Decompress);
            }
            catch
            {
                if (stream != null)
                    stream.Dispose();

                throw;
            }
        }

        byte[] Download(string url,
                        NetworkCredential credentials)
        {
            WebRequest request = SetupWebRequest(WebRequest.Create(url), credentials);

            byte[] data = new byte[0];
            byte[] buffer = new byte[250000];
            using (WebResponse response = request.GetResponse())
            using (Stream stream = GetResponseStream(response))
            {
                int count;
                do
                {
                    count = stream.Read(buffer, 0, buffer.Length);

                    byte[] tempData = new byte[data.Length + count];
                    Array.Copy(data, 0, tempData, 0, data.Length);
                    Array.Copy(buffer, 0, tempData, data.Length, count);
                    data = tempData;
                }
                while (count > 0);
                response.Close();
            }
            return data;
        }

        public int Commit(string tfsUrl,
                          NetworkCredential credentials,
                          string workspaceName,
                          string comment,
                          IEnumerable<string> serverItems)
        {
            Repository repository = GetRepository(tfsUrl, credentials);

            List<string> items = new List<string>(serverItems);
            Failure[] failures;
            string username = GetUsername(credentials);
            CheckinNotificationInfo notifyInfo = new CheckinNotificationInfo();
            Changeset info = new Changeset();
            info.Comment = comment;
            info.owner = username;

            CheckinResult result = repository.CheckIn(workspaceName, username, items.ToArray(), info, notifyInfo, CheckinOptions.ValidateCheckinOwner, out failures);

            List<Failure> realFailures = FilterFailures(failures);
            if (realFailures.Count > 0)
                throw new Exception("Failure during commit");

            return result.cset;
        }

        public void DeleteWorkspace(string tfsUrl,
                                    NetworkCredential credentials,
                                    string workspaceName)
        {
            Repository repository = GetRepository(tfsUrl, credentials);
            repository.DeleteWorkspace(workspaceName, GetUsername(credentials));
        }

        public Changeset[] QueryLog(string tfsUrl,
                                    NetworkCredential credentials,
                                    string serverPath,
                                    VersionSpec versionFrom,
                                    VersionSpec versionTo,
                                    RecursionType recursive,
                                    int maxCount)
        {
            Repository repository = GetRepository(tfsUrl, credentials);

            ItemSpec itemSpec = new ItemSpec();
            itemSpec.item = serverPath;
            itemSpec.recurse = recursive;
            Changeset[] changes = repository.QueryHistory(null, null, itemSpec, new LatestVersionSpec(), null, versionFrom, versionTo, maxCount, true, false, false);
            return changes;
        }

        public int GetLatestChangeset(string tfsUrl,
                                      NetworkCredential credentials)
        {
            Repository repository = GetRepository(tfsUrl, credentials);
            RepositoryProperties properties = repository.GetRepositoryProperties();
            return properties.lcset;
        }

        public byte[] DownloadFile(string downloadUrl,
                                   NetworkCredential credentials)
        {
            return Download(downloadUrl, credentials);
        }

        public SourceItem[] QueryItems(string tfsUrl,
                                       NetworkCredential credentials,
                                       string serverPath,
                                       RecursionType recursion,
                                       VersionSpec version,
                                       DeletedState deletedState,
                                       ItemType itemType)
        {
            Repository repository = GetRepository(tfsUrl, credentials);
            string downloadUrlPrefix = GetDownloadUrlPrefix(tfsUrl, credentials);

            ItemSpec spec = new ItemSpec();
            spec.item = serverPath;
            spec.recurse = recursion;

            ItemSet[] itemSets;
            try
            {
                itemSets = repository.QueryItems(null, null, new ItemSpec[1] { spec }, version, deletedState, itemType, true);
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;

                if (response == null || response.StatusCode != HttpStatusCode.Unauthorized)
                    throw;

                throw new NetworkAccessDeniedException(ex);
            }

            List<SourceItem> result = new List<SourceItem>();

            foreach (Item item in itemSets[0].Items)
            {
                ItemType remoteItemType = item.type;
                result.Add(SourceItem.FromRemoteItem(item.itemid, remoteItemType, null, SourceItemStatus.Unmodified,
                                                     item.item, item.cs, item.len, item.date,
                                                     downloadUrlPrefix + "?" + item.durl));
            }

            result.Sort();
            return result.ToArray();
        }
    }
}