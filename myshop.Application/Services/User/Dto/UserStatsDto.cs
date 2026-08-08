namespace myshop.Application.Services.User.Dto
{
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveCount { get; set; }
        public int LockedCount { get; set; }
        public int AdminsCount { get; set; }
    }
}
