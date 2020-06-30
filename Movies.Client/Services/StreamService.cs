using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Marvin.StreamExtensions;

namespace Movies.Client.Services
{
  public class StreamService : IIntegrationService
  {
    private static HttpClient _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip });

    public StreamService()
    {
      _httpClient.BaseAddress = new Uri("http://localhost:57863");
      _httpClient.Timeout = new TimeSpan(0, 0, 30);
      _httpClient.DefaultRequestHeaders.Clear();
    }

    public async Task Run()
    {
       await GetPosterWithStream();
      // await GetPosterWithStreamAndCompletionMode();
      // await TestGetPosterWithoutStream();
      // await TestGetPosterWithStream();      
      // await TestGetPosterWithStreamAndCompletionMode();
      // await PostPosterWithStream();
      // await PostAndReadPosterWithStream();
      // await TestPostPosterWithoutStream();
      // await TestPostPosterWithStream();
      // await TestPostAndReadPosterWithStreams();
      // await GetPosterWithGZipCompression();
    }

    public async Task GetPosterWithStream()
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/26fcbcc4-b7f7-47fc-9382-740c12246b59/posters/{Guid.NewGuid()}");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      using (var response = await _httpClient.SendAsync(request))
      {
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var poster = stream.ReadAndDeserializeFromJson<Poster>();
      }

    }

    public async Task GetPosterWithOutStream()
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/26fcbcc4-b7f7-47fc-9382-740c12246b59/posters/{Guid.NewGuid()}");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var posters = JsonConvert.DeserializeObject<Poster>(content);

    }

    public async Task GetPosterWithStreamAndCompletionMode()
    {
      var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/26fcbcc4-b7f7-47fc-9382-740c12246b59/posters/{Guid.NewGuid()}");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
      {
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();

        var poster = stream.ReadAndDeserializeFromJson<Poster>();
      }

    }

    private async Task PostPosterWithoutStream()
    {
      // generate a movie poster of 500KB
      var random = new Random();
      var generatedBytes = new byte[524288];
      random.NextBytes(generatedBytes);

      var posterForCreation = new PosterForCreation()
      {
        Name = "A new poster for The Big Lebowski",
        Bytes = generatedBytes
      };

      var serializedPosterForCreation = JsonConvert.SerializeObject(posterForCreation);

      var request = new HttpRequestMessage(HttpMethod.Post,
          "api/movies/da2fd609-d754-4feb-8acd-c4f9ff13ba96/posters");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      request.Content = new StringContent(serializedPosterForCreation);
      request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var createdMovie = JsonConvert.DeserializeObject<Poster>(content);
    }
    public async Task PostPosterWithStream()
    {
      var random = new Random();
      var generateBytes = new byte[524288];
      random.NextBytes(generateBytes);

      var posterForCreation = new PosterForCreation
      {
        Name = "A new poster for The Big Lebowski",
        Bytes = generateBytes
      };

      var memoryContentStream = new MemoryStream();
      memoryContentStream.SerializeToJsonAndWrite(posterForCreation, new UTF8Encoding(), 1024, true);

      memoryContentStream.Seek(0, SeekOrigin.Begin);
      using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/movies/da2fd609-d754-4feb-8acd-c4f9ff13ba96/posters"))
      {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using (var streamContent = new StreamContent(memoryContentStream))
        {
          request.Content = streamContent;
          request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

          var response = await _httpClient.SendAsync(request);

          response.EnsureSuccessStatusCode();

          var content = await response.Content.ReadAsStringAsync();
          var createrdPoster = JsonConvert.DeserializeObject<Poster>(content);
        }
      }

    }

    public async Task PostAndReadPosterWithStream()
    {
      var random = new Random();
      var generateBytes = new byte[524288];
      random.NextBytes(generateBytes);

      var posterForCreation = new PosterForCreation
      {
        Name = "A new poster for The Big Lebowski",
        Bytes = generateBytes
      };

      var memoryContentStream = new MemoryStream();
      memoryContentStream.SerializeToJsonAndWrite(posterForCreation, new UTF8Encoding(), 1024, true);

      memoryContentStream.Seek(0, SeekOrigin.Begin);
      using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/movies/da2fd609-d754-4feb-8acd-c4f9ff13ba96/posters"))
      {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using (var streamContent = new StreamContent(memoryContentStream))
        {
          request.Content = streamContent;
          request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

          using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
          {
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var poster = stream.ReadAndDeserializeFromJson<Poster>();
          }
        }
      }

    }

    private async Task GetPosterWithGZipCompression()
    {      
      var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/26fcbcc4-b7f7-47fc-9382-740c12246b59/posters/{Guid.NewGuid()}");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

      using (var response = await _httpClient.SendAsync(request))
      {
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var poster = stream.ReadAndDeserializeFromJson<Poster>();
      }
    }


    // Get
    private async Task TestGetPosterWithoutStream()
    {
      // warmup
      await GetPosterWithOutStream();

      var stopWatch = Stopwatch.StartNew();

      for (int i = 0; i < 200; i++)
      {
        await GetPosterWithOutStream();
      }

      stopWatch.Stop();

      Console.WriteLine("GetPosterWithoutStream");
      Console.WriteLine($"Elapsed milliseconds without stream: " +
        $"{stopWatch.ElapsedMilliseconds}, " +
        $"avaraging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
      Console.WriteLine();
    }

    public async Task TestGetPosterWithStream()
    {
      // warmup
      await GetPosterWithStream();

      var stopWatch = Stopwatch.StartNew();

      for (int i = 0; i < 200; i++)
      {
        await GetPosterWithStream();
      }

      stopWatch.Stop();

      Console.WriteLine("GetPosterWithStream");
      Console.WriteLine($"Elapsed milliseconds stream: " +
        $"{stopWatch.ElapsedMilliseconds}, " +
        $"avaraging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
      Console.WriteLine();

    }

    public async Task TestGetPosterWithStreamAndCompletionMode()
    {
      // warmup
      await GetPosterWithStreamAndCompletionMode();

      var stopWatch = Stopwatch.StartNew();

      for (int i = 0; i < 200; i++)
      {
        await GetPosterWithStreamAndCompletionMode();
      }

      stopWatch.Stop();

      Console.WriteLine("GetPosterWithStreamAndCompletionMode");
      Console.WriteLine($"Elapsed milliseconds with stream and completionmode: " +
        $"{stopWatch.ElapsedMilliseconds}, " +
        $"avaraging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
      Console.WriteLine();

    }


    // Post
    private async Task TestPostPosterWithoutStream()
    {
      // warmup
      await PostPosterWithoutStream();

      // start stopwatch 
      var stopWatch = Stopwatch.StartNew();

      // run requests
      for (int i = 0; i < 200; i++)
      {
        await PostPosterWithoutStream();
      }

      // stop stopwatch
      stopWatch.Stop();
      Console.WriteLine($"Elapsed milliseconds without stream: " +
          $"{stopWatch.ElapsedMilliseconds}, " +
          $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
    }

    private async Task TestPostPosterWithStream()
    {
      // warmup
      await PostPosterWithStream();

      // start stopwatch 
      var stopWatch = Stopwatch.StartNew();

      // run requests
      for (int i = 0; i < 200; i++)
      {
        await PostPosterWithStream();
      }

      // stop stopwatch
      stopWatch.Stop();
      Console.WriteLine($"Elapsed milliseconds with stream: " +
          $"{stopWatch.ElapsedMilliseconds}, " +
          $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
    }

    private async Task TestPostAndReadPosterWithStreams()
    {
      // warmup
      await PostAndReadPosterWithStream();

      // start stopwatch 
      var stopWatch = Stopwatch.StartNew();

      // run requests
      for (int i = 0; i < 200; i++)
      {
        await PostAndReadPosterWithStream();
      }

      // stop stopwatch
      stopWatch.Stop();
      Console.WriteLine($"Elapsed milliseconds with stream on post and read: " +
          $"{stopWatch.ElapsedMilliseconds}, " +
          $"averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
    }

  }
}
