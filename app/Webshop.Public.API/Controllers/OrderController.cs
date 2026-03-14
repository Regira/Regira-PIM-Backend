using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Entities.Orders;

namespace Webshop.Public.API.Controllers;

[ApiController, Route("orders")]
public class OrderController
    : EntityControllerBase<Order, OrderSearchObject, OrderSortBy, OrderIncludes, OrderDto, OrderInputDto>;
