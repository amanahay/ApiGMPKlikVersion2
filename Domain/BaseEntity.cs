using ApiGMPKlik.Interfaces.Repositories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

// ============================================================================
// INTERFACES
// ============================================================================

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTime? ModifiedAt { get; set; }  // ← Perhatikan: ModifiedAt (bukan UpdatedAt)
    string? ModifiedBy { get; set; }     // ← Perhatikan: ModifiedBy (bukan UpdatedBy)

    void MarkAsCreated(string userId);
    void MarkAsUpdated(string userId);
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string? DeletedBy { get; set; }

    void SoftDelete(string userId);
    void Restore(string userId);
    bool IsValidForOperation();
}

public interface IActivatable
{
    bool IsActive { get; set; }

    void Activate(string userId);
    void Deactivate(string userId);
}

public interface IMetadataEntity
{
    string? Metadata { get; set; }

    void SetMetadata<T>(T metadata) where T : class;
    T? GetMetadata<T>() where T : class;
    bool HasMetadata();
    void ClearMetadata();
}

public interface IVersionable
{
    byte[]? RowVersion { get; set; }
}

public interface INamedEntity
{
    string Name { get; set; }
    string? Code { get; set; }
    string? Description { get; set; }
    int SortOrder { get; set; }

    void GenerateCodeIfEmpty(string prefix = "");
    void NormalizeName();
}

public interface IGeoLocatable
{
    decimal? Latitude { get; set; }
    decimal? Longitude { get; set; }
    string? Address { get; set; }
    string? PostalCode { get; set; }
    string? TimeZone { get; set; }

    bool HasValidCoordinates();
    void SetCoordinates(decimal latitude, decimal longitude);
    double? CalculateDistanceTo(decimal targetLatitude, decimal targetLongitude);
}

// ============================================================================
// BASE ENTITY
// ============================================================================

public abstract class BaseEntity :
    IAuditable,
    ISoftDeletable,
    IActivatable,
    IMetadataEntity,
    IVersionable,
    IEquatable<BaseEntity>,
    IEntity<int>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // Audit Properties
    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    [StringLength(450)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ModifiedAt { get; set; }  // ← Sesuai interface IAuditable

    [StringLength(450)]
    public string? ModifiedBy { get; set; }     // ← Sesuai interface IAuditable

    // Soft Delete Properties
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    [StringLength(450)]
    public string? DeletedBy { get; set; }

    // Status Properties
    public bool IsActive { get; set; } = true;

    // Versioning
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Metadata
    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; }

    // JSON Options
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Audit Methods
    public virtual void MarkAsCreated(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
        IsDeleted = false;
        IsActive = true;
    }

    public virtual void MarkAsUpdated(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

        ModifiedAt = DateTime.UtcNow;  // ← Sesuai interface
        ModifiedBy = userId;           // ← Sesuai interface
    }

    // Soft Delete Methods
    public virtual void SoftDelete(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
        IsActive = false;

        MarkAsUpdated(userId);
    }

    public virtual void Restore(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        IsActive = true;

        MarkAsUpdated(userId);
    }

    public virtual bool IsValidForOperation() => IsActive && !IsDeleted;

    // Activation Methods
    public virtual void Activate(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

        if (IsDeleted)
            throw new InvalidOperationException("Cannot activate deleted entity. Restore it first.");

        IsActive = true;
        MarkAsUpdated(userId);
    }

    public virtual void Deactivate(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or whitespace", nameof(userId));

        IsActive = false;
        MarkAsUpdated(userId);
    }

    // Metadata Methods
    public virtual void SetMetadata<T>(T metadata) where T : class
    {
        if (metadata is null)
        {
            Metadata = null;
            return;
        }

        try
        {
            Metadata = JsonSerializer.Serialize(metadata, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to serialize metadata of type {typeof(T).Name}", ex);
        }
    }

    public virtual T? GetMetadata<T>() where T : class
    {
        if (string.IsNullOrWhiteSpace(Metadata))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(Metadata, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public virtual bool HasMetadata() => !string.IsNullOrWhiteSpace(Metadata);

    public virtual void ClearMetadata() => Metadata = null;

    // Equality
    public virtual bool Equals(BaseEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        if (Id == 0 && other.Id == 0)
            return ReferenceEquals(this, other);

        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as BaseEntity);

    public override int GetHashCode()
    {
        if (Id == 0)
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);

    public override string ToString() =>
        $"{GetType().Name} [Id: {Id}, Active: {IsActive}, Deleted: {IsDeleted}]";
}

// ============================================================================
// NAMED ENTITY
// ============================================================================

public abstract class NamedEntity : BaseEntity, INamedEntity
{
    [Required]
    [StringLength(255)]
    public virtual string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public virtual string? Code { get; set; }

    [StringLength(1000)]
    public virtual string? Description { get; set; }

    public virtual int SortOrder { get; set; } = 0;

    public virtual void GenerateCodeIfEmpty(string prefix = "")
    {
        if (!string.IsNullOrEmpty(Code)) return;

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomSuffix = Random.Shared.Next(100, 999);
        Code = $"{prefix}{timestamp}{randomSuffix}".ToUpperInvariant();
    }

    public virtual void NormalizeName()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        Name = Name.Trim();
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        Name = textInfo.ToTitleCase(Name.ToLowerInvariant());
    }

    public override string ToString() =>
        $"{base.ToString()}, Name: '{Name}', Code: '{Code}'";
}

// ============================================================================
// GEO ENTITY
// ============================================================================

public abstract class GeoEntity : NamedEntity, IGeoLocatable
{
    private const double EarthRadiusKm = 6371.0;

    [Column(TypeName = "decimal(10,8)")]
    public virtual decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(11,8)")]
    public virtual decimal? Longitude { get; set; }

    [StringLength(500)]
    public virtual string? Address { get; set; }

    [StringLength(20)]
    public virtual string? PostalCode { get; set; }

    [StringLength(50)]
    public virtual string? TimeZone { get; set; }

    public virtual bool HasValidCoordinates() =>
        Latitude is >= -90 and <= 90 &&
        Longitude is >= -180 and <= 180;

    public virtual void SetCoordinates(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Must be between -90 and 90");
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Must be between -180 and 180");

        Latitude = latitude;
        Longitude = longitude;
    }

    public virtual double? CalculateDistanceTo(decimal targetLatitude, decimal targetLongitude)
    {
        if (!HasValidCoordinates()) return null;

        var dLat = ToRadians((double)(targetLatitude - Latitude!.Value));
        var dLon = ToRadians((double)(targetLongitude - Longitude!.Value));
        var lat1 = ToRadians((double)Latitude.Value);
        var lat2 = ToRadians((double)targetLatitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180);

    public virtual string GetCoordinatesString() =>
        HasValidCoordinates() ? $"{Latitude:F6}, {Longitude:F6}" : "Not set";

    public override string ToString() =>
        $"{base.ToString()}, Coordinates: [{GetCoordinatesString()}]";
}