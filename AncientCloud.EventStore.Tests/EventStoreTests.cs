using System;
using System.Data.Common;
using System.Threading.Tasks;
using AncientCloud.EventStore.Tests.Invoices;
using Microsoft.Data.Sqlite;

namespace AncientCloud.EventStore.Tests;


[TestClass]
public class EventStoreTests
{
    [TestMethod]
    public async Task DoSomethingManyTimes()
    {
        using (DbConnection con = new SqliteConnection("Data Source=:memory:"))
        {
            con.Open();
            IEventStore store = new EventStore(con);
            store.Init();

            Guid id = Guid.NewGuid();
            long version = await store.GetCurrentVersionById(id);

            InvoiceCreated newInvoice = new InvoiceCreated
            {
                InvoiceId = id,
                Name = "Customer One",
                Amount = 1000M,
                Version = version++,
            };

            AdjustInvoiceAmount adjustment = new AdjustInvoiceAmount
            {
                AdjustBy = 200M,
                InvoiceId = id,
                Version = version++,
            };

            AdjustInvoiceAmount adjustment2 = new AdjustInvoiceAmount
            {
                AdjustBy = 300M,
                InvoiceId = id,
                Version = version++,
            };

            InvoiceBilled billed = new InvoiceBilled
            {
                InvoiceId = id,
                Version = version++,
            };

            await store.AppendEvents<Invoice>(
                id, new object[] { newInvoice, adjustment, adjustment2, billed }, version
            );

            var events = await store.GetEventsAsync(id);
            Invoice inv = Invoice.Evolve(new Invoice(), events);

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(1500M, inv.Amount);
            Assert.AreEqual(3, inv.Version);
        }
    }

    [TestMethod]
    public async Task DoSomething()
    {
        using (DbConnection con = new SqliteConnection("Data Source=:memory:"))
        {
            con.Open();
            IEventStore store = new EventStore(con);
            store.Init();

            Guid invoiceId = Guid.NewGuid();

            InvoiceCreated newInvoice = new InvoiceCreated
            {
                InvoiceId = invoiceId,
                Name = "Test Customer",
                Amount = 100.00M,
            };

            await store.AppendEvents<Invoice>(
                invoiceId, new object[] { newInvoice }, 0
            );

            var events = await store.GetEventsAsync(invoiceId);

            Assert.AreEqual(1, events.Count);
        }
    }
}
