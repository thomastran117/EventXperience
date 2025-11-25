namespace backend.Common
{
    public class OAuthUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public OAuthUser(int Id, string Email, string Name)
        {
            this.Id = Id;
            this.Email = Email;
            this.Name = Name;
        }
    }
}