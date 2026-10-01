using GYM.Domain.Shared;

namespace GYM.Application.Errors;

public static class ServiceErrors
{
    public static class Common
    {
        public static readonly Error NotFound = new("Common.NotFound", "The requested resource was not found.");
    }
}