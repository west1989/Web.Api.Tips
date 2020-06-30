using Marvin.StreamExtensions;
using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
  public class DealingWithErrorsAndFaultsService : IIntegrationService
  {

    private readonly IHttpClientFactory _httpClientFactory;
    private CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();

    public DealingWithErrorsAndFaultsService(IHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
    }

    public async Task Run()
    {
      // await GetMovieAndDealWithInvalidResponses(_cancelationTokenSource.Token);
      await GetMovieAndHandleValidationErrors(_cancelationTokenSource.Token);
    }

    private async Task GetMovieAndDealWithInvalidResponses(CancellationToken cancellationToken)
    {
      var httpClient = _httpClientFactory.CreateClient("MoviesClient");
      var request = new HttpRequestMessage(HttpMethod.Get, "api/movies/16fcbcc4-b7f7-47fc-9382-740c12246b59");
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

    private async Task GetMovieAndHandleValidationErrors(CancellationToken cancellationToken)
    {
      var httpClient = _httpClientFactory.CreateClient("MoviesClient");

      var moviesForCreation = new MovieForCreation
      {
        Title = "Pulp Fiction",
        Description = "Too short",
        DirectorId = Guid.Parse("D28888E9-2BA9-473A-A40F-E38CB54F9B35"),
        ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
        Genre = "Crime, Drama"
      };

      var serializedMovieForCreation = JsonConvert.SerializeObject(moviesForCreation);

      using (var request = new HttpRequestMessage(HttpMethod.Post, "api/movies"))
      {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        request.Content = new StringContent(serializedMovieForCreation);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
          if (!response.IsSuccessStatusCode)
          {
            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
              var errorStream = await response.Content.ReadAsStreamAsync();
              var validationErrors = errorStream.ReadAndDeserializeFromJson();
              Console.WriteLine(validationErrors);
              return;
            }
            else
            {
              response.EnsureSuccessStatusCode();
            }
          }

          var stream = await response.Content.ReadAsStreamAsync();
          var movie = stream.ReadAndDeserializeFromJson<Movie>();
        }

      }

    }


  }
}
