namespace api.Models
{
    public class OtpFileObject
    {
        public Guid Uuid { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expire { get; set; }
        public string? RandomString { get; set; }
        public DateTime? LastAccess { get; set; }
        public string LastIpOfRequest { get; set; }
        public bool OnTokenDeleteMongodbUser { get; set; }
        public string Username { get; set; }

        public OtpFileObject(Guid uuid, DateTime created, string? randomString, string ipOfRequest, bool onTokenDeleteMongodbUser, string username)
        {
            Uuid = uuid;
            Created = created;
            RandomString = randomString;
            LastAccess = DateTime.Now;
            LastIpOfRequest = ipOfRequest;
            OnTokenDeleteMongodbUser = onTokenDeleteMongodbUser;
            Username = username;
        }
    }
}
