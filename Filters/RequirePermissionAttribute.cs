using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyHeThongBanHang.Data;
using System.Security.Claims;

namespace QuanLyHeThongBanHang.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permissionCode) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { permissionCode };
        }
    }

    public class RequirePermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _permissionCode;
        private readonly AppDbContext _context;

        public RequirePermissionFilter(string permissionCode, AppDbContext context)
        {
            _permissionCode = permissionCode;
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Respect [AllowAnonymous] - if present, skip permission check
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute))
            {
                return;
            }

            var user = context.HttpContext.User;
            
            // Allow if user is not authenticated (should be handled by [Authorize])
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Always allow Admin full access to bypass these checks
            if (user.IsInRole("Admin"))
            {
                return;
            }

            // Get userId from claims
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Check if user has the specific permission in DB
            bool hasPermission = await _context.UserPermissions
                .Include(up => up.Function)
                .AnyAsync(up => up.UserId == userId && up.Function != null && up.Function.Code == _permissionCode);

            if (!hasPermission)
            {
                context.Result = new ForbidResult(); // Return 403 Forbidden
            }
        }
    }
}
