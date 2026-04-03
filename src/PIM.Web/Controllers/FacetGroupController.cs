using Microsoft.AspNetCore.Mvc;
using PIM.Models.Taxonomy.FacetGroups;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("facet-groups")]
public class FacetGroupController
    : EntityControllerBase<FacetGroup, FacetGroupSearchObject, EntitySortBy, FacetGroupIncludes, FacetGroupDto, FacetGroupInputDto>;
