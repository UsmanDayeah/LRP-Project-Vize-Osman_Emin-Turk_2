using LRP_Project.Data;
using LRP_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace LRP_Project.Endpoints;

public static class ComputerEndpoints
{
    public static void MapComputerEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. LİSTELEME (İlişkili Kullanıcı ile)
        app.MapGet("/api/computers", async (AppDbContext db) =>
        {
            return await db.Computers
                .Include(c => c.AssignedUser)
                .ToListAsync();
        });

        // 2. EKLEME (Otomatik PC Kodu üretimi ile)
        app.MapPost("/api/computers", async (Computer pc, AppDbContext db) => {
            var lab = await db.Labs.FindAsync(pc.LabId);
            if (lab == null) return Results.BadRequest("Geçersiz Laboratuvar!");

            // Yeni PC Kodu Oluşturma
            var pcCount = await db.Computers.CountAsync(c => c.LabId == pc.LabId);
            string labCode = lab.Name.Replace(" ", "_").ToUpper();
            pc.AssetCode = $"{labCode}-PC-{(pcCount + 1):D2}";

            db.Computers.Add(pc);
            await db.SaveChangesAsync();
            return Results.Ok(pc);
        });

        // 3. GÜNCELLEME (Laboratuvar Değişiminde Kodu Yenileyen Versiyon)
        app.MapPut("/api/computers/{id}", async (int id, Computer updatedPc, AppDbContext db) => {
            var pc = await db.Computers.FindAsync(id);
            if (pc == null) return Results.NotFound("Bilgisayar bulunamadı.");

            // Laboratuvar değişti mi kontrolü
            if (pc.LabId != updatedPc.LabId)
            {
                var newLab = await db.Labs.FindAsync(updatedPc.LabId);
                if (newLab != null)
                {
                    // Yeni laboratuvara göre PC Kodu üret
                    int count = await db.Computers.CountAsync(c => c.LabId == updatedPc.LabId);
                    string labCode = newLab.Name.Replace(" ", "_").ToUpper();
                    pc.AssetCode = $"{labCode}-PC-{(count + 1):D2}";
                    pc.LabId = updatedPc.LabId;
                }
            }

            // Temel özellikleri güncelle
            pc.Brand = updatedPc.Brand;
            pc.Processor = updatedPc.Processor;
            pc.RAM = updatedPc.RAM;
            pc.HasHDMI = updatedPc.HasHDMI;
            pc.HasVeyon = updatedPc.HasVeyon;

            await db.SaveChangesAsync();
            return Results.Ok(pc);
        });

        // 4. SİLME (DELETE)
        app.MapDelete("/api/computers/{id}", async (int id, AppDbContext db) => {
            var pc = await db.Computers.FindAsync(id);
            if (pc == null) return Results.NotFound("Bilgisayar bulunamadı.");

            // İlişkili zimmet varsa temizle
            pc.AssignedUserId = null;

            db.Computers.Remove(pc);
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Bilgisayar kaydı silindi." });
        });

        // 5. ÖĞRENCİ ÖZEL GÖRÜNÜMÜ
        app.MapGet("/api/my-computer/{username}", async (string username, AppDbContext db) => {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return Results.NotFound();

            var pc = await db.Computers
                .Include(c => c.AssignedUser)
                .FirstOrDefaultAsync(c => c.AssignedUserId == user.Id);

            return pc != null ? Results.Ok(pc) : Results.NotFound(new { message = "Cihaz bulunamadı" });
        });
    }
}