using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Base.API.Common;

internal class TokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : IdentityUser<Guid>
{
    public TokenProvider(IDataProtectionProvider dataProtectionProvider,
        IOptions<TokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {

    }
}

internal class TokenProviderOptions : DataProtectionTokenProviderOptions
{
    public TokenProviderOptions()
    {
        Name = "TokenProvider";
        TokenLifespan = TimeSpan.FromHours(12);
    }
}
