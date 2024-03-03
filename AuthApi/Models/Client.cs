namespace AuthApi.Models
{
    public class Client
    {
        public Guid Guid { get; set; }
        public DateTime TokenExpirationDate { get; set; } = DateTime.MinValue;
    }
}
