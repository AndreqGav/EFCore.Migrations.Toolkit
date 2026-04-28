using EFCore.Migrations.Triggers.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace EFCore.Migrations.Triggers.Conventions;

public class TriggerSqlSetPlugin : IConventionSetPlugin
{
    private readonly ITriggerSqlGenerator _triggerSqlGenerator;

    public TriggerSqlSetPlugin(ITriggerSqlGenerator triggerSqlGenerator)
    {
        _triggerSqlGenerator = triggerSqlGenerator;
    }

    public ConventionSet ModifyConventions(ConventionSet conventionSet)
    {
        conventionSet.ModelFinalizingConventions.Add(new TriggerSqlConvention(_triggerSqlGenerator));

        return conventionSet;
    }
}