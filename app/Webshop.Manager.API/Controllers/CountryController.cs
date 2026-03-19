using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Countries;

namespace Webshop.Manager.API.Controllers;

[ApiController]
[Route("countries")]
public class CountryController : EntityControllerBase<Country, string, SearchObject<string>, CountryDto, CountryDto>;