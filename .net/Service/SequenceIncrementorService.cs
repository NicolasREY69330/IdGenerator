using _net.Model;
using MongoDB.Driver;

namespace _net.Service
{
    public class SequenceIncrementorService
    {
        private readonly IMongoClient _mongoClient;
        private readonly SemaphoreSlim _semaphore;

        public SequenceIncrementorService(
            IMongoClient mongoClient
            )
        {
            _mongoClient = mongoClient;
            _semaphore = new SemaphoreSlim(100, 100);
        }

        public async Task<long> GenerateNextSequence()
        {
            var database = _mongoClient.GetDatabase("orderid_sequence");
            var collection = database.GetCollection<Sequence>("sequence");
            var sequence = new Sequence();
            try
            {
                await _semaphore.WaitAsync();
                sequence = await collection
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
            }
            finally
            {
                _semaphore.Release();
            }

            return sequence.Counter;
        }
    }
}
