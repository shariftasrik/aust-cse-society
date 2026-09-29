namespace AustCseApp.Services
{
    public class PrivateFileStore
    {
        private readonly IWebHostEnvironment _environment;

        public PrivateFileStore(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SavePdfAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0) return null;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf") return null;
            if (file.Length > 15 * 1024 * 1024) return null;

            var relativeDirectory = Path.Combine("App_Data", folder);
            var absoluteDirectory = Path.Combine(_environment.ContentRootPath, relativeDirectory);
            Directory.CreateDirectory(absoluteDirectory);

            var fileName = Guid.NewGuid().ToString("N") + extension;
            var absolutePath = Path.Combine(absoluteDirectory, fileName);
            await using var stream = File.Create(absolutePath);
            await file.CopyToAsync(stream);
            return Path.Combine(folder, fileName).Replace('\\', '/');
        }

        public string? AbsolutePath(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;
            var cleaned = relativePath.Replace('/', Path.DirectorySeparatorChar);
            var full = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "App_Data", cleaned));
            var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "App_Data"));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
                return null;
            return full;
        }
    }
}
