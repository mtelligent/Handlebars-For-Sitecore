using SF.Feature.Handlebars.SXA.Models;
using Sitecore.XA.Foundation.Mvc.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SF.Feature.Handlebars.SXA.Repositories
{
    public class HandlebarContainerRepository : ModelRepository, IHandlebarContainerRepository
    {
        public override IRenderingModelBase GetModel()
        {
            var model = new HandlebarContainerModel();
            FillBaseProperties(model);
            return model;
        }
    }
}