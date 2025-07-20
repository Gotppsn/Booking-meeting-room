using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Project001.Controllers;

public class LanguageController : Controller
{
    [HttpGet]
    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            // Validate culture is supported
            var supportedCultures = new[] { "en", "th", "ja" };
            if (supportedCultures.Contains(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)),
                    new CookieOptions { 
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        Path = "/",
                        HttpOnly = false,
                        SameSite = SameSiteMode.Lax,
                        IsEssential = true,
                        Secure = false // Allow for localhost testing
                    }
                );
                
                // Add TempData for debugging
                TempData["LanguageChanged"] = $"Language changed to: {culture}";
            }
        }

        // First try the returnUrl parameter
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        // Fallback to referer with improved URL validation
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            try
            {
                var refererUri = new Uri(referer);
                var requestUri = new Uri($"{Request.Scheme}://{Request.Host}");
                
                // Check if referer is from the same host
                if (refererUri.Host.Equals(requestUri.Host, StringComparison.OrdinalIgnoreCase))
                {
                    return LocalRedirect(refererUri.PathAndQuery);
                }
            }
            catch (UriFormatException)
            {
                // Invalid referer URL, continue to fallback
            }
        }
        
        return LocalRedirect("/");
    }
}