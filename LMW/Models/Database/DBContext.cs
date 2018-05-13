using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using LMW.Models.Mobile;

namespace LMW.Models.Database
{
    public class DBContext : DbContext
    {
        public DBContext() : base("name=LMWConnectionString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>()
            //    .HasMany(r => r.Roles)
            //    .WithMany(u => u.Users)
            //    .Map(m => {
            //        m.ToTable("UserRoles");
            //        m.MapLeftKey("UserId");
            //        m.MapRightKey("RoleId");
            //    });

            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetails>().ToTable("OrderDetails");
            modelBuilder.Entity<Contact>().ToTable("Contacts");
            modelBuilder.Entity<AppVersion>().ToTable("AppVersion");
            modelBuilder.Entity<Message>().ToTable("Messages");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<CarServiceGroup>().ToTable("CarServiceGroup");
            modelBuilder.Entity<CarService>().ToTable("CarService");
            modelBuilder.Entity<CarServiceDetails>().ToTable("CarServiceDetails");
        }
        
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<AppVersion> AppVersion { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<CarServiceGroup> CarServiceGroups { get; set; }
        public DbSet<CarService> CarServices { get; set; }
        public DbSet<CarServiceDetails> CarServiceDetails { get; set; }
    }
}