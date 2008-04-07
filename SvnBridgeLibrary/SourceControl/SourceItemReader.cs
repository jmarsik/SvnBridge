using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public class SourceItemReader : IDisposable
    {
        private Stream _requestStream;
        private XmlReader _reader;
        private SourceItem _sourceItem;
        private string _tfsUrl;

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

        public SourceItem SourceItem
        {
            get { return _sourceItem; }
        }

        public bool Read()
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
            _sourceItem = item;
            return true;
        }
    }
}
