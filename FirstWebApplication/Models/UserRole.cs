namespace FirstWebApplication.Models;

/// <summary>
/// Representerer en brukerrolle i systemet (Admin eller User/Pilot).
/// Lagres i UserRoles tabellen i databasen.
/// </summary>
public class UserRole
{
    /// <summary>
    /// Unik ID for rollen (Primary Key).
    /// Datatype: smallint (16-bit integer).
    /// 1 = Admin, 2 = User/Pilot
    /// </summary>
    public short UserRoleId { get; set; }
    
    /// <summary>
    /// Navn på rollen (f.eks. "Admin" eller "User").
    /// Påkrevd felt, maks 30 tegn.
    /// </summary>
    public string Role { get; set; } = null!;
    
    /// <summary>
    /// Navigation property: Liste over alle brukere med denne rollen.
    /// Entity Framework bruker dette til å håndtere relasjonen mellom UserRole og User.
    /// One-to-Many: En rolle kan ha mange brukere.
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}