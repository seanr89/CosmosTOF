using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ToFConsole
{
    class Program
    {
        #region settings
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

        #endregion

        //<Main>
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            try
            {
                Console.WriteLine("Beginning operations...\n");
                bool delete = Confirm("Do you want to clean the DB?");
                Program p = new Program();
                await p.InitialiseCosmosDbAndContainer(delete);

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

        public async Task InitialiseCosmosDbAndContainer(bool delete)
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            //await this.AddItemsToContainerAsync();

            await this.QueryItemsAsync();
            await this.QueryItemsAsync("HealthTestOrderForm");

            if (delete)
                await this.DeleteDatabaseAndCleanupAsync();
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
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/Type", 400);
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
                ItemResponse<TestOrderForm> recordResponse = await this.container.ReadItemAsync<TestOrderForm>(baseRecord.Id.ToString(), new PartitionKey(baseRecord.Type));
                Console.WriteLine("Item in database with id: {0} already exists\n", recordResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<TestOrderForm> tofBaseResponse = await this.container.CreateItemAsync<TestOrderForm>(baseRecord, new PartitionKey(baseRecord.Type));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", tofBaseResponse.Resource.Id, tofBaseResponse.RequestCharge);
            }

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<TestOrderForm> recordResponse = await this.container.ReadItemAsync<TestOrderForm>(healthRec.Id.ToString(), new PartitionKey(healthRec.Type));
                Console.WriteLine("Item in database with id: {0} already exists\n", recordResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<TestOrderForm> healthBaseResponse = await this.container.CreateItemAsync<TestOrderForm>(healthRec, new PartitionKey(healthRec.Type));
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

        // <QueryItemsAsync>
        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of Type in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task QueryItemsAsync(string type = "TestOrderForm")
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.Type = '{type}'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<TestOrderForm> queryResultSetIterator = this.container.GetItemQueryIterator<TestOrderForm>(queryDefinition);

            List<TestOrderForm> records = new List<TestOrderForm>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<TestOrderForm> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (TestOrderForm record in currentResultSet)
                {
                    //families.Add(family);
                    Console.WriteLine("\tRead {0}\n", record.ToString());
                }
            }
        }
        // </QueryItemsAsync>

        // <DeleteDatabaseAndCleanupAsync>
        /// <summary>
        /// Delete the database and dispose of the Cosmos Client instance
        /// </summary>
        private async Task DeleteDatabaseAndCleanupAsync()
        {
            DatabaseResponse databaseResourceResponse = await this.database.DeleteAsync();
            // Also valid: await this.cosmosClient.Databases["FamilyDatabase"].DeleteAsync();

            Console.WriteLine("Deleted Database: {0}\n", this.databaseId);

            //Dispose of CosmosClient
            this.cosmosClient.Dispose();
        }
        // </DeleteDatabaseAndCleanupAsync>

        /// <summary>
        /// Test method to handle Yes/No selection on console apps
        /// </summary>
        /// <param name="title">The text to display on the read message</param>
        /// <returns></returns>
        public static bool Confirm(string title)
        {
            ConsoleKey response;
            do
            {
                Console.Write($"{ title } [y/n] ");
                response = Console.ReadKey(false).Key;
                if (response != ConsoleKey.Enter)
                {
                    Console.WriteLine();
                }
            } while (response != ConsoleKey.Y && response != ConsoleKey.N);

            return (response == ConsoleKey.Y);
        }
    }
}
