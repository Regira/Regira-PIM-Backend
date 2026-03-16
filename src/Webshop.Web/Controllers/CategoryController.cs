using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Classification.Categories;

namespace Webshop.Web.Controllers;

[ApiController, Route("categories")]
public class CategoryController
    : EntityControllerBase<Category, CategorySearchObject, EntitySortBy, CategoryIncludes, CategoryDto, CategoryInputDto>;
