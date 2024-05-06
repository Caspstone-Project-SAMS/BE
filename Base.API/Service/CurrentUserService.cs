using Base.Service.Common;
using System.Security.Claims;

namespace Base.API.Service
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Undefined";

        public string UserName =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? "Undefined";

        public IEnumerable<string> Roles =>
            _httpContextAccessor.HttpContext?.User?.FindAll(c => c.Type == "scope").Select(c => c.Value) ?? Enumerable.Empty<string>();
    }
}
