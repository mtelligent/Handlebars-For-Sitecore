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
    public class HandlebarFacetContainerController : StandardController
    {
        protected readonly IHandlebarFacetContainerRepository HandlebarFacetContainerRepository;

        public HandlebarFacetContainerController(IHandlebarFacetContainerRepository repository)
        {
            this.HandlebarFacetContainerRepository = repository;
        }

        protected override object GetModel()
        {
            return HandlebarFacetContainerRepository.GetModel();
        }

        public override ActionResult Index()
        {
            var model = this.GetModel() as HandlebarContainerModel;
            HandlebarManager.SetupFacetContainer();
            return View(model);
        }
    }
}