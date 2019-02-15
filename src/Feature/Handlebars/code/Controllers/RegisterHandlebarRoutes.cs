using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SF.Feature.Handlebars
{
    public class RegisterHandlebarRoutes 
    {
        public void Process(PipelineArgs args)
        {
            GlobalConfiguration.Configure(Configure);
        }
        protected void Configure(HttpConfiguration configuration)
        {
            MapRouteWithSession(configuration, "SF.Handlebars.AddItem", "sitecore/api/sf/additem", "HandlebarsAPI", "AddItem");
        }

        protected static void MapRouteWithSession(HttpConfiguration configuration, string routeName, string routePath, string controller, string action)
        {
            var routes = configuration.Routes;
            routes.MapHttpRoute(routeName, routePath, new
            {
                controller = controller,
                action = action
            });

            var route = System.Web.Routing.RouteTable.Routes[routeName] as System.Web.Routing.Route;
            route.RouteHandler = new SessionRouteHandler();
        }
    }
}
