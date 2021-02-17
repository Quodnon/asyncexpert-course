using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwaitExercises.Core
{
    public class AsyncHelpers
    {
        public static async Task<string> GetStringWithRetries(HttpClient client, string url, int maxTries = 3, CancellationToken token = default)
        {
            // Create a method that will try to get a response from a given `url`, retrying `maxTries` number of times.
            // It should wait one second before the second try, and double the wait time before every successive retry
            // (so pauses before retries will be 1, 2, 4, 8, ... seconds).
            // * `maxTries` must be at least 2
            // * we retry if:
            //    * we get non-successful status code (outside of 200-299 range), or
            //    * HTTP call thrown an exception (like network connectivity or DNS issue)
            // * token should be able to cancel both HTTP call and the retry delay
            // * if all retries fails, the method should throw the exception of the last try
            // HINTS:
            // * `HttpClient.GetStringAsync` does not accept cancellation token (use `GetAsync` instead)
            // * you may use `EnsureSuccessStatusCode()` method

            if (maxTries < 2)
                throw new ArgumentException(nameof(maxTries));

            return await Task.Run(() => InternalGetStringWithRetriesAsync(client, url, maxTries, token), token);
        }
        static async Task<string> InternalGetStringWithRetriesAsync(HttpClient client, string url, int maxTries,
            CancellationToken token)
        {
            var attempt = 0;
            
            do
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    using var response = await client.GetAsync(url, token);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                    if (attempt == maxTries)
                        throw;
                }

                var timeout = getWaitTimeoutSpan(attempt);
                await Task.Delay(timeout, token);
            } while (attempt++ < maxTries);

            return default;
        }

        static TimeSpan getWaitTimeoutSpan(int attempt)
        {
            var timeoutSeconds = 1 << attempt;
            return TimeSpan.FromSeconds(timeoutSeconds);
        }
    }
}
