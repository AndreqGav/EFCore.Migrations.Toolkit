using EFCore.Migrations.CustomSql.Constants;
using EFCore.Migrations.CustomSql.Helpers;

namespace EFCore.Migrations.CustomSql.Models
{
    public abstract class SqlAnnotationModel
    {
        public string Annotation { get; }

        public string Sql { get; }

        protected SqlAnnotationModel(string annotation, string sql)
        {
            Annotation = annotation.NormalizeLineEndings();
            Sql = sql.NormalizeLineEndings();
        }
    }

    public class SqlUpModel : SqlAnnotationModel
    {
        public SqlUpModel(string name, string sql) : base($"{CustomSqlConstants.SqlUp}{name}", sql)
        {
        }
    }

    public class SqlDownModel : SqlAnnotationModel
    {
        public SqlDownModel(string name, string sql) : base($"{CustomSqlConstants.SqlDown}{name}", sql)
        {
        }
    }

    public class CustomSqlModels
    {
        public string Name { get; }

        public string SqlUp { get; }

        public string SqlDown { get; }

        public CustomSqlModels(string name, string sqlUp, string sqlDown)
        {
            Name = name;
            SqlUp = sqlUp;
            SqlDown = sqlDown;
        }
    }
}