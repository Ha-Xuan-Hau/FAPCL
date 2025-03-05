using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace FAPCL.Model
{
    public partial class AspNetRoleClaim : IdentityRoleClaim<string>
    {
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }

        public virtual AspNetRole Role { get; set; } = null!;
    }
}
