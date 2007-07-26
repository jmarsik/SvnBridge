using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SvnBridge.Net
{
    public class HttpResponseHeaderCollection : WebHeaderCollection
    {
        private readonly List<KeyValuePair<string, string>> headers;

        public HttpResponseHeaderCollection()
        {
            headers = new List<KeyValuePair<string, string>>();
        }

        public override string[] AllKeys
        {
            get
            {
                List<string> allKeys = new List<string>();

                foreach (KeyValuePair<string, string> header in headers)
                    if (!allKeys.Contains(header.Key))
                        allKeys.Add(header.Key);

                return allKeys.ToArray();
            }
        }

        public override int Count
        {
            get { return AllKeys.Length; }
        }

        public override void Add(string name, string value)
        {
            headers.Add(new KeyValuePair<string, string>(name, value));
        }

        public override void Clear()
        {
            headers.Clear();
        }

        public override string Get(int index)
        {
            return headers[index].Value;
        }

        public override string Get(string name)
        {
            StringBuilder buffer = new StringBuilder();

            foreach (KeyValuePair<string, string> header in headers)
                if (header.Key == name)
                    buffer.AppendFormat("{0},", header.Value);

            if (buffer.Length > 0)
            {
                buffer.Length--;
                return buffer.ToString();
            }
            else
                return null;
        }

        public override string GetKey(int index)
        {
            return headers[index].Key;
        }

        public override string[] GetValues(int index)
        {
            return Get(index).Split(',');
        }

        public override string[] GetValues(string header)
        {
            string value = Get(header);

            if (value != null)
                return value.Split(',');
            else
                return null;
        }

        public override void Remove(string name)
        {
            headers.RemoveAll(delegate(KeyValuePair<string, string> header) { return header.Key == name; });
        }

        public override void Set(string name, string value)
        {
            Remove(name);
            Add(name, value);
        }

        public string[] GetDistinctValues(string headerName)
        {
            List<string> values = new List<string>();

            foreach (KeyValuePair<string, string> header in headers)
                if (header.Key == headerName)
                    values.Add(header.Value);

            return values.Count > 0 ? values.ToArray() : null;
        }
    }
}