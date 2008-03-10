namespace SvnBridge.Infrastructure
{
    public interface IInterceptor
    {
        void Invoke(IInvocation invocation);
    }
}