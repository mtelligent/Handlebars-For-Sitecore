using SF.Feature.Handlebars.SXA.Models;
using SF.Feature.Handlebars.SXA.Repositories;
using Sitecore.XA.Foundation.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SF.Foundation.Handlebars;


namespace SF.Feature.Handlebars.SXA.Controllers
{
    public class HandlebarJsonContainerController : StandardController
    {
        protected readonly IHandlebarJsonContainerRepository HandlebarContainerJsonRepository;

        public HandlebarJsonContainerController(IHandlebarJsonContainerRepository repository)
        {
            this.HandlebarContainerJsonRepository = repository;
        }

        protected override object GetModel()
        {
            return HandlebarContainerJsonRepository.GetModel();
        }

        public override ActionResult Index()
        {
            var model = this.GetModel() as HandlebarJsonContainerModel;
            HandlebarManager.SetupJsonContainer(model.JsonUrl);
            return View(model);
        }
    }
}