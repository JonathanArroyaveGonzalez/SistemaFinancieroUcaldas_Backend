using SAPFIAI.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace SAPFIAI.Infrastructure.Identity;

public static class IdentityResultExtensions
{
    public static Result ToApplicationResult(this IdentityResult result)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(e => new Error(e.Code, e.Description)).FirstOrDefault() ?? Error.Failure("Identity", "Unknown error"));
    }
}
