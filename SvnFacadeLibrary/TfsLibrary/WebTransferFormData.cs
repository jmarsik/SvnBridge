using System.Collections.Generic;
using System.IO;
using System.Text;
//using CodePlex.TfsLibrary.Utility;

namespace SvnBridge.TfsLibrary
{
    public class WebTransferFormData
    {
        // Fields

        List<IFormPart> formParts = new List<IFormPart>();
        string boundary = "--------------------------8e5m2D6l5Q4h6";

        // Properties

        public string Boundary
        {
            get { return boundary; }
        }

        // Methods

        public void Add(string name,
                        string value)
        {
            formParts.Add(new StringFormPart(name, value));
        }

        public void AddFile(string name,
                            byte[] fileData)
        {
            formParts.Add(new BinaryFormPart(name, fileData));
        }

        public void Render(Stream stream)
        {
            foreach (IFormPart formPart in formParts)
            {
                WriteString(stream, "--{0}\r\n", boundary);
                formPart.Render(stream);
                WriteString(stream, "\r\n");
            }

            WriteString(stream, "--{0}--\r\n", boundary);
        }

        internal static void WriteString(Stream stream,
                                         string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        internal static void WriteString(Stream stream,
                                         string format,
                                         params object[] args)
        {
            WriteString(stream, string.Format(format, args));
        }

        // Inner types

        interface IFormPart
        {
            void Render(Stream stream);
        }

        class StringFormPart : IFormPart
        {
            string name;
            string content;

            public StringFormPart(string name,
                                  string content)
            {
                this.name = name;
                this.content = content;
            }

            public void Render(Stream stream)
            {
                WriteString(stream, "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", name, content);
            }
        }

        class BinaryFormPart : IFormPart
        {
            string filename;
            byte[] content;

            public BinaryFormPart(string filename,
                                  byte[] content)
            {
                this.filename = filename;
                this.content = content;
            }

            public void Render(Stream stream)
            {
                WriteString(stream, "Content-Disposition: form-data; name=\"content\"; filename=\"{0}\"\r\nContent-Type: application/octet-stream\r\n\r\n", filename);
                stream.Write(content, 0, content.Length);
            }
        }
    }
}