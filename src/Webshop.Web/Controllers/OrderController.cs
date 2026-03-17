using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Models.Orders;

namespace Webshop.Web.Controllers;

[ApiController, Route("orders")]
public class OrderController
    : EntityControllerBase<Order, OrderSearchObject, OrderSortBy, OrderIncludes, OrderDto, OrderInputDto>;
