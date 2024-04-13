namespace Testcontainers.MongoDb;

public class MongoDbReplicaSetContainerTest
{
        [Fact]
        public async Task Foo()
        {
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
                
                var result = await mongoDbContainer.ExecAsync(command);
                var connectionString = $"mongodb://{hostname}:{port}/?replicaSet={replicaSetName}";
                
                // This connection string works!
        }
}