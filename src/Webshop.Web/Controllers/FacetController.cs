using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Taxonomy.Facets;

namespace Webshop.Web.Controllers;

[ApiController, Route("facets")]
public class FacetController
    : EntityControllerBase<Facet, FacetSearchObject, EntitySortBy, FacetIncludes, FacetDto, FacetInputDto>;
