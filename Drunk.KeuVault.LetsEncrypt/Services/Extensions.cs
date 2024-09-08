using Certes.Acme;
using Certes.Acme.Resource;

namespace Drunk.KeuVault.LetsEncrypt.Services;

/// <summary>
/// Provides extension methods for ACME order and challenge contexts.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Tries to download the certificate chain for the given order context.
    /// </summary>
    /// <param name="orderContext">The order context.</param>
    /// <param name="preferredChain">The preferred certificate chain.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the certificate chain, or null if the download fails.</returns>
    public static async Task<CertificateChain?> TryDownload(this IOrderContext orderContext, string? preferredChain = null,
        CancellationToken cancellationToken = default)
    {
        const int maxTry = 5;
        var count = 0;

        do
        {
            try
            {
                return await orderContext.Download(preferredChain);
            }
            catch (Exception)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
            finally
            {
                count++;
            }
        } while (count < maxTry);

        return null;
    }

    /// <summary>
    /// Tries to validate the challenge for the given challenge context.
    /// </summary>
    /// <param name="challengeContext">The challenge context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task TryChallenge(this IChallengeContext challengeContext,
        CancellationToken cancellationToken = default)
    {
        Challenge rs;
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

            rs = await challengeContext.Validate();
            if (cancellationToken.IsCancellationRequested) break;

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

        } while (rs.Status == ChallengeStatus.Pending);

        Console.WriteLine($"Challenge is completed: {challengeContext.Type}");
    }
}