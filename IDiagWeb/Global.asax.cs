using IDiagLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace IDiagWeb
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var retVal = new CompositeDisposable();
            var initializer = new DiagnosticInitializer(new[] { new HttpDiagnosticListenerImplBase<HttpWebRequest, HttpWebResponse>() });
            retVal.Add(initializer);

            retVal.Add(DiagnosticListener
                .AllListeners
                .Subscribe(initializer));

        }
    }
}
