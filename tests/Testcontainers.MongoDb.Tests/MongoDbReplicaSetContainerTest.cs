namespace Testcontainers.MongoDb;

public class MongoDbReplicaSetContainerTest
{
        [Fact]
        public async Task ReplicaSetIsAccessibleWithReplicaSetConnectionString()
        {
                // Given
                const string replicaSetName = "rs0";
                const string hostname = "mongo";
                
                var mongoDbContainer =
                        new MongoDbBuilder()
                                .WithUsername("")
                                .WithPassword("")
                                .WithHostname(hostname)
                                .WithImage("mongo:7.0.7-jammy")
                                .WithPortBinding(27017, false)
                                .WithReplicaSet(replicaSetName)
                                .Build();

                await mongoDbContainer.StartAsync();
                
                var port = mongoDbContainer.GetMappedPublicPort(27017);
                
                var initiateCommand = 
                        $@"rs.initiate({{_id: '{replicaSetName}', members: [{{_id: 0, host: '{hostname}:27017'}}]}});";
                
                var command = new[]
                {
                        "mongosh",
                        "--eval",
                        initiateCommand
                };
                
                await mongoDbContainer.ExecAsync(command);
                var connectionString = $"mongodb://{hostname}:{port}/?replicaSet={replicaSetName}";
                
                var client = new MongoClient(connectionString);

                // When
                using var databases = await client.ListDatabasesAsync();

                // Then
                Assert.Contains(databases.ToEnumerable(), database => database.TryGetValue("name", out var name) && "admin".Equals(name.AsString));
        }
}