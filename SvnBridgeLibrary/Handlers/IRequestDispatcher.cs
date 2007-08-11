namespace SvnBridge.Handlers
{
    public interface IRequestDispatcher
    {
        void Dispatch(IHttpRequest request);

        void RegisterHandler<THandler>()
            where THandler : IRequestHandler, new();
    }
}
