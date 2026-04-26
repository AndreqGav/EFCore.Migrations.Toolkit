using System;
using System.Linq;
using EFCore.Migrations.AutoComments.Helpers;
using Xunit;

namespace EFCore.Migrations.Toolkit.Tests.UnitTests.AutoComments;

/// <summary>
/// Юнит-тесты <see cref="TypeHelper"/>.
/// </summary>
public class TypeHelperTests
{
    [Fact]
    public void GetParentTypes_Should_ReturnEmpty_WhenTypeIsNull()
    {
        // Arrange
        Type type = null;

        // Act
        var result = TypeHelper.GetParentTypes(type);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetParentTypes_Should_ReturnOnlySelf_WhenTypeIsRoot()
    {
        // Arrange
        var type = typeof(ParentClass);

        // Act
        var result = TypeHelper.GetParentTypes(type).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(typeof(ParentClass), result[0]);
    }

    [Fact]
    public void GetParentTypes_Should_ReturnEmpty_ForValueType()
    {
        // Arrange
        var type = typeof(int);

        // Act
        var result = TypeHelper.GetParentTypes(type).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetParentTypes_Should_IncludeParentClassAndInterface_ForDerivedClass()
    {
        // Arrange
        var type = typeof(ChildClass);

        // Act
        var result = TypeHelper.GetParentTypes(type).ToList();

        // Assert
        Assert.Contains(typeof(ChildClass), result);
        Assert.Contains(typeof(ParentClass), result);
        Assert.Contains(typeof(IMarker), result);
    }

    [Fact]
    public void GetParentTypes_Should_ReturnSelfAsFirst()
    {
        // Arrange
        var type = typeof(ChildClass);

        // Act
        var result = TypeHelper.GetParentTypes(type).ToList();

        // Assert
        Assert.Equal(typeof(ChildClass), result[0]);
    }

    [Fact]
    public void GetParentTypes_Should_ReturnParentBeforeGrandparent()
    {
        // Arrange
        var type = typeof(GrandChildClass);

        // Act
        var result = TypeHelper.GetParentTypes(type).ToList();

        // Assert
        var childIdx = result.IndexOf(typeof(ChildClass));
        var parentIdx = result.IndexOf(typeof(ParentClass));
        Assert.True(childIdx < parentIdx);
    }

    private interface IMarker
    {
    }

    private class ParentClass
    {
    }

    private class ChildClass : ParentClass, IMarker
    {
    }

    private class GrandChildClass : ChildClass
    {
    }
}