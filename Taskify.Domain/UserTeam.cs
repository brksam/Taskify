public class UserTeam {
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = default!;
    public string Role { get; set; } = "Member"; // Admin, Member, Visitor
}