using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    public class TxDeltaData
    {
        public TxDeltaData() {}

        [XmlText(DataType = "string")]
        public string Data = null;
    }
}