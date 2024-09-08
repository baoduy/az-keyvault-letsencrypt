using Drunk.Cf.Dns;

namespace Drunk.KeuVault.LetsEncrypt.Services;

public class CfDnsHelper(ICloudflareDnsClient cfDnsClient)
{
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