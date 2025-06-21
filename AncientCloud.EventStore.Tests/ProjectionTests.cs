using System;
using System.Data.Common;
using System.Threading.Tasks;
using AncientCloud.EventStore.Tests.Invoices;
using Microsoft.Data.Sqlite;


namespace AncientCloud.EventStore.Tests;

[TestClass]
public class ProjectionTests
{
    [TestMethod]
    public async Task Projections()
    {
        using (DbConnection con = new SqliteConnection("Data Source=:memory:"))
        {
            con.Open();
            IEventStore store = new EventStore(con);
            ProjectionTester mock = new ProjectionTester();
            store.RegisterProjection(mock);

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

            Assert.IsTrue(mock.InitSatified);
            Assert.IsTrue(mock.Ran);
        }
    }
}
