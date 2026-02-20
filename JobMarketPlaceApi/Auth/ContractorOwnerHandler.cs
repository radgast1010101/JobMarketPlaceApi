// JobMarketPlaceApi\Auth\ContractorOwnerHandler.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JobMarketPlaceApi.Auth
{
    public class ContractorOwnerHandler : AuthorizationHandler<ContractorOwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContractorOwnerHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ContractorOwnerRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Try read route value "contractorId"
            if (!httpContext.Request.RouteValues.TryGetValue("contractorId", out var val) || val is null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (!Guid.TryParse(val.ToString(), out var contractorId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Get subject claim (support NameIdentifier and sub)
            var subject = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(subject) && Guid.TryParse(subject, out var subjectGuid) && subjectGuid == contractorId)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}