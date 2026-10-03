using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using AChen.Networking;

namespace AChen.Player
{
    public sealed partial class PlayerSession
    {
        internal BackendConfig DuelBackendConfig => m_config;
        internal UniTask<T> DuelAuthenticatedAsync<T>(Func<string, CancellationToken, UniTask<T>> call, CancellationToken token) =>
            SendAuthenticatedCallAsync(call, token);
    }
}
