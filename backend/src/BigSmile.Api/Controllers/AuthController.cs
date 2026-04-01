using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BigSmile.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserTenantMembershipRepository _membershipRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserTenantMembershipRepository membershipRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _membershipRepository = membershipRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
                return Unauthorized("Invalid email or password.");

            // Verify password
            if (!_passwordHasher.VerifyPassword(user.HashedPassword, request.Password))
                return Unauthorized("Invalid email or password.");

            // Get user's memberships
            var memberships = await _membershipRepository.GetByUserIdAsync(user.Id);
            if (!memberships.Any())
                return Unauthorized("User is not a member of any tenant.");

            // For simplicity, pick the first active membership
            var membership = memberships.FirstOrDefault(m => m.IsActive);
            if (membership == null)
                return Unauthorized("No active tenant membership.");

            // Generate token
            var token = _jwtTokenService.GenerateToken(user, membership.Tenant, membership.Role);

            return new LoginResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName
                },
                Tenant = new TenantDto
                {
                    Id = membership.Tenant.Id,
                    Name = membership.Tenant.Name
                },
                Role = membership.Role.Name
            };
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            // This endpoint requires authentication; we'll rely on JWT middleware to set User.Identity
            // For now, return placeholder; we'll implement after we have tenant resolution from JWT claim.
            return Unauthorized("Not implemented yet.");
        }

        #region DTOs

        public class LoginRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, MinLength(6)]
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public UserDto User { get; set; } = null!;
            public TenantDto Tenant { get; set; } = null!;
            public string Role { get; set; } = string.Empty;
        }

        public class UserDto
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? DisplayName { get; set; }
        }

        public class TenantDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        #endregion
    }
}