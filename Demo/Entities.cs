namespace ApiGMPKlik.Demo
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

  
        // ============================================================================
        // SQL SERVER ENTITIES (Primary Database)
        // ============================================================================

        public class Product : NamedEntity
        {
            [StringLength(500)]
            public string? ShortDescription { get; set; }

            [Required]
            [Column(TypeName = "decimal(18,2)")]
            public decimal Price { get; set; }

            public int StockQuantity { get; set; }

            // Mapping Code (dari NamedEntity) ke SKU
            public string Sku
            {
                get => Code ?? string.Empty;
                set => Code = value;
            }

            public int CategoryId { get; set; }
            public virtual Category Category { get; set; } = null!;

            // Helper method untuk mengurangi stok
            public void ReduceStock(int quantity, string userId)
            {
                if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
                if (StockQuantity < quantity) throw new InvalidOperationException("Insufficient stock");

                StockQuantity -= quantity;
                MarkAsUpdated(userId);
            }
        }

        public class Category : NamedEntity
        {
            public virtual ICollection<Product> Products { get; set; } = new List<Product>();

            // Method untuk menambah produk dengan validasi
            public Product AddProduct(string name, decimal price, int stock, string userId)
            {
                var product = new Product
                {
                    Name = name,
                    Price = price,
                    StockQuantity = stock,
                    CategoryId = Id
                };

                product.MarkAsCreated(userId);
                product.GenerateCodeIfEmpty("SKU-");
                Products.Add(product);
                return product;
            }
        }

        public enum OrderStatus
        {
            Pending = 0,
            Processing = 1,
            Shipped = 2,
            Delivered = 3,
            Cancelled = 4
        }

        public class Order : BaseEntity
        {
            [Required]
            [StringLength(50)]
            public string OrderNumber { get; set; } = string.Empty;

            [StringLength(450)]
            public string UserId { get; set; } = string.Empty;

            [Required]
            [StringLength(200)]
            public string CustomerName { get; set; } = string.Empty;

            [Required]
            [StringLength(500)]
            public string ShippingAddress { get; set; } = string.Empty;

            [Column(TypeName = "decimal(18,2)")]
            public decimal TotalAmount { get; set; }

            public OrderStatus Status { get; set; } = OrderStatus.Pending;

            public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

            // Bisa ditambahkan OrderItems jika diperlukan nanti
            // public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

            // Business logic
            public void CalculateTotal(decimal amount)
            {
                TotalAmount = amount;
            }

            public void ChangeStatus(OrderStatus newStatus, string userId)
            {
                if (IsDeleted) throw new InvalidOperationException("Cannot modify deleted order");
                Status = newStatus;
                MarkAsUpdated(userId);
            }
        }

        // ============================================================================
        // SQLITE ENTITIES (Secondary Database - Cache & Logs)
        // ============================================================================

        public class LocalCache
        {
            [Key]
            [StringLength(500)]
            public string Key { get; set; } = string.Empty;

            public string? Value { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime ExpiryDate { get; set; }

            [StringLength(100)]
            public string? DataType { get; set; }

            public bool IsExpired => DateTime.UtcNow > ExpiryDate;
        }

        public class OfflineSyncQueue
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            [StringLength(100)]
            public string EntityType { get; set; } = string.Empty; // "Product", "Order", dll

            [Required]
            [StringLength(50)]
            public string Action { get; set; } = string.Empty; // "Create", "Update", "Delete"

            public string? Payload { get; set; } // JSON data

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime? ProcessedAt { get; set; }

            public bool IsProcessed { get; set; } = false;

            public int RetryCount { get; set; } = 0;

            public string? ErrorMessage { get; set; }

            public void MarkAsProcessed()
            {
                IsProcessed = true;
                ProcessedAt = DateTime.UtcNow;
            }

            public void MarkAsFailed(string error)
            {
                ErrorMessage = error;
                RetryCount++;
            }
        }

        public class AuditLog
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            [StringLength(100)]
            public string TableName { get; set; } = string.Empty;

            [Required]
            [StringLength(50)]
            public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

            public string? EntityId { get; set; } // ID dari entity yang diubah (int/string)

            public string? OldValues { get; set; } // JSON

            public string? NewValues { get; set; } // JSON

            public DateTime Timestamp { get; set; } = DateTime.UtcNow;

            [StringLength(450)]
            public string? UserId { get; set; }

            [StringLength(200)]
            public string? UserName { get; set; }

            [StringLength(50)]
            public string? IpAddress { get; set; }

            // Helper method untuk membuat log entry
            public static AuditLog Create(string tableName, string action, string entityId,
                string? oldValues = null, string? newValues = null,
                string? userId = null, string? userName = null)
            {
                return new AuditLog
                {
                    TableName = tableName,
                    Action = action,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    UserId = userId,
                    UserName = userName,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

}
