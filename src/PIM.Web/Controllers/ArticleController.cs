using Microsoft.AspNetCore.Mvc;
using PIM.Models.Catalog.Articles;
using PIM.Models.Catalog.Articles.DTO;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("articles")]
public class ArticleController
    : EntityControllerBase<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes, ArticleDto, ArticleInputDto>;