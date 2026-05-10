using LRP_Project.Data;
using LRP_Project.Models; // User modeli için
using Microsoft.EntityFrameworkCore;

namespace LRP_Project.Endpoints;

public static class AuthEndpoints
{
    // Program.cs'den çağrılacak genişletme metodu
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/login", async (LRP_Project.Models.User loginData, AppDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.Username == loginData.Username && u.Password == loginData.Password);

            if (user == null)
            {
                return Results.Json(new { success = false, message = "Kullanıcı adı veya şifre hatalı!" }, statusCode: 401);
            }
            return Results.Ok(new { success = true, username = user.Username, role = user.Role, fullName = user.FullName });
        });
    }
}