namespace AustCseApp
{
    public static class ProfilePhoto
    {
        public static string Src(string? url)
        {
            const string fallback = "~/images/avatar/user.svg";
            if (string.IsNullOrWhiteSpace(url)) return fallback;

            var trimmed = url.Trim().Replace('\\', '/');
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var path = trimmed.Split('?', '#')[0];
                if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed;
                }

                return fallback;
            }

            if (trimmed.StartsWith("~/")) return trimmed;
            if (trimmed.StartsWith("/")) return "~" + trimmed;
            return "~/" + trimmed.TrimStart('/');
        }
    }
}
