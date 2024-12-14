namespace IntroductionToRedis
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
}
