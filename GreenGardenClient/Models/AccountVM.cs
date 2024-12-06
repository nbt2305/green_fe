namespace GreenGardenClient.Models
{
    public class AccountVM
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }

        public AccountVM(string CloudName, string ApiKey, string ApiSecret)
        {
            this.CloudName = CloudName;
            this.ApiKey = ApiKey;
            this.ApiSecret = ApiSecret;
        }
        public AccountVM()
        {

        }
    }
}
