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
  public class CancellationService : IIntegrationService
  {
    private static HttpClient _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip });
    private CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();
    public CancellationService()
    {
      _httpClient.BaseAddress = new Uri("http://localhost:57863");
      _httpClient.Timeout = new TimeSpan(0, 0, 5);
      _httpClient.DefaultRequestHeaders.Clear();
    }

    public async Task Run()
    {
      //_cancelationTokenSource.CancelAfter(10000);
      //await GetTrailerAndCancel(_cancelationTokenSource.Token);
      await GetTrailerAndHandleTimout();
    }


    private async Task GetTrailerAndCancel(CancellationToken cancelationToken)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/26fcbcc4-b7f7-47fc-9382-740c12246b59/trailers/{Guid.NewGuid()}");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

      //var cancelationTokenSource = new CancellationTokenSource();
      //cancelationTokenSource.CancelAfter(2000);

      try
      {
        using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelationToken))
        {
          var stream = await response.Content.ReadAsStreamAsync();
          response.EnsureSuccessStatusCode();

          var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
        }
      }
      catch (OperationCanceledException ex)
      {
        Console.WriteLine($"An operation was cancelled with message: \"{ex.Message}\"");
      }
    }

    private async Task GetTrailerAndHandleTimout()
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/26fcbcc4-b7f7-47fc-9382-740c12246b59/trailers/{Guid.NewGuid()}");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

      try
      {
        using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
        {
          var stream = await response.Content.ReadAsStreamAsync();
          response.EnsureSuccessStatusCode();

          var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
        }
      }
      catch (OperationCanceledException ex)
      {
        Console.WriteLine($"An operation was cancelled with message: \"{ex.Message}\"");
      }
    }
  }
}
