using Microsoft.AspNetCore.Mvc;
using PIM.Models.Orders;
using Regira.Entities.Web.Controllers.Abstractions;

namespace PIM.Web.Controllers;

[ApiController, Route("orders")]
public class OrderController
    : EntityControllerBase<Order, OrderSearchObject, OrderSortBy, OrderIncludes, OrderDto, OrderInputDto>;
