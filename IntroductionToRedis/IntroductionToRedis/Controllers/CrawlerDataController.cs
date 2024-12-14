using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace IntroductionToRedis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrawlerData : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private readonly HttpClient _client;

        static readonly string ApiEndpoint = "https://tiki.vn/api/v2/products?limit=40&include=advertisement&aggregations=2&trackity_id=b60cd8c8-8bba-18d3-6cca-f7f2b515da8e&q=ao";

        public CrawlerData(IDistributedCache cache, IHttpClientFactory httpClientFactory)
        {
            _cache = cache;

            // Get a configured HttpClient from the factory
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://tiki.vn/");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        static void ShowProducts(List<Product> products)
        {
            if (products == null || products.Count == 0)
            {
                Console.WriteLine("No products found.");
                return;
            }

            foreach (var product in products)
            {
                Console.WriteLine($"ID: {product.id}\tName: {product.name}\tSKU: {product.sku}\tPrice: {product.original_price}\tBrand: {product.brand_name}\tSeller: {product.seller_name}");

                if (product.badges_new != null && product.badges_new.Count > 0)
                {
                    Console.WriteLine("Badges:");
                    foreach (var badge in product.badges_new)
                    {
                        Console.WriteLine($"\tPlacement: {badge.placement}, Type: {badge.type}, Text: {badge.text}, Text Color: {badge.text_color}");
                    }
                }
                Console.WriteLine("*****************");
            }
        }

        async Task<APIResponse> GetProductsAsync(string path)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(path);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<APIResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                    }) ?? new APIResponse { data = new List<Product>() };
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return new APIResponse { data = new List<Product>() };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return new APIResponse { data = new List<Product>() };
            }
        }

        [HttpGet("Craw")]
        public async Task<IActionResult> Get()
        {
            string cacheKey = "ProductsCache";
            string cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                Console.WriteLine("Returning data from cache...");
                var cachedResponse = JsonSerializer.Deserialize<APIResponse>(cachedData);
                return Ok(cachedResponse?.data);
            }

            try
            {
                APIResponse response = await GetProductsAsync(ApiEndpoint);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                };
                var serializedData = JsonSerializer.Serialize(response);
                await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

                //ShowProducts(response.data);
                Console.WriteLine("Returning from tiki.vn");
                return Ok(response.data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return BadRequest(e);
            }
        }
    }
}
