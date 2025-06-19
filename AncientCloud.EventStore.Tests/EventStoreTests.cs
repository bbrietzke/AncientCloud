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
                invoiceId, new object[] { newInvoice }, null
            );

            var events = await store.GetEventsAsync(invoiceId);

            Assert.AreEqual(1, events.Count);
        }
    }
}
