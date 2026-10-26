using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

public class ActiveStatusAuthorizationHandler : AuthorizationHandler<ActiveStatusRequirement>
{
    private readonly AppDbContext _context;
    public ActiveStatusAuthorizationHandler(AppDbContext context)
    {
        _context = context;
    }
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ActiveStatusRequirement requirement)
    {
        
            CancellationToken cancellationToken = (context.Resource as HttpContext)?.RequestAborted ?? throw new InvalidCastException("Authorization handler was not called through a http request");    
            // Get the current users Id
            var userId = context.User.GetCurrentUser();

            // Check if the user is active or not
            var isUserActive = await _context.Users.AnyAsync(u => u.UserId == userId && u.Status == UserStatus.Active, cancellationToken);

            if (isUserActive)
            {
                context.Succeed(requirement);
            }
    
    }
}