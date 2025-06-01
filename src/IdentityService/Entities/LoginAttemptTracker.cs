namespace IdentityService.Entities
{
    public class LoginAttemptTracker
    {
        public int Attempts { get; set; }
        public DateTime LastAttempt { get; set; }
        public DateTime? BlockUntil { get; set; }
    }
}
