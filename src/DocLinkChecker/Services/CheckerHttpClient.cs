namespace DocLinkChecker.Services
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DocLinkChecker.Models;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// HttpClient used to check external links.
    /// </summary>
    public class CheckerHttpClient
    {
        private readonly AppConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckerHttpClient"/> class.
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="config">App configuration.</param>
        public CheckerHttpClient(HttpClient client, IOptions<AppConfig> config)
        {
            Client = client;
            _config = config.Value;
        }

        private HttpClient Client { get; }

        /// <summary>
        /// Verify resource.
        /// </summary>
        /// <param name="address">Address to verify.</param>
        /// <returns>Value indicating whether check was succesful, the HTTP status code and error string.</returns>
        public Task<(bool success, HttpStatusCode? statusCode, string error)> VerifyResource(string address)
        {
            return VerifyResource(address, 0);
        }

        private async Task<(bool success, HttpStatusCode? statusCode, string error)> VerifyResource(string address, int depth)
        {
            try
            {
                if (depth >= _config.DocLinkChecker.MaxHttpRedirects)
                {
                    throw new Exception($"Excessive number of redirects");
                }

                Uri uri = new Uri(address);
                var ipHost = await Dns.GetHostEntryAsync(uri.DnsSafeHost);
                if (ipHost == null || ipHost.AddressList.Length == 0)
                {
                    return (false, null, $"Invalid host name: {uri.DnsSafeHost}");
                }

                var ip = ipHost.AddressList.First();
                using var response = await Client.GetAsync(address, HttpCompletionOption.ResponseHeadersRead);
                if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
                {
                    var redirectUri = response.Headers.Location;
                    var requestUri = response.RequestMessage?.RequestUri;
                    Debug.WriteLine($"Location {redirectUri}");
                    if (redirectUri != null && !redirectUri.IsAbsoluteUri && requestUri != null)
                    {
                        var authority = requestUri.GetLeftPart(UriPartial.Authority);
                        var resource = redirectUri.ToString();
                        string path;
                        if (resource.StartsWith("/"))
                        {
                            path = string.Empty;
                        }
                        else
                        {
                            path = string.Join(string.Empty, requestUri.Segments.Take(requestUri.Segments.Length - 1));
                        }

                        redirectUri = new Uri(string.Join(string.Empty, authority, path, resource), UriKind.Absolute);

                        if (redirectUri == response.Headers.Location)
                        {
                            throw new Exception($"Redirection loop detected for {response.Headers.Location}");
                        }
                    }

                    Debug.WriteLine($"Redirect: {address} => {redirectUri}");
                    return await VerifyResource(redirectUri!.ToString(), depth + 1);
                }

                return (response.IsSuccessStatusCode, response.StatusCode, string.Empty);
            }
            catch (HttpRequestException err)
            {
                Debug.WriteLine($"Failed with status {err.StatusCode}");
                return (false, null, $"HttpRequestException - address: {address}");
            }
            catch (Exception err)
            {
                return (false, null, $"{err.Message} - address: {address}");
            }
        }
    }
}