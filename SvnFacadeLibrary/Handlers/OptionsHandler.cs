using System.Text;
using SvnBridge.Net;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public class OptionsHandler : HttpContextHandlerBase
    {
        public override string MethodToHandle
        {
            get { return "options"; }
        }

        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            WebDavService webDavService = new WebDavService(sourceControlProvider);

            SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
            response.Headers.Add("DAV", "1,2");
            response.Headers.Add("DAV", "version-control,checkout,working-resource");
            response.Headers.Add("DAV", "merge,baseline,activity,version-controlled-collection");
            response.Headers.Add("MS-Author-Via", "DAV");
            response.Headers.Add("Allow", "OPTIONS,GET,HEAD,POST,DELETE,TRACE,PROPFIND,PROPPATCH,COPY,MOVE,LOCK,UNLOCK,CHECKOUT");
            webDavService.Options(path, response.OutputStream);
        }
    }
}