namespace SvnBridge.Handlers
{
    public delegate IRequestDispatcher CreateRequestDispatcher(string serverUrl);
    
    public static class RequestDispatcherFactory
    {
        private static CreateRequestDispatcher createDelegate;
        
        public static CreateRequestDispatcher CreateDelegate
        {
            set { createDelegate = value; }
        }
        
        public static IRequestDispatcher Create(string serverUrl)
        {
            IRequestDispatcher dispatcher;
            
            if (createDelegate == null)
                dispatcher = new RequestDispatcher(serverUrl);
            else
                dispatcher = createDelegate(serverUrl);

            dispatcher.RegisterHandler<CheckOutHandler>();
            dispatcher.RegisterHandler<CopyHandler>();
            dispatcher.RegisterHandler<DeleteHandler>();
            dispatcher.RegisterHandler<MergeHandler>();
            dispatcher.RegisterHandler<MkActivityHandler>();
            dispatcher.RegisterHandler<MkColHandler>();
            dispatcher.RegisterHandler<OptionsHandler>();
            dispatcher.RegisterHandler<PropFindHandler>();
            dispatcher.RegisterHandler<PropPatchHandler>();
            dispatcher.RegisterHandler<PutHandler>();
            dispatcher.RegisterHandler<ReportHandler>();

            return dispatcher;
        }
    }
}
