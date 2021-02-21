using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ToFConsole
{
    class Program
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "TestOrderForms";
        private string containerId = "Items";

        //<Main>
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            try
            {
                Console.WriteLine("Beginning operations...\n");
                Program p = new Program();
                //await p.GetStartedDemoAsync();

            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        public async Task InitialiseCosmosDbAndContainer()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.AddItemsToContainerAsync();
        }

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }
        // </CreateDatabaseAsync>

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/LastName", 400);
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        // <AddItemsToContainerAsync>
        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private async Task AddItemsToContainerAsync()
        {
            var baseRecord = await CreateBasicForm();
            var healthRec = await CreateHealthForm();

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<TestOrderForm> recordResponse = await this.container.ReadItemAsync<TestOrderForm>(baseRecord.Id.ToString(), new PartitionKey(baseRecord.GetType().Name));
                Console.WriteLine("Item in database with id: {0} already exists\n", recordResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<TestOrderForm> tofBaseResponse = await this.container.CreateItemAsync<TestOrderForm>(baseRecord, new PartitionKey(baseRecord.GetType().Name));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", tofBaseResponse.Resource.Id, tofBaseResponse.RequestCharge);
            }

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<TestOrderForm> recordResponse = await this.container.ReadItemAsync<TestOrderForm>(healthRec.Id.ToString(), new PartitionKey(healthRec.GetType().Name));
                Console.WriteLine("Item in database with id: {0} already exists\n", recordResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<TestOrderForm> healthBaseResponse = await this.container.CreateItemAsync<TestOrderForm>(healthRec, new PartitionKey(healthRec.GetType().Name));
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", healthBaseResponse.Resource.Id, healthBaseResponse.RequestCharge);
            }
        }

        #region ToF Creation

        private async Task<TestOrderForm> CreateBasicForm()
        {
            return await Task.Run(() =>
            {
                var record = new TestOrderForm("Iron", DateTime.Now, "RCLS");
                return record;
            });
        }

        private async Task<HealthTestOrderForm> CreateHealthForm()
        {
            return await Task.Run(() =>
            {
                var record = new HealthTestOrderForm("EM", DateTime.Now, "Randox Health");
                var content = new CustomContent()
                {
                    Type = "string",
                    Value = "PID1234"
                };
                record.PID = "1234";
                record.DateOfBirth = DateTime.Now.AddYears(-7).AddMonths(-1);
                record.MetaData.Add("PID", content);

                return record;
            });
        }

        #endregion
    }
}
