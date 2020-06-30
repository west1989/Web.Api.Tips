using Marvin.StreamExtensions;
using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
  public class HttpClientFactoryInstanceManagementService : IIntegrationService
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MoviesClient _moviesClient;
    private CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();

    public HttpClientFactoryInstanceManagementService(IHttpClientFactory httpClientFactory, MoviesClient moviesClient)
    {
      _httpClientFactory = httpClientFactory;
      _moviesClient = moviesClient;
    }

    public async Task Run()
    {
      // await TestDisposeHttpClient(_cancelationTokenSource.Token);      
      // await TestReuseHttpClient(_cancelationTokenSource.Token);
      // await GetMoviesWithHttpClientFromFactory(_cancelationTokenSource.Token);
      // await GetMoviesWithTypedHttpClientFromFactory(_cancelationTokenSource.Token);
      await GetMoviesViaMoviesClient(_cancelationTokenSource.Token);
    }

    private async Task TestDisposeHttpClient(CancellationToken cancellationToken)
    {
      
      for (int i = 0; i < 10; i++)
      {
        using (var httpClient = new HttpClient())
        {
          var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com");

          using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
          {
            var stream = await response.Content.ReadAsStreamAsync();
            response.EnsureSuccessStatusCode();

            Console.WriteLine($"Request completed with status code: {response.StatusCode}");
          }
        }
      }
    }


    private async Task TestReuseHttpClient(CancellationToken cancellationToken)
    {
      var httpClient = new HttpClient();

      for (int i = 0; i < 10; i++)
      {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com");

        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
          var stream = await response.Content.ReadAsStreamAsync();
          response.EnsureSuccessStatusCode();

          Console.WriteLine($"Request completed with status code: {response.StatusCode}");
        }

      }

    }

    private async Task GetMoviesWithHttpClientFromFactory(CancellationToken cancellationToken)
    {
      var httpClient = _httpClientFactory.CreateClient("MoviesClient");

      var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

      using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
      {
        var stream = await response.Content.ReadAsStreamAsync();
        response.EnsureSuccessStatusCode();

        var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
      }
    }

    //private async Task GetMoviesWithTypedHttpClientFromFactory(CancellationToken cancellationToken)
    //{
    //  var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
    //  request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    //  request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

    //  using (var response = await _moviesClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
    //  {
    //    response.EnsureSuccessStatusCode();
    //    var stream = await response.Content.ReadAsStreamAsync();

    //    var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
    //  }
    //}

    private async Task GetMoviesViaMoviesClient(CancellationToken cancellationToken)
    {
      var movies = await _moviesClient.GetMovies(cancellationToken);
    }
  }
}
