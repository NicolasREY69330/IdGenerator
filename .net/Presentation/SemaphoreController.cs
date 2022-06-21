using _net.Service;
using Microsoft.AspNetCore.Mvc;

namespace _net.Presentation
{
    [ApiController]
    [Route("api/Semaphore")]
    public class SemaphoreController : Controller
    {
        public SemaphoreController(SequenceIncrementorService sequenceIncrementorService)
            => SequenceIncrementorService = sequenceIncrementorService;

        public SequenceIncrementorService SequenceIncrementorService { get; }

        [HttpGet("GenerateNextSequence")]
        public async Task<long> GenerateNextSequence()
            => await SequenceIncrementorService.GenerateNextSequence();
    }
}
