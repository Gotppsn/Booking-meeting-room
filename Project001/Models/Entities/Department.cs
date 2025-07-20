using System.ComponentModel.DataAnnotations;

namespace Project001.Models.Entities;

public class Department
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameTh { get; set; }
    
    [StringLength(100)]
    public string? NameJa { get; set; }
    
    [StringLength(7)]
    public string? ColorCode { get; set; } = "#2563EB";
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}