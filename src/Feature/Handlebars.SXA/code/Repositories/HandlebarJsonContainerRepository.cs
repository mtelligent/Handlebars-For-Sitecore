using SF.Feature.Handlebars.SXA.Models;
using Sitecore.XA.Foundation.Mvc.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SF.Foundation.Handlebars;

namespace SF.Feature.Handlebars.SXA.Repositories
{
    public class HandlebarJsonContainerRepository : ModelRepository, IHandlebarJsonContainerRepository
    {
        public override IRenderingModelBase GetModel()
        {
            var model = new HandlebarJsonContainerModel();
            FillBaseProperties(model);
            
            var JsonUrlField = (Sitecore.Data.Fields.LinkField)model.Item.Fields["Url"];
            model.JsonUrl = JsonUrlField.LinkUrl();

            return model;
        }
    }
}