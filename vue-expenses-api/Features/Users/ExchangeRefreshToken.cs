﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using vue_expenses_api.Dtos;
using vue_expenses_api.Infrastructure;
using vue_expenses_api.Infrastructure.Exceptions;
using vue_expenses_api.Infrastructure.Security;

namespace vue_expenses_api.Features.Users
{
    public class ExchangeRefreshToken
    {
        public class Command : IRequest<UserDto>
        {
            public Command(
                string token,
                string refreshToken)
            {
                Token = token;
                RefreshToken = refreshToken;
            }

            public string Token { get; set; }
            public string RefreshToken { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.RefreshToken).NotNull().NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, UserDto>
        {
            private readonly ExpensesContext _context;
            private readonly IJwtTokenGenerator _jwtTokenGenerator;
            private IOptions<JwtSettings> _jwtSettings;

            public Handler(
                ExpensesContext context,
                IJwtTokenGenerator jwtTokenGenerator,
                IOptions<JwtSettings> jwtSettings)
            {
                _context = context;
                _jwtTokenGenerator = jwtTokenGenerator;
                _jwtSettings = jwtSettings;
            }

            public async Task<UserDto> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var email = GetIdentifierFromExpiredToken(request.Token).Value;

                var user = await _context.Users.Include(u => u.RefreshTokens)
                    .SingleAsync(
                        x => x.Email == email && !x.Archived,
                        cancellationToken);

                if (user == null)
                {
                    throw new HttpException(
                        HttpStatusCode.Unauthorized,
                        new {Error = "Invalid credentials."});
                }

                if (!user.IsValidRefreshToken(request.RefreshToken))
                {
                    throw new HttpException(
                        HttpStatusCode.Unauthorized,
                        new {Error = "Invalid credentials."});
                }

                var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
                user.RemoveRefreshToken(request.RefreshToken);
                user.AddRefreshToken(
                    refreshToken,
                    user.Id);
                var token = await _jwtTokenGenerator.CreateToken(user.Email);
                await _context.SaveChangesAsync(cancellationToken);

                return new UserDto(
                    user.FirstName,
                    user.LastName,
                    user.FullName,
                    user.SystemName,
                    user.Email,
                    token,
                    refreshToken,
                    user.CurrencyRegionName,
                    user.UseDarkMode,
                    user.Role, user.Region);
            }


            private Claim GetIdentifierFromExpiredToken(
                string token)
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _jwtSettings.Value.SigningCredentials.Key,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Value.Audience,
                    ValidateLifetime = false, // do not check for expiry date time
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out var securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Contains(
                        SecurityAlgorithms.HmacSha256Signature,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
            }
        }
    }
}