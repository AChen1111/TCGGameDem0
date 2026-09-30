using System.Collections.Generic;
using System.Threading;
using AChen.Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace AChen.Networking
{
    public sealed class ActivityClaimResult
    {
        public PlayerData Player;
        public ActivityListResponse Activities;
        public long GoldGained;
        public long UrGained;
    }
    public sealed partial class AuthApi
    {
        static readonly Dictionary<string, string> ActivityHeaders = new Dictionary<string, string>
        { ["X-Activity-Schema"] = "1", ["X-Activity-Types"] = "0,1,2,3,4" };

        async UniTask<T> ActivityRequest<T>(string method, string path, object body, string access, CancellationToken token)
        {
            var result = await m_http.SendRawAsync(method, "/api/activities" + path, body, access, ActivityHeaders, null, token);
            return BackendJson.DeserializeResponse<T>(result.Body);
        }
        public UniTask<ActivityListResponse> GetActivitiesAsync(string access, bool visit, CancellationToken token) =>
            ActivityRequest<ActivityListResponse>(visit ? "POST" : "GET", visit ? "/visit" : "", null, access, token);
        public UniTask<ActivityListResponse> ReportActivityPopupAsync(string access, string id, ActivityPopupShownRequest request, CancellationToken token) =>
            ActivityRequest<ActivityListResponse>("POST", "/" + id + "/popup-shown", request, access, token);
        public async UniTask<ActivityClaimResult> ClaimActivityAsync(string access, string id, ActivityClaimRequest request, bool exchange, CancellationToken token)
        {
            var result = await ActivityRequest<ActivityClaimDto>("POST", "/" + id + (exchange ? "/exchange" : "/claim"), request, access, token);
            return new ActivityClaimResult { Player = ToPlayer(result.Player), Activities = result.Activities, GoldGained = result.GoldGained, UrGained = result.UrGained };
        }
        [Preserve] sealed class ActivityClaimDto
        {
            public PlayerDto Player;
            public ActivityListResponse Activities;
            public long GoldGained;
            public long UrGained;
        }
    }
}
