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
        using (DbConnection con = new SqliteConnection("Data Source='/tmp/doSomethingManyTimes.sqlite3'"))
        {
            con.Open();
            IEventStore store = new EventStore(con);
            store.Init();

            Guid id = Guid.NewGuid();

            InvoiceCreated newInvoice = new InvoiceCreated
            {
                InvoiceId = id,
                Name = "Customer One",
                Amount = 1000M,
                Version = 0,
            };

            AdjustInvoiceAmount adjustment = new AdjustInvoiceAmount
            {
                AdjustBy = 200M,
                InvoiceId = id,
                Version = 1,
            };

            AdjustInvoiceAmount adjustment2 = new AdjustInvoiceAmount
            {
                AdjustBy = 300M,
                InvoiceId = id,
                Version = 2,
            };

            await store.AppendEvents<Invoice>(
                id, new object[] { newInvoice, adjustment, adjustment2 }, 0
            );

            var events = await store.GetEventsAsync(id);
            Invoice inv = Invoice.Evolve(new Invoice(), events);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(1500M, inv.Amount);
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
