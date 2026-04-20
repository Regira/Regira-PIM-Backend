//using Microsoft.AspNetCore.Mvc;
//using PIM.Admin.API.Models;
//using PIM.Core.Constants;
//using PIM.Identity.Models;
//using Regira.Entities.Services.Abstractions;
//using Regira.Security.Authentication.Jwt.Extensions;

//namespace PIM.Admin.API.Controllers;

//[ApiController]
//[Route("profile")]
//public class ProfileController(IEntityRepository<PimUserEntity, string> userService) : ControllerBase
//{
//    [HttpPost("personal-data")]
//    public async Task<IActionResult> ChangePersonalData(ChangePersonalDataInput model)
//    {
//        var userId = User.FindUserId()!;
//        if (string.IsNullOrWhiteSpace(userId))
//        {
//            return NotFound();
//        }
//        var item = await userService.Details(userId);
//        if (item == null)
//        {
//            return NotFound();
//        }


//        item.GivenName = model.GivenName;
//        item.LastName = model.LastName;
//        var claims = item.UserClaims!.ToList();
//        var culture = claims.FirstOrDefault(c => c.ClaimType == PimClaimTypes.Culture);
//        if (culture?.ClaimValue != model.Culture)
//        {
//            claims.RemoveAll(x => x.ClaimType == PimClaimTypes.Culture);
//            claims.Add(new IdentityUserClaimEntity
//            {
//                ClaimType = PimClaimTypes.Culture,
//                ClaimValue = model.Culture
//            });
//            await userService.Save(item);
//            await userService.SaveChanges();
//        }

//        return Ok();
//    }
//}