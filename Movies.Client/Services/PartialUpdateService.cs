﻿using Microsoft.AspNetCore.JsonPatch;
using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
  public class PartialUpdateService : IIntegrationService
  {
    private static HttpClient _httpClient = new HttpClient();

    public PartialUpdateService()
    {
      _httpClient.BaseAddress = new Uri("http://localhost:57863");
      _httpClient.Timeout = new TimeSpan(0, 0, 30);
      _httpClient.DefaultRequestHeaders.Clear();
    }

    public async Task Run()
    {
      await PatchResource();
    }

    public async Task PatchResource()
    {
      var patchDoc = new JsonPatchDocument<MovieForUpdate>();

      patchDoc.Replace(m => m.Title, "Updated Title");
      patchDoc.Remove(m => m.Description);

      var serializedChangeSet = JsonConvert.SerializeObject(patchDoc);

      var request = new HttpRequestMessage(HttpMethod.Patch, "api/movies/bb6a100a-053f-4bf8-b271-60ce3aae6eb5");
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

      request.Content = new StringContent(serializedChangeSet);
      request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

      var response = await _httpClient.SendAsync(request);

      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var updatedMovie = JsonConvert.DeserializeObject<Movie>(content);

    }
  }
}
