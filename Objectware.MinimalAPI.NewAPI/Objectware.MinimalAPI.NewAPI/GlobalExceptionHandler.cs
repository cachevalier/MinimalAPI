
using Microsoft.AspNetCore.Diagnostics;

namespace Objectware.MinimalAPI.NewAPI;
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception");

        await TypedResults
            .Problem(statusCode: StatusCodes.Status500InternalServerError)
            .ExecuteAsync(context);

        return true;
    }
}
