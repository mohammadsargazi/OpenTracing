using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;
using User.Models;
using User.Services;

namespace User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        #region Fields
        private readonly IUserService _userService;
        private readonly ITracer _tracer;
        #endregion

        #region Ctor
        public UsersController(IUserService userService, ITracer tracer)
        {
            _userService = userService;
            _tracer = tracer;
        }
        #endregion

        [HttpGet]
        public async Task<ActionResult> GetUsers()
            => Ok(await _userService.GetUsersAsync());

        [HttpPost]
        public async Task<ActionResult> AddUser(UserViewModel model)
        {
            var actionName = ControllerContext.ActionDescriptor.DisplayName;
            using var scope = _tracer.BuildSpan(actionName).StartActive(true);
            scope.Span.Log($"Add user log username {model.FullName}");
            return Ok(await _userService.AddUserAsync(new Models.User { FullName=model.FullName,MobileNumber=model.MobileNumber}));
        }
    }
}
