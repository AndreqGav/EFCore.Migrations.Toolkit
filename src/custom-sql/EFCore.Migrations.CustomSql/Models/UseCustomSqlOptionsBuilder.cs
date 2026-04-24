using Microsoft.EntityFrameworkCore;

namespace EFCore.Migrations.CustomSql.Models
{
    public class UseCustomSqlOptions
    {
        public DbContextOptionsBuilder OptionsBuilder { get; }

        public UseCustomSqlOptions(DbContextOptionsBuilder optionsBuilder)
        {
            OptionsBuilder = optionsBuilder;
        }
    }
}