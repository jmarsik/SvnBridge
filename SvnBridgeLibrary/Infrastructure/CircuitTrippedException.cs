using System;
using System.Runtime.Serialization;

namespace SvnBridge.Infrastructure
{
    [Serializable]
    public class CircuitTrippedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public CircuitTrippedException()
        {
        }

        public CircuitTrippedException(string message) : base(message)
        {
        }

        public CircuitTrippedException(string message,
                                       Exception inner) : base(message, inner)
        {
        }

        protected CircuitTrippedException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}