namespace MyManual.Models.User
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime JoinDate { get; set; }
    }
}