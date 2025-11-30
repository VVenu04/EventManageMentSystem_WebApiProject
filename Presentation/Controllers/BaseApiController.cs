using Application.Interface.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        private ICurrentUserService _currentUserService;

        protected Guid CurrentUserId
        {
            get
            {
                _currentUserService ??= HttpContext.RequestServices.GetService<ICurrentUserService>();
                return _currentUserService?.UserId ?? Guid.Empty;
            }
        }
    }
}
