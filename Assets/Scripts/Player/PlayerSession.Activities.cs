using System.Threading;
using AChen.Activities;
using AChen.Configuration;
using AChen.Networking;
using Cysharp.Threading.Tasks;

namespace AChen.Player
{
    public sealed partial class PlayerSession
    {
        ActivityManager m_activities;
        public ActivityManager Activities => m_activities ??= new ActivityManager(this);
        internal string ActivityBackendAddress => Api.ActivityBackendAddress;
        internal UniTask<byte[]> DownloadActivityAsync(string path, CancellationToken token) =>
            SendAuthenticatedCallAsync((access, ct) => Api.DownloadActivityAsync(access, path, ct), token);
        internal async UniTask<ActivityIndexResponse> LoadActivitiesAsync(bool visit, CancellationToken token)
        {
            // Refresh also recovers inventory after a claim whose response was lost.
            await SendAuthenticatedAsync(Api.GetPlayerAsync, token);
            return await SendAuthenticatedCallAsync((access, ct) => Api.GetActivitiesAsync(access, visit, ct), token);
        }
        internal UniTask<ActivityIndexResponse> ReportActivityPopupAsync(string id, ActivityPopupShownRequest request, CancellationToken token) =>
            SendAuthenticatedCallAsync((access, ct) => Api.ReportActivityPopupAsync(access, id, request, ct), token);
        internal UniTask<ActivityClaimResult> ClaimActivityAsync(string id, ActivityClaimRequest request, bool exchange, CancellationToken token) =>
            ExecuteLockedAsync("ActivityClaim", id + "/" + request.EntryId, async (player, ct) =>
            {
                // Retrying a timeout keeps the entire original request, including its revision.
                var result = await SendAuthenticatedCallAsync((access, retry) => Api.ClaimActivityAsync(access, id, request, exchange, retry), ct);
                if (result.Player.Revision >= CurrentPlayer.Revision) SetCurrentPlayer(result.Player);
                return result;
            }, result => $"Revision={result.Player.Revision}", token);
    }
}
