using Microsoft.EntityFrameworkCore;
using OrderService.Api.Models;

namespace OrderService.Api.Data;

public class OrderContext : DbContext
{
    public OrderContext(DbContextOptions<OrderContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<PaymentDetail> PaymentDetails { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public DbSet<ShippingDetail> ShippingDetails { get; set; }
  }
