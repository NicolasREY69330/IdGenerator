using _net.Model;
using _net.Service;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IMongoClient>(c => {
    var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
    //settings.WaitQueueSize = 50000;
    return new MongoClient(settings);
});
builder.Services.AddSingleton<SequenceIncrementorService>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.MapGet("/api/hello", () => "Hi !");

app.MapGet("/api/getNextCounter", async (IMongoClient mongoClient) => 
{
    var database = mongoClient.GetDatabase("orderid_sequence");
    var collection = database.GetCollection<Sequence>("sequence");

    var sequence = await collection
    .FindOneAndUpdateAsync
    (
        filter: Builders<Sequence>.Filter.Eq(a => a._id, "globalcsharp"),
        update: Builders<Sequence>.Update.Inc(a => a.Counter, 1),
        options: new FindOneAndUpdateOptions<Sequence, Sequence>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        }
    );

    return sequence?.Counter;
});

app.Run("http://localhost:8001");