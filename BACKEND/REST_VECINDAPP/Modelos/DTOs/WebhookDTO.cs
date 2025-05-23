namespace REST_VECINDAPP.Modelos.DTOs
{
    public class WebhookNotification
    {
        public string Type { get; set; }
        public string Action { get; set; }
        public WebhookData Data { get; set; }
    }

    public class WebhookData
    {
        public string Id { get; set; }
        public string Status { get; set; }
    }
} 