using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;

#pragma warning disable EF1001

namespace EFCore.Migrations.Abstractions;

public class CompositeMigrationsModelDiffer : IMigrationsModelDiffer
{
    private readonly IMigrationsModelDiffer _baseDiffer;

    private readonly IReadOnlyList<IMigrationOperationModifier> _modifiers;

    public CompositeMigrationsModelDiffer(
        IRelationalTypeMappingSource typeMappingSource,
        IMigrationsAnnotationProvider migrationsAnnotationProvider,
        IRowIdentityMapFactory rowIdentityMapFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies,
        IEnumerable<IMigrationOperationModifier> modifiers)
    {
        _baseDiffer = new MigrationsModelDiffer(typeMappingSource, migrationsAnnotationProvider,
            rowIdentityMapFactory,
            commandBatchPreparerDependencies);

        _modifiers = modifiers.ToList();
    }

    public bool HasDifferences(IRelationalModel source, IRelationalModel target)
    {
        if (_baseDiffer.HasDifferences(source, target))
        {
            return true;
        }

        var operations = _baseDiffer.GetDifferences(source, target);

        foreach (var modifier in _modifiers)
        {
            operations = modifier.ModifyOperations(operations, source, target);
        }

        return operations.Any();
    }

    public IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel source, IRelationalModel target)
    {
        var operations = _baseDiffer.GetDifferences(source, target);

        foreach (var modifier in _modifiers)
        {
            operations = modifier.ModifyOperations(operations, source, target);
        }

        return operations;
    }
}
#pragma warning restore EF1001