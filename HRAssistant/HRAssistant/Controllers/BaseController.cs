using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRAssistant.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string? Email => User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
    }
}
