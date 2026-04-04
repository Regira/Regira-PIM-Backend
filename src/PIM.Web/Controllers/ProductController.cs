using Microsoft.AspNetCore.Mvc;
using PIM.Models.Catalog.Products;
using PIM.Models.Catalog.Products.DTO;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("products")]
public class ProductController
    : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, ProductIncludes, ProductDto, ProductInputDto>;