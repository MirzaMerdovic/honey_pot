using System.Collections.Concurrent;

namespace HoneyPot.Api
{
    public sealed class HoneyCollector : NonBlocking.ConcurrentDictionary<string, long>
    {
        public void RegisterHoneyAmount(string name, long amount)
        {
            AddOrUpdate(name, amount, (_, currentAmount) => currentAmount += amount);
        }
    }
}
