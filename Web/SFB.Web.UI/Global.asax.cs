﻿using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.ApplicationInsights.Extensibility;

namespace SFB.Web.UI 
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            IocConfig.Register();

            MvcHandler.DisableMvcResponseHeader = true;

            var enableAITelemetry = ConfigurationManager.AppSettings["EnableAITelemetry"];
            TelemetryConfiguration.Active.DisableTelemetry = enableAITelemetry == null || !bool.Parse(enableAITelemetry);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.AddHeader("x-frame-options", "SAMEORIGIN");

            if (!Request.Path.Contains("/BenchmarkCharts/"))
            {
                Response.Cache.SetCacheability(HttpCacheability.Server); // HTTP 1.1.
                Response.Cache.AppendCacheExtension("no-store, must-revalidate");
                Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
                Response.AppendHeader("Expires", "0"); // Proxies.
            }
        }

        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
        }
    }
}
