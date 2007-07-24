using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;

namespace SvnBridge.Utility
{
    public static class Helper
    {
        public static XmlReaderSettings InitializeNewXmlReaderSettings()
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.CloseInput = false;
            return readerSettings;
        }

        public static T DeserializeXml<T>(XmlReader reader)
        {
            XmlSerializer requestSerializer = new XmlSerializer(typeof(T));
            return (T)requestSerializer.Deserialize(reader);
        }

        public static T DeserializeXml<T>(string xml)
        {
            XmlReader reader = XmlReader.Create(new StringReader(xml), InitializeNewXmlReaderSettings());
            return (T)DeserializeXml<T>(reader);
        }

        public static T DeserializeXml<T>(byte[] xml)
        {
            XmlReader reader = XmlReader.Create(new MemoryStream(xml), InitializeNewXmlReaderSettings());
            return (T)DeserializeXml<T>(reader);
        }

        public static T DeserializeXml<T>(Stream requestStream)
        {
            using (XmlReader reader = XmlReader.Create(requestStream, InitializeNewXmlReaderSettings()))
            {
                return DeserializeXml<T>(reader);
            }
        }

        public static bool IsValidPort(string port)
        {
            int portAsInt;

            if (!int.TryParse(port, out portAsInt))
                return false;

            return IsValidPort(portAsInt);
        }

        public static bool IsValidPort(int port)
        {
            return (port >= 1 && port <= 65535);
        }

        public static bool IsPortInUse(int port)
        {
            bool inUse = false;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            try
            {
                listener.Start();
            }
            catch (SocketException)
            {
                inUse = true;
            }
            listener.Stop();
            return inUse;
        }

        public static bool IsValidTFSUrl(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url + "/Services/v1.0/Registration.asmx");
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                request.Timeout = 20000;

                using (WebResponse response = request.GetResponse())
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    string output = reader.ReadToEnd();
                    return (output.Contains("Team Foundation Registration web service"));
                }
            }
            catch (WebException e)
            {
                HttpWebResponse response = e.Response as HttpWebResponse;

                if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidUrl(string url)
        {
            try
            {
                new Uri(url);
                return true;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        public static byte[] SerializeXml<T>(T request)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = false;
            settings.Encoding = Encoding.UTF8;
            MemoryStream xml = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(xml, settings);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            serializer.Serialize(writer, request, ns);
            writer.Flush();
            return xml.ToArray();
        }

        public static void ReDim(ref byte[] arr,
                                 int length)
        {
            byte[] arrTemp = new byte[length];
            if (length > arr.Length)
            {
                Array.Copy(arr, 0, arrTemp, 0, arr.Length);
                arr = arrTemp;
            }
            else
            {
                Array.Copy(arr, 0, arrTemp, 0, length);
                arr = arrTemp;
            }
        }

        public static string GetMd5Checksum(byte[] data)
        {
            MD5 md5 = MD5.Create();
            StringBuilder sb = new StringBuilder();

            foreach (byte b in md5.ComputeHash(data))
                sb.Append(b.ToString("x2").ToLower());

            return sb.ToString();
        }

        static readonly string[] DECODED = new string[] { "%", " ", "&" };
        static readonly string[] ENCODED = new string[] { "%25", "%20", "&amp;" };

        public static string Encode(string value)
        {
            for (int i = 0; i < DECODED.Length; i++)
                value = value.Replace(DECODED[i], ENCODED[i]);

            return value;
        }

        public static string Decode(string value)
        {
            for (int i = ENCODED.Length - 1; i >= 0; i--)
                value = value.Replace(ENCODED[i], DECODED[i]);

            return value;
        }

        static readonly string[] DECODED_B = new string[] { "&", "<", ">" };
        static readonly string[] ENCODED_B = new string[] { "&amp;", "&lt;", "&gt;" };

        public static string EncodeB(string value)
        {
            if (value != null)
                for (int i = 0; i < DECODED_B.Length; i++)
                    value = value.Replace(DECODED_B[i], ENCODED_B[i]);

            return value;
        }

        public static string DecodeB(string value)
        {
            for (int i = ENCODED_B.Length - 1; i >= 0; i--)
                value = value.Replace(ENCODED_B[i], DECODED_B[i]);

            return value;
        }
    }
}