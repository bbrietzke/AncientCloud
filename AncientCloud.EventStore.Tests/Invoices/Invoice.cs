using System;
using System.Security.Cryptography.X509Certificates;

namespace AncientCloud.EventStore.Tests.Invoices;


[AggregateRoot("813e2a73-5813-444e-9deb-4b2d4a22cd0c")]
public class Invoice
{
    public Invoice() { }

    public string CustomerName { get; set; }
    public DateTime? InvoicedOn { get; set; }
    public DateTime? PaidOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public decimal Amount { get; set; }
    public Guid Id { get; set; }
    public long Version { get; set; }
    public InvoiceStatus Status { get; set; }

    public static Invoice Evolve(Invoice inv, IEnumerable<object> events)
    {
        foreach (var @event in @events)
        {
            inv = Invoice.Evolve(inv, @event);
        }

        return inv;
    }

    public static Invoice Evolve(Invoice invoice, object @event)
    {
        return @event switch
        {
            InvoiceCreated created => Create(created),
            InvoiceBilled i => invoice.Apply(i),
            AdjustInvoiceAmount i => invoice.Apply(i),
            InvoicePaid i => invoice.Apply(i),
            _ => invoice
        };
    }

    private static Invoice Create(InvoiceCreated @event)
    {
        return new Invoice
        {
            Version = @event.Version,
            CustomerName = @event.Name,
            Status = InvoiceStatus.Created,
            Id = @event.InvoiceId,
            PaidOn = null,
            InvoicedOn = null,
            CreatedOn = DateTime.Now,
            Amount = @event.Amount
        };
    }

    private Invoice Apply(AdjustInvoiceAmount @event)
    {
        if (Status == InvoiceStatus.Created)
        {
            Amount += @event.AdjustBy;
            Version = @event.Version;
        }
        else
        {
            throw new InvalidOperationException("Invoice cannot be modified after it has been billed");
        }

        return this;
    }

    private Invoice Apply(InvoiceBilled @event)
    {
        Status = InvoiceStatus.Billed;
        InvoicedOn = DateTime.Now;
        Version = @event.Version;

        return this;
    }

    private Invoice Apply(InvoicePaid @event)
    {
        Status = InvoiceStatus.Paid;
        PaidOn = DateTime.Now;
        Version = @event.Version;

        return this;
    }
}

public enum InvoiceStatus
{
    Created,
    Billed,
    Paid
}

public class InvoiceCreated
{
    public string Name { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedOn { get; set; }
    public long Version { get; set; }
}

public class AdjustInvoiceAmount
{
    public Guid InvoiceId { get; set; }
    public decimal AdjustBy { get; set; }
    public long Version { get; set; }
}

public class InvoiceBilled
{
    public Guid InvoiceId { get; set; }
    public long Version { get; set; }
}

public class InvoicePaid
{
    public Guid InvoiceId { get; set; }
    public long Version { get; set; }
}