using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MyModularMonolith.Modules.Users.Application.Services;
using MyModularMonolith.Modules.Users.Contracts;
using MyModularMonolith.Modules.Users.Contracts.Commands;
using MyModularMonolith.Modules.Users.Contracts.Queries.GetByUserId;
using MyModularMonolith.Modules.Users.Domain;
using MyModularMonolith.Modules.Users.Presentation.Models;
using MyModularMonolith.Shared.Domain.ValueObjects;
using MyModularMonolith.Shared.Presentation;
using MyModularMonolith.Shared.Security;

namespace MyModularMonolith.Modules.Users.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", RegisterUser)
            .AllowAnonymous()
            .WithName("RegisterUser")
            .WithSummary("Register a new user")
            .Produces<RegistrationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        group.MapPost("/login", LoginUser)
            .AllowAnonymous()
            .RequireRateLimiting("login")  // 🛡️ FASE 1: Rate limiting por IP (20 req/min)
            .WithName("LoginUser")
            .WithSummary("Authenticate user and get tokens")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status429TooManyRequests); // Documentar 429

        group.MapPost("/change-password", ChangePassword)
            .WithName("ChangePassword")
            .WithSummary("Change user password")
            .Produces<PasswordChangeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> RegisterUser(
        [FromBody] RegisterUserRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IUserMetricsService userMetrics,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var email = Email.Create(request.Email);

            var command = new RegisterUserCommand(
                email,
                request.FirstName,
                request.LastName,
                request.Role ?? UserRoles.User,
                request.Password,
                request.HomeGymId,
                request.HomeGymName);

            var result = await mediator.Send(command, cancellationToken);

            return result.Match(
                success =>
                {
                    userMetrics.IncrementUserRegistrations();
                    return Results.Created($"/api/users/{success.Id}", success);
                },
                errors => result.ToProblemDetails()
            );
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = "Invalid email format", details = ex.Message });
        }
    }

    private static async Task<IResult> LoginUser(
        [FromBody] LoginUserRequest request,
        [FromServices] IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginUserCommand(
            request.Email,
            request.Password,
            httpContext.Connection.RemoteIpAddress?.ToString());

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsError)
        {
            return result.ToProblemDetails();
        }

        var login = result.Value;

        var userResult = await mediator.Send(new GetUserByIdQuery(login.UserId), cancellationToken);
        if (userResult.IsError)
        {
            return userResult.ToProblemDetails();
        }

        var authDto = new AuthenticationDto(
            userResult.Value,
            login.AccessToken,
            login.RefreshToken,
            login.RefreshTokenExpires
        );

        var response = AuthenticationResponse.FromDto(authDto);
        return Results.Ok(response);
    }

    private static async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new ChangePasswordCommand(
            request.Email,
            request.CurrentPassword,
            request.NewPassword);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Results.Ok(success),
            errors => result.ToProblemDetails()
        );
    }

    private static async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var command = new RefreshTokenCommand(
            request.RefreshToken,
            httpContext.Connection.RemoteIpAddress?.ToString());

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            success => Results.Ok(success),
            errors => result.ToProblemDetails()
        );
    }
}

