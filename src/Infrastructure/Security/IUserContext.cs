namespace Roller.Infrastructure.Security;

public interface IUserContext<TId> where TId : IEquatable<TId>
{
    TId Id { get; set; }

    string Username { get; set; }

    string Name { get; set; }

    string Email { get; set; }

    string[] RoleIds { get; set; }

    string RemoteIpAddress { get; set; }
}

public class UserContext<TId> : IUserContext<TId> where TId : IEquatable<TId>
{
    public TId Id { get; set; }

    public string Username { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string[] RoleIds { get; set; }

    public string RemoteIpAddress { get; set; }
}