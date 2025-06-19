using System;
using System.Net.NetworkInformation;

namespace AncientCloud.EventStore;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class AggregateRootAttribute : Attribute
{
    private readonly Guid _id;

    public AggregateRootAttribute(string id)
    {
        this._id = Guid.Parse(id);
    }

    public Guid Id
    {
        get { return this._id; }
    }
}

public class AggregateRootNotFoundException : Exception
{
    public AggregateRootNotFoundException() { }    
}

public class AggregateRoot
{
    public static Guid ExtractId<TStream>() where TStream : notnull
    {
        try
        {
            return ((AggregateRootAttribute)typeof(TStream)
                .GetCustomAttributes(false)
                .OfType<AggregateRootAttribute>()
                .First()).Id;
        }
        catch (InvalidOperationException)
        {
            throw new AggregateRootNotFoundException();
        }
    }

    public static Guid ExtractId(object obj)
    {
        try
        {
            return ((AggregateRootAttribute)obj.GetType()
                .GetCustomAttributes(false)
                .OfType<AggregateRootAttribute>()
                .First()).Id;
        }
        catch (InvalidOperationException)
        {
            throw new AggregateRootNotFoundException();
        }
    }
}