using System;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public class RequestHandler
    {
        string _server;

        public RequestHandler(string server)
        {
            _server = server;
        }

        protected virtual ISourceControlProvider GetSourceControlProvider(IHttpRequest context)
        {
            return new TFSSourceControlProvider(_server, context.Credentials);
        }

        public void ProcessRequest(IHttpRequest context)
        {
            ISourceControlProvider sourceControlProvider = GetSourceControlProvider(context);
            WebDavService webDavService = new WebDavService(sourceControlProvider);
            CommandProcessor processor = new CommandProcessor(context, webDavService);
            if (String.Compare(context.HttpMethod, "propfind", true) == 0)
            {
                processor.ProcessPropFindRequest();
            }
            else if (String.Compare(context.HttpMethod, "report", true) == 0)
            {
                processor.ProcessReportRequest();
            }
            else if (String.Compare(context.HttpMethod, "options", true) == 0)
            {
                processor.ProcessOptionsRequest();
            }
            else if (String.Compare(context.HttpMethod, "mkactivity", true) == 0)
            {
                processor.ProcessMkActivityRequest();
            }
            else if (String.Compare(context.HttpMethod, "checkout", true) == 0)
            {
                processor.ProcessCheckoutRequest();
            }
            else if (String.Compare(context.HttpMethod, "proppatch", true) == 0)
            {
                processor.ProcessPropPatchRequest();
            }
            else if (String.Compare(context.HttpMethod, "put", true) == 0)
            {
                processor.ProcessPutRequest();
            }
            else if (String.Compare(context.HttpMethod, "merge", true) == 0)
            {
                processor.ProcessMergeRequest();
            }
            else if (String.Compare(context.HttpMethod, "delete", true) == 0)
            {
                processor.ProcessDeleteRequest();
            }
            else if (String.Compare(context.HttpMethod, "mkcol", true) == 0)
            {
                processor.ProcessMkColRequest();
            }
            else
            {
                throw new Exception("Unknown HTTP method '" + context.HttpMethod + "'.");
            }
        }
    }
}