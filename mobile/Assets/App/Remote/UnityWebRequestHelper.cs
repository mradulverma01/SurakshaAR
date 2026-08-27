using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SurakshaAR.Remote
{
    internal static class UnityWebRequestHelper
    {
        public static Task SendAsync(UnityWebRequest request, CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<bool>();
            var operation = request.SendWebRequest();
            operation.completed += _ => completion.TrySetResult(true);
            cancellationToken.Register(() =>
            {
                request.Abort();
                completion.TrySetCanceled();
            });
            return completion.Task;
        }
    }
}
