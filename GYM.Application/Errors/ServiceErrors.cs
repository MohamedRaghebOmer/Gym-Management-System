using GYM.Domain.Shared;

namespace GYM.Application.Errors;

internal static class ServiceErrors
{
    public static class Common
    {
        public static readonly Error UnExcepected = new(
            "Common.Unexpected", 
            "An unexpected error occurred.");

        public static readonly Error NotFound = new(
            "Common.NotFound", 
            "The requested resource was not found.");
    }
}