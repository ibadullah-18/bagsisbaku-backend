using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace bagsisbaku.Api.Models.Media;

public sealed class UploadImageForm
{
    [Required]
    public IFormFile File { get; init; } = null!;
}
