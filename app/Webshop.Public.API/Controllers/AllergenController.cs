using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Catalog.Allergens;

namespace Webshop.Public.API.Controllers;

[ApiController, Route("allergens")]
public class AllergenController
    : EntityControllerBase<Allergen, int, AllergenSearchObject, AllergenDto, AllergenInputDto>;
