using System.ComponentModel.DataAnnotations.Schema;

namespace LRP_Project.Models;

public class Computer
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Processor { get; set; } = string.Empty;
    public string RAM { get; set; } = string.Empty;
    public bool HasHDMI { get; set; }
    public bool HasVeyon { get; set; }

    // İlişkiler
    public int LabId { get; set; }

    // Veritabanında saklanan ID
    public int? AssignedUserId { get; set; }

    // Bağlı olan Kullanıcı Nesnesi
    [ForeignKey("AssignedUserId")]
    public User? AssignedUser { get; set; }
}