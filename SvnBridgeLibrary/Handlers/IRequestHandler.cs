namespace SvnBridge.Handlers
{
    public interface IRequestHandler
    {
        string Method { get; }

        void Handle(IHttpRequest request, string tfsServer);
    }
}
