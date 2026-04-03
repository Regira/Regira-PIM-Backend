using Microsoft.AspNetCore.Mvc;
using PIM.Models.Taxonomy.Facets;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("facets")]
public class FacetController
    : EntityControllerBase<Facet, FacetSearchObject, EntitySortBy, FacetIncludes, FacetDto, FacetInputDto>;
