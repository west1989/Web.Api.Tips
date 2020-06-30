using Marvin.StreamExtensions;
using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
  public class HttpHandlersService : IIntegrationService
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();

    private static HttpClient _notSoNicelyInstantiatedHttpClient =
      new HttpClient(
        new RetryPolicyDelegatingHandler(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip }, 2)
      );

    public HttpHandlersService(IHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
    }

    public async Task Run()
    {
      await GetMoviesWithRetryPolice(_cancelationTokenSource.Token);
    }


    private async Task GetMoviesWithRetryPolice(CancellationToken cancellationToken)
    {
      var httpClient = _httpClientFactory.CreateClient("MoviesClient");
      var request = new HttpRequestMessage(HttpMethod.Get, "api/movies/6e87f657-f2c1-4d90-9b37-cbe43cc6adb9");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

      using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
      {
        if (!response.IsSuccessStatusCode)
        {
          if (response.StatusCode == HttpStatusCode.NotFound)
          {
            Console.WriteLine("The request movie cannot be found!");
            return;
          }
          else if (response.StatusCode == HttpStatusCode.Unauthorized)
          {
            // trigger to login flow.
            return;
          }
          response.EnsureSuccessStatusCode();
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var movie = stream.ReadAndDeserializeFromJson<Movie>();
      }
    }

  }
}
