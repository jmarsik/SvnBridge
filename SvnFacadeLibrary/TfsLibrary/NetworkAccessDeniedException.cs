using System;

namespace SvnBridge.TfsLibrary
{
    public class NetworkAccessDeniedException : Exception
    {
        // Lifetime

        public NetworkAccessDeniedException()
            : this(null) {}

        public NetworkAccessDeniedException(Exception innerException)
            : base("Access to the network resource is denied.", innerException) {}
    }
}