
using LPGxWebApi.Controllers;
using LPGxWebApi.Model;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ProjectInfos> PROJECTINFO { get; set; }
    public DbSet<BranchInfos> BRANCHINFO { get; set; }
    public DbSet<LevelInfos> LEVELINFO { get; set; }
    public DbSet<AuthenticatorInfos> AUTHENTICATORS { get; set; }
    public DbSet<LoginInfos> LOGIN { get; set; }
    public DbSet<GenerateOTP> GENERATEOTP { get; set; }
    public DbSet<UserMapping> USERMAPPING { get; set; }

    public DbSet<ProductInfos> PRODUCTSTAB { get; set; }
    public DbSet<SalesManInfos> SALESMANTAB { get; set; }
    public DbSet<SaleInfos> SALEENTRYTAB { get; set; }
    public DbSet<ReturnInfos> RETURNENTRYTAB { get; set; }


	//FOR IVAC
	public DbSet<IVACInfos> IVACINFO { get; set; }
	

	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenerateOTP>()
            .Property(g => g.REQID)
            .ValueGeneratedOnAdd(); // Ensure EF knows REQID is an identity column

        modelBuilder.Entity<UserMapping>()
            .Property(c => c.MAPPINGID)
            .ValueGeneratedOnAdd(); // This tells EF that MAPPINGID is an identity column

        modelBuilder.Entity<LoginInfos>()
            .Property(c => c.USERCODE)
            .ValueGeneratedOnAdd(); // This tells EF that USERCODE is an identity column

    }


}
