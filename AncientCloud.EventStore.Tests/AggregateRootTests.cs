using System;

namespace AncientCloud.EventStore.Tests;

[TestClass]
public class AggregateRootTests
{
    [TestMethod]
    public void CanViewAggregateRootId()
    {
        DecoratedClass obj1 = new();

        Assert.IsNotNull(AggregateRoot.ExtractId(obj1));
        Assert.AreEqual(AggregateRoot.ExtractId(obj1), Guid.Parse("40c558bd-9b1d-43bb-9b7f-d8c6af0ad0d7"));
    }

    [TestMethod]
    public void NoAggregateRootThrowsException()
    {
        UndecoratedClass obj1 = new();

        Assert.Throws<AggregateRootNotFoundException>(() => AggregateRoot.ExtractId(obj1));
    }

    [AggregateRoot("40c558bd-9b1d-43bb-9b7f-d8c6af0ad0d7")]
    private class DecoratedClass { }

    private class UndecoratedClass { }
}
