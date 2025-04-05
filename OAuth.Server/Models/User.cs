using System;
using System.Collections.Generic;

namespace OAuth.Server.Models;

public partial class User
{
    public long Id { get; set; }

    public string? Email { get; set; }

    public string? Username { get; set; }

    public string AuthType { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public DateTime ExpireTimestamp { get; set; }

    public string? AccessToken { get; set; }

    public string? Password { get; set; }
}
