namespace BookingQueue.Services;

public class FileUploadService
{
    private readonly IWebHostEnvironment _environment;

    public FileUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string UploadFile(IFormFile file)
    {
        if (file is not { Length: > 0 }) return null!;
        
        var uploads = Path.Combine(_environment.WebRootPath, "uploads");
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploads, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(fileStream);
        }

        return $"/uploads/{fileName}";
    }
}