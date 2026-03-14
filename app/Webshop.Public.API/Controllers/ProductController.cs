using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Catalog.Products;

namespace Webshop.Public.API.Controllers;

[ApiController, Route("products")]
public class ProductController
    : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInputDto>;
