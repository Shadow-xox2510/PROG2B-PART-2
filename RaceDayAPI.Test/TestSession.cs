using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RaceDayAPI.Tests
{
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _session = new();

        public IEnumerable<string> Keys => _session.Keys;

        public string Id => "TestSession";

        public bool IsAvailable => true;

        public void Clear()
        {
            _session.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _session.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _session[key] = value;
        }

        public bool TryGetValue(string key, out byte[]? value)
        {
            return _session.TryGetValue(key, out value);
        }
    }
}
