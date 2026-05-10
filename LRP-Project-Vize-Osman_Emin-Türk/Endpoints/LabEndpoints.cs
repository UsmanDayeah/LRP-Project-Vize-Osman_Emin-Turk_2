using LRP_Project.Data;
using LRP_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace LRP_Project.Endpoints;

public static class LabEndpoints
{
    public static void MapLabEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/labs", async (AppDbContext db) => await db.Labs.ToListAsync());

        app.MapPost("/api/labs", async (Lab lab, AppDbContext db) => {
            var existingIds = await db.Labs.Select(l => l.Id).ToListAsync();
            int newId = 1;
            while (existingIds.Contains(newId)) { newId++; }

            lab.Id = newId;
            db.Labs.Add(lab);
            await db.SaveChangesAsync();
            return Results.Created($"/api/labs/{lab.Id}", lab);
        });

        app.MapPut("/api/labs/{id}", async (int id, Lab updatedLab, AppDbContext db) => {
            var lab = await db.Labs.FindAsync(id);
            if (lab == null) return Results.NotFound();

            lab.Name = updatedLab.Name;
            lab.Location = updatedLab.Location;

            await db.SaveChangesAsync();
            return Results.Ok(lab);
        });

        app.MapDelete("/api/labs/{id}", async (int id, AppDbContext db) => {
            var lab = await db.Labs.FindAsync(id);
            if (lab == null) return Results.NotFound();

            db.Labs.Remove(lab);
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Laboratuvar silindi" });
        });
    }
}