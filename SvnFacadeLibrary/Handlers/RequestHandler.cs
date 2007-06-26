using System.Net;
using CodePlex.TfsLibrary;
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

            try
            {
                // TODO: Make this a dispatch table
                switch (context.HttpMethod.ToLowerInvariant())
                {
                    case "propfind":
                        processor.ProcessPropFindRequest();
                        break;

                    case "report":
                        processor.ProcessReportRequest();
                        break;

                    case "options":
                        processor.ProcessOptionsRequest();
                        break;

                    case "mkactivity":
                        processor.ProcessMkActivityRequest();
                        break;

                    case "checkout":
                        processor.ProcessCheckoutRequest();
                        break;

                    case "proppatch":
                        processor.ProcessPropPatchRequest();
                        break;

                    case "put":
                        processor.ProcessPutRequest();
                        break;

                    case "merge":
                        processor.ProcessMergeRequest();
                        break;

                    case "delete":
                        processor.ProcessDeleteRequest();
                        break;

                    case "mkcol":
                        processor.ProcessMkColRequest();
                        break;

                    default:
                        context.StatusCode = 405;
                        context.ContentType = "text/html";
                        context.AddHeader("Allow", "PROPFIND, REPORT, OPTIONS, MKACTIVITY, CHECKOUT, PROPPATCH, PUT, MERGE, DELETE, MKCOL");
                        context.Write("<html><head><title>405 Method Not Allowed</title></head><body><h1>The requested method is not supported.</h1></body></html>");
                        break;
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;

                if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    processor.SendUnauthorizedResponse();
                else
                    throw;
            }
            catch (NetworkAccessDeniedException)
            {
                processor.SendUnauthorizedResponse();
            }
        }
    }
}