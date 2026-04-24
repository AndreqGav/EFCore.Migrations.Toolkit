using EFCore.Migrations.Triggers.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace EFCore.Migrations.Triggers.Conventions
{
    public class TriggerSqlSetPlugin : IConventionSetPlugin
    {
        private readonly ITriggerSqlGenerator _triggerSqlGenerator;

        private readonly RelationalConventionSetBuilderDependencies _dependencies;

        public TriggerSqlSetPlugin(ITriggerSqlGenerator triggerSqlGenerator, RelationalConventionSetBuilderDependencies dependencies)
        {
            _triggerSqlGenerator = triggerSqlGenerator;
            _dependencies = dependencies;
        }

        public ConventionSet ModifyConventions(ConventionSet conventionSet)
        {
            conventionSet.ModelFinalizingConventions.Add(new TriggerSqlConvention(_triggerSqlGenerator, _dependencies));

            return conventionSet;
        }
    }
}