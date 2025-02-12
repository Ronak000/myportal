using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPortal
{
    public interface ITokenManager
    {
        Task<TokenDTO> GetNewAccessTokenAsync(string MicrosoftUrl, string TenantId, string ClientId, string ClientSecret);
    }
}