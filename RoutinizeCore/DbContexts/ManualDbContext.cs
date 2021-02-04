using System;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using RoutinizeCore.Models;

namespace RoutinizeCore.DbContexts {

    public partial class RoutinizeDbContext {

        private IConfiguration Configuration { get; }

        public RoutinizeDbContext(IConfiguration configuration) {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("RoutinizeDbDev"));
        }

        // partial void OnModelCreatingPartial(ModelBuilder modelBuilder) {
        //     var permissionEnumConversion = new ValueConverter<SharedEnums.Permissions, byte>(
        //         v => (byte) v,
        //         v => (SharedEnums.Permissions)Enum.Parse(typeof(SharedEnums.Permissions), v.ToString())
        //     );
        //     
        //     modelBuilder.Entity<CollaboratorTask>()
        //                 .Property(e => e.Permission)
        //                 .HasConversion(permissionEnumConversion);
        // }
    }
}