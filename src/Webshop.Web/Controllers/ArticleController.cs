using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Catalog.Articles;

namespace Webshop.Web.Controllers;

[ApiController, Route("articles")]
public class ArticleController
    : EntityControllerBase<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes, ArticleDto, ArticleInputDto>;