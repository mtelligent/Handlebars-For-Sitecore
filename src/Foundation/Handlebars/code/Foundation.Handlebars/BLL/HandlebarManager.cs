using HandlebarsDotNet;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;


namespace SF.Foundation.Handlebars
{
    public static class HandlebarManager
    {
        static HandlebarManager()
        {
            //Run Pipeleine to get Helpers and Register them
            var args = new HandlebarHelpersPipelineArgs();
            Sitecore.Pipelines.CorePipeline.Run("handlebarHelpers", args);
            foreach (var helper in args.Helpers)
            {
                if (helper.Helper != null && !HandlebarsDotNet.Handlebars.Configuration.Helpers.ContainsKey(helper.Name) && !HandlebarsDotNet.Handlebars.Configuration.BlockHelpers.ContainsKey(helper.Name))
                {
                    HandlebarsDotNet.Handlebars.RegisterHelper(helper.Name, helper.Helper);
                }
                else
                {
                    if (helper.BlockHelper != null && !HandlebarsDotNet.Handlebars.Configuration.Helpers.ContainsKey(helper.Name) && !HandlebarsDotNet.Handlebars.Configuration.BlockHelpers.ContainsKey(helper.Name))
                    {
                        HandlebarsDotNet.Handlebars.RegisterHelper(helper.Name, helper.BlockHelper);
                    }
                }
            }
        }

        public static void SetupJsonContainer(string url)
        {
            var wc = new System.Net.WebClient();
            var jsonText = wc.DownloadString(url);
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonText);
            if (HttpContext.Current.Items["HandlebarDataSource"] == null)
            {
                HttpContext.Current.Items.Add("HandlebarDataSource", obj);
            }
            else
            {
                HttpContext.Current.Items["HandlebarDataSource"] = obj;
            }
        }

        public static void SetupFacetContainer()
        {
            //Get Facet Type from 
            XmlNodeList nodes = Factory.GetConfigNodes(@"model/entities/contact/facets/*");
            MethodInfo method = typeof(Sitecore.Analytics.Tracking.Contact).GetMethod("GetFacet");

            if (Sitecore.Analytics.Tracker.Current == null)
            {
                Sitecore.Analytics.Tracker.StartTracking();
            }

            var myDynamicItem = new ExpandoObject() as IDictionary<string, Object>;
            myDynamicItem.Add("IsPageEditorEditing", Sitecore.Context.PageMode.IsExperienceEditorEditing ? true : false);
            myDynamicItem.Add("IsExperienceEditorEditing", Sitecore.Context.PageMode.IsExperienceEditorEditing ? true : false);

            foreach (XmlNode node in nodes)
            {
                var facetName = Sitecore.Xml.XmlUtil.GetAttribute("name", node);
                var facetTypeName = Sitecore.Xml.XmlUtil.GetAttribute("contract", node);

                Type facetType = Type.GetType(facetTypeName);
                MethodInfo generic = method.MakeGenericMethod(facetType);

                object obj = generic.Invoke(Sitecore.Analytics.Tracker.Current.Contact, new object[] { facetName });
                if (obj != null)
                {
                    myDynamicItem.Add(facetName, obj);
                }

            }

            if (HttpContext.Current.Items["HandlebarDataSource"] == null)
            {
                HttpContext.Current.Items.Add("HandlebarDataSource", myDynamicItem);
            }
            else
            {
                HttpContext.Current.Items["HandlebarDataSource"] = myDynamicItem;
            }
        }

        public static void SetupContainer(Item item)
        {

            var itemAsObj = getItemAsObject(item);


            if (HttpContext.Current.Items["HandlebarDataSource"] == null)
            {
                HttpContext.Current.Items.Add("HandlebarDataSource", itemAsObj);
            }
            else
            {
                HttpContext.Current.Items["HandlebarDataSource"] = itemAsObj;
            }
        }

        public static void SetupContainer(List<Item> items)
        {

            var itemAsObj = getItemListAsObject(items);



            if (HttpContext.Current.Items["HandlebarDataSource"] == null)
            {
                HttpContext.Current.Items.Add("HandlebarDataSource", itemAsObj);
            }
            else
            {
                HttpContext.Current.Items["HandlebarDataSource"] = itemAsObj;
            }
        }

        public static IHtmlString GetTemplatedContent(Item handlebarTemplate, Item templateData, object model = null)
        {
            var itemAsObj = getItemAsObject(templateData, model);
            return GetTemplatedContent(handlebarTemplate, itemAsObj);
        }

        public static IHtmlString GetTemplatedContent(Item handlebarTemplate, object targetData)
        {
            //field from variant is named Template
            var template = GetCompiledTemplate(handlebarTemplate, "Template");

            if (targetData != null)
            {
                //Wrap with Dynamic Object
                string output = string.Empty;
                try
                {
                    output = template(targetData);
                }
                catch (Exception ex)
                {
                    output = string.Format("Template Error: {0}", ex);
                }

                return new MvcHtmlString(output);
            }

            return new MvcHtmlString("<p>No Data Exists to Bind</p>");
        }

        public static IHtmlString GetTemplatedContent(this HtmlHelper helper, Item handlebarTemplate)
        {
            var template = GetCompiledTemplate(handlebarTemplate);
            var templateData = HttpContext.Current.Items["HandlebarDataSource"];
            if (templateData != null)
            {
                string output = string.Empty;
                try
                {
                    output = template(templateData);
                }
                catch (Exception ex)
                {
                    output = string.Format("Template Error: {0}", ex);
                }

                return new MvcHtmlString(output);
            }

            return new MvcHtmlString("<p>No Data Exists to Bind</p>");
        }

        private static object lockObject = new object();

        private static Func<object, string> GetCompiledTemplate(Item handlebarTemplate, string templateField = "Content")
        {
            bool bypassHttpCache = Sitecore.Context.PageMode.IsPreview ||
                    Sitecore.Context.PageMode.IsExperienceEditor ||
                    Sitecore.Context.PageMode.IsSimulatedDevicePreviewing;

            Dictionary<Guid, Func<object, string>> compiledTemplates = null;
            Guid templateID = Guid.Empty;

            bool updateCache = false;

            //avoid competing compilations
            lock (lockObject)
            {
                if (!bypassHttpCache)
                {
                    compiledTemplates = HttpRuntime.Cache["CompiledHandlebarTemplates"] as Dictionary<Guid, Func<object, string>>;
                }

                if (compiledTemplates == null)
                {
                    compiledTemplates = new Dictionary<Guid, Func<object, string>>();
                    updateCache = true;
                }

                templateID = handlebarTemplate.ID.ToGuid();
                if (!compiledTemplates.ContainsKey(templateID))
                {
                    updateCache = true;
                    var templateContent = handlebarTemplate.Fields[templateField].Value;

                    if (Sitecore.Context.PageMode.IsExperienceEditorEditing)
                    {
                        //force replace everything to include html
                        templateContent = templateContent.Replace("{{", "{{{");
                        templateContent = templateContent.Replace("}}", "}}}");
                        //fix html to not have 4 from the first replace
                        templateContent = templateContent.Replace("{{{{", "{{{");
                        templateContent = templateContent.Replace("}}}}", "}}}");
                    }

                    var template = HandlebarsDotNet.Handlebars.Compile(templateContent);
                    compiledTemplates.Add(templateID, template);
                }

                if (updateCache && !bypassHttpCache)
                {
                    HttpRuntime.Cache.Insert("CompiledHandlebarTemplates", compiledTemplates);
                }
            }

            return compiledTemplates[templateID];
        }

        private static object getItemListAsObject(List<Item> items)
        {
            var myDynamicItem = new ExpandoObject() as IDictionary<string, Object>;

            myDynamicItem.Add("IsPageEditorEditing", Sitecore.Context.PageMode.IsExperienceEditorEditing ? true : false);

            List<object> children = new List<object>();

            foreach (var childItem in items)
            {
                children.Add(getItemAsObject(childItem));
            }
            myDynamicItem.Add("Items", children);
            return myDynamicItem;
        }

        private static object getItemAsObject(Item item, object model = null)
        {

            var myItem = new DynamicItem(item);
            if (model != null)
            {
                myItem.Model = model;
            }

            return myItem;
        }
    } 


    
}
