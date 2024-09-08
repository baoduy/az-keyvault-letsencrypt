using Certes.Acme;
using Certes.Acme.Resource;

namespace Drunk.KeuVault.LetsEncrypt.Services;

public static class Extensions
{
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