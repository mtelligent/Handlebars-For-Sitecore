using Sitecore.XA.Foundation.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SF.Feature.Handlebars.SXA.Models
{
    public class AddNewItemModel : RenderingModelBase
    {
        public string ButtonText { get; set; }
        public string Template { get; set; }
        public string Parent { get; set; }

    }
}