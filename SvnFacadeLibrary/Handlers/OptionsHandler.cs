using System.Text;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public class OptionsHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "options"; }
        }

        protected override void Handle(IHttpRequest request, ISourceControlProvider sourceControlProvider)
        {
            WebDavService webDavService = new WebDavService(sourceControlProvider);
            
            SetResponseSettings(request, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
            request.AddHeader("DAV", "1,2");
            request.AddHeader("DAV", "version-control,checkout,working-resource");
            request.AddHeader("DAV", "merge,baseline,activity,version-controlled-collection");
            request.AddHeader("MS-Author-Via", "DAV");
            request.AddHeader("Allow", "OPTIONS,GET,HEAD,POST,DELETE,TRACE,PROPFIND,PROPPATCH,COPY,MOVE,LOCK,UNLOCK,CHECKOUT");
            webDavService.Options(request.Path, request.OutputStream);
        }
    }
}
