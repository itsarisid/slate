using Microsoft.AspNetCore.Http;

namespace Alphabet.Modules.IdentityModule.Api.Models
{
    public sealed class UploadAvatarRequest
    {
        public IFormFile Avatar { get; set; } = default!;
    }
}
