﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MLFlow.NET.Lib.Contract;
using MLFlow.NET.Lib.Helpers;
using MLFlow.NET.Lib.Model;
using Newtonsoft.Json;

namespace MLFlow.NET.Lib.Services
{
    public class HttpService : IHttpService
    {
        private readonly IOptions<MLFlowConfiguration> _config;
        private readonly HttpClient _client;
        public HttpService(IOptions<MLFlowConfiguration> config)
        {
            _config = config;
            _client = new HttpClient { BaseAddress = new Uri(config.Value.MlFlowServerBaseUrl) };
        }

        string _serialise<T>(T request)
        {


            var content = JsonConvert.SerializeObject(request, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            return content;
        }



        public async Task<T> Post<T, Y>(string urlPart, Y request) where T : class where Y : class
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {

                var uri = _getUrl(urlPart);
                var content = new StringContent(_serialise(request));
                var response = await _client.PostAsync(uri, content);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(responseBody);
                return result;

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            return null;
        }

        public async Task<T> Get<T, Y>(string urlPart, Y request) where T : class where Y : class
        {
            try
            {
                var path = $"{_config.Value.MlFlowServerBaseUrl}/{_config.Value.APIBase}{urlPart}?{request.GetQueryString()}";
                var response = await _client.GetAsync(path);
                var result = default(T);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<T>(responseBody);
                }
                return result;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            return null;
        }
        private Uri _getUrl(string urlPart)
        {
            var baseUri = new Uri(new Uri(_config.Value.MlFlowServerBaseUrl), _config.Value.APIBase);
            var fullUri = new Uri(baseUri, urlPart);
            return fullUri;
        }
    }
}
