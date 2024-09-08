using Drunk.Cf.Dns;

namespace Drunk.KeuVault.LetsEncrypt.Services;

/// <summary>
/// Helper class for managing DNS records with Cloudflare.
/// </summary>
public class CfDnsHelper(ICloudflareDnsClient cfDnsClient)
{
    /// <summary>
    /// Upserts a DNS TXT record in the specified zone.
    /// </summary>
    /// <param name="zoneId">The ID of the DNS zone.</param>
    /// <param name="recordName">The name of the DNS record.</param>
    /// <param name="recordValue">The value of the DNS record.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the upserted DNS record result, or null if the operation fails.</returns>
    public async Task<DnsRecordResult?> UpsertRecord(string zoneId, string recordName, string recordValue)
    {
        var record = new DnsRecord { Name = recordName, Content = recordValue, Type = RecordType.TXT, Ttl = 120 };
        var found = (await cfDnsClient.FindByNameAsync(zoneId, recordName)).Result.FirstOrDefault();

        found = (found is not null)
            ? (await cfDnsClient.UpdateAsync(zoneId, found.Id, record)).Result
            : (await cfDnsClient.CreateAsync(zoneId, record)).Result;

        Console.WriteLine($"DNS record created: {recordName}");
        return found;
    }
}