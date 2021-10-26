using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;

namespace RTC.XNet
{
    public static class ExtendTools
    {
        private static readonly JsonParser JsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
        
        public static T TryParseMessage<T>(this string json) where T : IMessage, new()
        {
            return JsonParser.Parse<T>(json);
        }

        public static string ToJson(this IMessage msg)
        {
            return JsonFormatter.Default.Format(msg);
        }
        
        public static bool GetHeader(this ServerCallContext context, string key, out string value)
        {
            value = context.RequestHeaders.Get(key)?.Value;
            return !string.IsNullOrEmpty(value);
        }

        public static string GetAccountId(this ServerCallContext context, string key = null)
        {
            context.GetHeader(key ?? "user-key", out var account);
            return account;
        }

        public static async Task<bool> WriteSession(this ServerCallContext context, string account, LogServer server)
        {
            if (!server.TryCreateSession(account, out var session)) return false;
            await context.WriteResponseHeadersAsync(new Metadata {{"session-key", session}});
            return true;
        }

        public static string JoinToString(this string[] array, string split= " ")
        {
            var sb = new StringBuilder();
            for (var i = 0; i < array.Length; i++)
            {
                if (i != 0) sb.Append(split);
                sb.Append(array[i]);
                
            }

            return sb.ToString();
        }

    }
}