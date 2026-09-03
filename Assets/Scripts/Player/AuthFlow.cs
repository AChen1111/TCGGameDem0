using System.Text.RegularExpressions;
using System.Threading;
using AChen.Networking;
using Cysharp.Threading.Tasks;

namespace AChen.Player
{
    public enum AuthMode
    {
        Login,
        Register
    }

    public readonly struct AuthResult
    {
        public bool Succeeded { get; }
        public string ErrorMessage { get; }

        AuthResult(bool succeeded, string errorMessage)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
        }

        public static AuthResult Success() => new AuthResult(true, null);
        public static AuthResult Failure(string message) => new AuthResult(false, message);
    }

    /// <summary>
    /// 登录/注册流程：输入校验、调用会话、错误码转用户提示。不含任何 UI 表现。
    /// </summary>
    public static class AuthFlow
    {
        static readonly Regex s_usernamePattern = new Regex(@"^[A-Za-z0-9_]{3,24}$", RegexOptions.Compiled);

        /// <summary>返回 null 表示通过，否则为可直接展示的提示文本。</summary>
        public static string Validate(AuthMode mode, string username, string password, string passwordConfirm)
        {
            if (username == null || !s_usernamePattern.IsMatch(username))
            {
                return "账号需为 3-24 位英文、数字或下划线";
            }

            if (password == null || password.Length is < 8 or > 128)
            {
                return "密码长度需为 8-128 位";
            }

            if (mode == AuthMode.Register && StringValidator.IsWeakPassword(password))
            {
                return "密码过弱";
            }

            if (mode == AuthMode.Register && password != passwordConfirm)
            {
                return "两次输入的密码不一致";
            }

            return null;
        }

        /// <summary>调用 PlayerSession 完成登录或注册。取消时抛出 OperationCanceledException，其余后端错误转为失败结果。</summary>
        public static async UniTask<AuthResult> AuthenticateAsync(
            AuthMode mode,
            string username,
            string password,
            CancellationToken cancellationToken)
        {
            try
            {
                if (mode == AuthMode.Register)
                {
                    await PlayerSession.Instance.RegisterAsync(username, password, cancellationToken);
                    ALog.Log("账号注册成功并建立玩家会话.", ALogCategories.Net);
                }
                else
                {
                    await PlayerSession.Instance.LoginAsync(username, password, cancellationToken);
                    ALog.Log("账号登录成功并建立玩家会话.", ALogCategories.Net);
                }

                return AuthResult.Success();
            }
            catch (BackendApiException exception)
            {
                ALog.LogError(
                    $"账号认证失败. Mode={mode}; Code={exception.Code}; Status={exception.StatusCode}",
                    ALogCategories.Net);
                return AuthResult.Failure(GetErrorMessage(exception.Code));
            }
        }

        public static string GetErrorMessage(string code) => code switch
        {
            "ACCOUNT_EXISTS" => "该账号已被注册",
            "INVALID_CREDENTIALS" => "账号或密码错误",
            "VALIDATION_ERROR" => "账号或密码格式不正确",
            "NETWORK_ERROR" => "无法连接服务器，请检查网络",
            "RATE_LIMITED" => "操作过于频繁，请稍后再试",
            _ => "账号操作失败，请稍后再试"
        };
    }
}
