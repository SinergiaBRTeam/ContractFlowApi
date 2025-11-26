using Microsoft.AspNetCore.Http;

namespace ContractsMvc.Models.Dtos;

public class FileUploadDto
{
    public IFormFile File { get; set; } = null!;
}
