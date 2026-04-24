using EFCore.Migrations.Toolkit.Sample;
using Microsoft.EntityFrameworkCore;

using var context = new SampleDbContext();

var sqlScript = context.Database.GenerateCreateScript();

Console.WriteLine(sqlScript);

context.Database.EnsureCreated();