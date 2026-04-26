using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;

#pragma warning disable EF1001

namespace EFCore.Migrations.Abstractions
{
    public class CompositeMigrationsModelDiffer
        : Microsoft.EntityFrameworkCore.Migrations.Internal.MigrationsModelDiffer
    {
        private readonly IReadOnlyList<IMigrationOperationModifier> _modifiers;

        public CompositeMigrationsModelDiffer(IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotations, IChangeDetector changeDetector,
            IUpdateAdapterFactory updateAdapterFactory,
            CommandBatchPreparerDependencies commandBatchPreparerDependencies,
            IEnumerable<IMigrationOperationModifier> modifiers)
            : base(typeMappingSource, migrationsAnnotations, changeDetector, updateAdapterFactory,
                commandBatchPreparerDependencies)
        {
            _modifiers = modifiers.ToList();
        }

        public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel source,
            IRelationalModel target)
        {
            var operations = base.GetDifferences(source, target);

            foreach (var modifier in _modifiers)
            {
                operations = modifier.ModifyOperations(operations, source, target);
            }

            return operations;
        }
    }
}
#pragma warning restore EF1001