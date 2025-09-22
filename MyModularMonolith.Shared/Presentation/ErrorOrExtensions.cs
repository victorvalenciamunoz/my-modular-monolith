using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace MyModularMonolith.Shared.Presentation
{
    public static class ErrorOrExtensions
    {
        public static IResult ToProblemDetails<T>(this ErrorOr<T> errorOr)
        {
            if (errorOr.IsError)
            {
                return CreateProblemDetails(errorOr.Errors);
            }

            throw new InvalidOperationException("Cannot convert successful result to problem details");
        }

        private static IResult CreateProblemDetails(List<Error> errors)
        {
            if (errors.Count == 0)
            {
                return Results.Problem();
            }

            if (errors.Count == 1)
            {
                return CreateProblemDetailsForSingleError(errors.First());
            }

            return CreateValidationProblemDetails(errors);
        }

        private static IResult CreateProblemDetailsForSingleError(Error error)
        {
            var statusCode = GetStatusCode(error.Type);

            return Results.Problem(
                statusCode: statusCode,
                title: GetTitle(error.Type),
                detail: error.Description,
                type: $"https://errors.mymodularmonolith.com/{error.Code}",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = error.Code,
                    ["timestamp"] = DateTime.UtcNow
                }
            );
        }

        private static IResult CreateValidationProblemDetails(List<Error> errors)
        {
            var validationErrors = new Dictionary<string, string[]>();
            var otherErrors = new List<Error>();

            foreach (var error in errors)
            {
                if (error.Type == ErrorType.Validation)
                {
                    var field = ExtractFieldFromErrorCode(error.Code);
                    validationErrors[field] = [error.Description];
                }
                else
                {
                    otherErrors.Add(error);
                }
            }

            if (validationErrors.Count > 0)
            {
                return Results.ValidationProblem(
                    errors: validationErrors,
                    title: "One or more validation errors occurred",
                    type: "https://errors.mymodularmonolith.com/validation",
                    extensions: new Dictionary<string, object?>
                    {
                        ["timestamp"] = DateTime.UtcNow,
                        ["additionalErrors"] = otherErrors.Select(e => new { e.Code, e.Description }).ToArray()
                    }
                );
            }

            return CreateProblemDetailsForSingleError(otherErrors.First());
        }

        private static string ExtractFieldFromErrorCode(string errorCode)
        {
            var parts = errorCode.Split('.');
            return parts.Length > 1 ? parts[1] : "General";
        }

        private static int GetStatusCode(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Failure => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static string GetTitle(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.NotFound => "Resource Not Found",
                ErrorType.Validation => "Validation Error",
                ErrorType.Conflict => "Conflict",
                ErrorType.Unauthorized => "Unauthorized",
                ErrorType.Forbidden => "Forbidden",
                ErrorType.Failure => "Internal Server Error",
                _ => "An error occurred"
            };
        }
    }
}
