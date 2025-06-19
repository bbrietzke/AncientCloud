using System;
using System.Security;

namespace AncientCloud.EventStore.Tests.Invoices;

public sealed class ProjectionTester : Projection
{
    public ProjectionTester()
    {
        this.Ran = false;
        this.InitSatified = false;

        projects<InvoiceCreated>(Apply);
    }

    public override void Init()
    {
        this.InitSatified = true;
    }

    public bool Ran { get; private set; } = false;
    public bool InitSatified { get; private set; } = false;

    private Task Apply(InvoiceCreated @event, CancellationToken ct)
    {
        this.Ran = true;
        
        return Task.CompletedTask;
    }
}
