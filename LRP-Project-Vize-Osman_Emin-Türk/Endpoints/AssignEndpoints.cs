using LRP_Project.Data;
using LRP_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace LRP_Project.Endpoints;

public static class AssignEndpoints
{
    public static void MapAssignEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. SORUMLULUK ATAMA (DÜZELTİLDİ)
        app.MapPost("/api/assign", async (AssignRequest req, AppDbContext db) => {
            var pc = await db.Computers.FindAsync(req.pcId);
            if (pc == null) return Results.BadRequest("Bilgisayar bulunamadı!");

            // KRİTİK DÜZELTME: Her seferinde 'new User' oluşturmak yerine önce veritabanında var mı kontrol ediyoruz
            var existingUser = await db.Users
                .FirstOrDefaultAsync(u => u.Username == req.studentNo);

            int targetUserId;

            if (existingUser != null)
            {
                // Kullanıcı zaten varsa, mevcut kullanıcının ID'sini alıyoruz
                targetUserId = existingUser.Id;
            }
            else
            {
                // Kullanıcı yoksa, ilk kez oluşturuyoruz
                var newUser = new LRP_Project.Models.User
                {
                    FullName = req.fullName,
                    StudentNumber = req.studentNo,
                    Username = req.studentNo,
                    Password = "123",
                    Role = "Student"
                };

                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                targetUserId = newUser.Id;
            }

            // Bilgisayarı hedef kullanıcıya bağlıyoruz
            pc.AssignedUserId = targetUserId;
            await db.SaveChangesAsync();

            return Results.Ok(new { message = $"Atama başarılı! Öğrenci: {req.studentNo}" });
        });

        // 2. ATAMA SİLME (DÜZELTİLDİ)
        app.MapDelete("/api/assign/{id}", async (int id, AppDbContext db) => {
            var pc = await db.Computers
                .Include(c => c.AssignedUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (pc == null) return Results.NotFound("Bilgisayar bulunamadı.");

            if (pc.AssignedUserId != null)
            {
                // DÜZELTME: Kullanıcıyı veritabanından SİLMİYORUZ. 
                // Çünkü bu öğrencinin üzerine kayıtlı BAŞKA bilgisayarlar da olabilir.
                // Sadece bilgisayar ile kullanıcı arasındaki bağı koparıyoruz.
                pc.AssignedUserId = null;
            }

            try
            {
                await db.SaveChangesAsync();
                return Results.Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Silme Hatası: " + ex.Message);
                return Results.Problem("Veritabanı hatası: " + ex.Message);
            }
        });
    }
}

public record AssignRequest(int pcId, string studentNo, string fullName);