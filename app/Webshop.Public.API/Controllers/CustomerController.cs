using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Clients.Customers;

namespace Webshop.Public.API.Controllers;

[ApiController, Route("customers")]
public class CustomerController
    : EntityControllerBase<Customer, int, CustomerSearchObject, CustomerSortBy, CustomerIncludes, CustomerDto, CustomerInputDto>;
