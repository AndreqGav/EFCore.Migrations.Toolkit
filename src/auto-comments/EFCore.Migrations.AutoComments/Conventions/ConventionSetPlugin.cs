using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace EFCore.Migrations.AutoComments.Conventions;

public class ConventionSetPlugin : IConventionSetPlugin
{
    private readonly AutoCommentsExtension _extension;

    public ConventionSetPlugin([NotNull] IDbContextOptions options)
    {
        _extension = options.FindExtension<AutoCommentsExtension>()!;
    }

    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        var enumAnnotationConvention = new AutoCommentEnumDescriptionConvention(_extension.Options.AutoCommentEnumDescriptions);
        conventionSet.ModelFinalizingConventions.Add(enumAnnotationConvention);

        var autoCommentsConvention = new AutoCommentsConvention(_extension.Options);
        conventionSet.ModelFinalizingConventions.Add(autoCommentsConvention);

        return conventionSet;
    }
}