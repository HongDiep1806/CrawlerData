using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrawlerData
{
    public class Product
    {
        public long id { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public int original_price { get; set; }
        public int price { get; set; }
        public string brand_name { get; set; }
        public string seller_name { get; set; }
        public List<Badge> badges_new { get; set; }
    }

    public class Badge
    {
        public string placement { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string text { get; set; }
        public string? text_color { get; set; }
    }

    public class APIResponse
    {
        public List<Product> data { get; set; }
    }

    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static readonly string ApiEndpoint = "https://tiki.vn/api/v2/products?limit=40&include=advertisement&aggregations=2&trackity_id=b60cd8c8-8bba-18d3-6cca-f7f2b515da8e&q=ao";

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

        static async Task<APIResponse> GetProductsAsync(string path)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(path);

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

        static async Task Main()
        {
            client.BaseAddress = new Uri("https://tiki.vn/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                APIResponse response = await GetProductsAsync(ApiEndpoint);
                ShowProducts(response.data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
