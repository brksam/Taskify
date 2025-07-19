public class Team {
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<UserTeam> Members { get; set; } = new();
}