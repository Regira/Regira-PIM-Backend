using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Taxonomy.FacetGroups;

namespace Webshop.Web.Controllers;

[ApiController, Route("facet-groups")]
public class FacetGroupController
    : EntityControllerBase<FacetGroup, FacetGroupSearchObject, EntitySortBy, FacetGroupIncludes, FacetGroupDto, FacetGroupInputDto>;
