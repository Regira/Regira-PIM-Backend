using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Catalog.Articles;

namespace Webshop.Admin.API.Controllers;

[ApiController, Route("articles")]
public class ArticleController
    : EntityControllerBase<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes, ArticleDto, ArticleInputDto>;
