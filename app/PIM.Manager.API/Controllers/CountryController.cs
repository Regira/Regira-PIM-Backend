using Microsoft.AspNetCore.Mvc;
using PIM.Models.Countries;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Admin.API.Controllers;

[ApiController]
[Route("countries")]
public class CountryController : EntityControllerBase<Country, string, SearchObject<string>, CountryDto, CountryDto>;