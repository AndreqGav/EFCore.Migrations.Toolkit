using EFCore.Migrations.CustomSql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Migrations.CustomSql
{
    public static class AddCustomSqlExtensions
    {
        public static ModelBuilder AddCustomSql(this ModelBuilder modelBuilder, string name, string sqlUp, string sqlDown)
        {
            var sqlUpModel = new SqlUpModel(name, sqlUp);
            var sqlDownModel = new SqlDownModel(name, sqlDown);
            
            modelBuilder.HasAnnotation(sqlUpModel.Annotation, sqlUpModel.Sql);
            modelBuilder.HasAnnotation(sqlDownModel.Annotation, sqlDownModel.Sql);

            return modelBuilder;
        }

        public static EntityTypeBuilder<TEntity> AddCustomSql<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, string name,
            string sqlUp, string sqlDown)
            where TEntity : class
        {
            var sqlUpModel = new SqlUpModel(name, sqlUp);
            var sqlDownModel = new SqlDownModel(name, sqlDown);

            entityTypeBuilder.HasAnnotation(sqlUpModel.Annotation, sqlUpModel.Sql);
            entityTypeBuilder.HasAnnotation(sqlDownModel.Annotation, sqlDownModel.Sql);

            return entityTypeBuilder;
        }

        public static IConventionEntityTypeBuilder AddCustomSql(this IConventionEntityTypeBuilder entityTypeBuilder, string name,
            string sqlUp, string sqlDown)
        {
            var sqlUpModel = new SqlUpModel(name, sqlUp);
            var sqlDownModel = new SqlDownModel(name, sqlDown);

            entityTypeBuilder.HasAnnotation(sqlUpModel.Annotation, sqlUpModel.Sql);
            entityTypeBuilder.HasAnnotation(sqlDownModel.Annotation, sqlDownModel.Sql);

            return entityTypeBuilder;
        }
    }
}