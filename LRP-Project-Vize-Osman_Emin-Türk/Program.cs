using LRP_Project.Data;
using LRP_Project.Endpoints;
using Microsoft.EntityFrameworkCore;
using LRP_Project.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Servisini Kaydet (SQLite)
// Veritabanı dosya adının projeyle tutarlı olduğundan emin ol (lrp_system.db)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=lrp_system.db"));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// İlk Çalıştırmada Veritabanı ve Varsayılan Admin Kontrolü
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            FullName = "Sistem Yöneticisi",
            Username = "admin",
            Password = "123",
            Role = "Admin"
        });
        db.SaveChanges();
    }
}

// --- 2. MODÜLER ROTALAR ---
// Not: Eğer MapComputerEndpoints içinde "/api/my-pc" veya benzeri bir tanım varsa 
// aşağıdakilerle çakışabilir. Bu kod en temiz haliyle her şeyi ayırır.
app.MapAuthEndpoints();
app.MapLabEndpoints();
app.MapComputerEndpoints();
app.MapAssignEndpoints();

// --- 3. ÖĞRENCİ PANELİ: CİHAZ SORGULAMA ---
// 'my-pc' adresi hem Admin çakışmasını önler hem de net bir ayrım sağlar.
app.MapGet("/api/my-pc/{username}", async (string username, AppDbContext db) =>
{
    Console.WriteLine($"[LOG] Sorgulanan Öğrenci: {username}");

    // Kullanıcıyı bul
    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Username == username);

    if (user == null)
    {
        return Results.NotFound(new { message = "Kullanıcı bulunamadı." });
    }

    // ÇOKLU CİHAZ DESTEĞİ: .Where() ve .ToListAsync() kullanımı kritiktir.
    var computers = await db.Computers
        .Where(c => c.AssignedUserId == user.Id)
        .ToListAsync();

    if (computers == null || computers.Count == 0)
    {
        return Results.NotFound(new { message = "Üzerinize kayıtlı bir cihaz bulunamadı." });
    }

    // Konsola kaç cihaz döndüğünü yazdır (Debug için)
    Console.WriteLine($"[LOG] {username} için {computers.Count} adet cihaz başarıyla gönderildi.");

    return Results.Ok(computers);
});

// Sistem Durum Kontrolü
app.MapGet("/api/status", () => new {
    message = "LRP API Sistemi Aktif",
    status = "OK",
    time = DateTime.Now
});

// --- STATİK DOSYALAR VE ÇALIŞTIRMA ---
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

// --- TİP TANIMLAMALARI ---
public record AssignRequest(int pcId, string studentNo, string fullName);