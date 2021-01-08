using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
    }
}