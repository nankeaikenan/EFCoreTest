namespace EFCoreDemo.Application.DTOs.Auth
{
    public record RegisterResult
    {
        public AuthResponse? Response { get; init; }
        public bool EmailAlreadyExists { get; init; }
        public IReadOnlyList<string> ValidationErrors { get; init; } = [];

        public bool Succeeded => Response is not null;

        public static RegisterResult Success(AuthResponse response) =>
            new() { Response = response };

        public static RegisterResult Conflict() =>
            new() { EmailAlreadyExists = true };

        public static RegisterResult Failure(IEnumerable<string> errors) =>
            new() { ValidationErrors = errors.ToList() };
    }
}
