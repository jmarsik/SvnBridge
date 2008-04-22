using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class SourceItemReader : IEnumerable<SourceItem>, IEnumerator<SourceItem>
    {
        private readonly Stream _requestStream;
        private readonly XmlReader _reader;
        private SourceItem current;
        private readonly string _tfsUrl;

        public SourceItemReader(string tfsUrl, Stream requestStream)
        {
            _tfsUrl = tfsUrl;
            _requestStream = requestStream;
            _reader = XmlReader.Create(_requestStream);
        }

        public void Dispose()
        {
            _requestStream.Close();
            _reader.Close();
            ((IDisposable)_requestStream).Dispose();
            ((IDisposable)_reader).Dispose();
        }


        #region IEnumerator<SourceItem> Members

        public SourceItem Current
        {
            get { return current; }
        }

        #endregion

        #region IEnumerator Members

        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator.Current
        {
            get { return current; }
        }

        #endregion

        public bool MoveNext()
        {
            if (!_reader.ReadToFollowing("Item"))
                return false;

            SourceItem item = new SourceItem();
            item.DownloadUrl = "";
            for (int i = 0; i < _reader.AttributeCount; i++)
            {
                _reader.MoveToAttribute(i);
                switch (_reader.Name)
                {
                    case "cs":
                        item.RemoteChangesetId = int.Parse(_reader.Value);
                        break;
                    case "date":
                        item.RemoteDate = DateTime.Parse(_reader.Value).ToUniversalTime();
                        break;
                    case "type":
                        switch (_reader.Value)
                        {
                            case "Folder":
                                item.ItemType = ItemType.Folder;
                                break;
                            case "File":
                                item.ItemType = ItemType.File;
                                break;
                        }
                        break;
                    case "itemid":
                        item.ItemId = int.Parse(_reader.Value);
                        break;
                    case "item":
                        item.RemoteName = _reader.Value;
                        break;
                    case "durl":
                        item.DownloadUrl = _tfsUrl + "/VersionControl/v1.0/item.asmx?" + _reader.Value;
                        break;
                }
            }
            current = item;
            return true;
        }

        #region IEnumerable<SourceItem> Members

        IEnumerator<SourceItem> IEnumerable<SourceItem>.GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<SourceItem>) this).GetEnumerator();
        }

        #endregion
    }
}
