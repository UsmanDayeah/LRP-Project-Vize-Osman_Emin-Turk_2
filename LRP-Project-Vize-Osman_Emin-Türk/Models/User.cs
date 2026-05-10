namespace LRP_Project.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty; // Admin için boş olabilir
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // Admin veya Student
}