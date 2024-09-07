using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Drunk.Cf.Dns;

public enum RecordType
{
    A, // IPv4 address
    AAAA, // IPv6 address
    CNAME, // Canonical name (alias)
    TXT, // Text record
    MX, // Mail exchange record
    NS, // Name server
    SRV, // Service locator
    SPF, // Sender Policy Framework (deprecated)
    PTR, // Pointer record (used for reverse DNS lookups)
    LOC, // Location record
    CAA, // Certification Authority Authorization
    CERT, // Certificate record
    DNSKEY, // DNSSEC public key
    DS, // Delegation signer
    NAPTR, // Naming Authority Pointer
    SMIMEA, // S/MIME cert association
    SSHFP, // SSH public key fingerprint
    TLSA, // TLS authentication
    URI // Uniform Resource Identifier
}

public class DnsRecord
{
    public RecordType Type { get; init; }
    public string Name { get; init; }
    public string Content { get; init; }
    public int Ttl { get; init; }

    public bool Proxied { get; init; }
}

public class DnsRecordResult : DnsRecord
{
    public string Id { get; init; }
}

public class DnsResponse<T>
{
    public bool Success { get; init; }
    public T Result { get; init; }
    public List<object> Errors { get; init; } = [];
    public List<object> Messages { get; init; } = [];
}

[Headers("Authorization: Bearer")]
public interface ICloudflareDnsClient
{
    [Get("/zones/{zoneId}/dns_records")]
    Task<DnsResponse<List<DnsRecordResult>>> ListAsync([AliasAs("zoneId")] string zoneId);

    [Get("/zones/{zoneId}/dns_records")]
    Task<DnsResponse<List<DnsRecordResult>>> FindByNameAsync([AliasAs("zoneId")] string zoneId, [Query] string name);

    [Post("/zones/{zoneId}/dns_records")]
    Task<DnsResponse<DnsRecordResult>> CreateAsync([AliasAs("zoneId")] string zoneId, [Body] DnsRecord record);

    [Patch("/zones/{zoneId}/dns_records/{dns_record_id}")]
    Task<DnsResponse<DnsRecordResult>> UpdateAsync([AliasAs("zoneId")] string zoneId, [AliasAs("dns_record_id")] string id,
        [Body] DnsRecord record);

    [Delete("/zones/{zoneId}/dns_records/{recordId}")]
    Task DeleteAsync([AliasAs("zoneId")] string zoneId, [AliasAs("recordId")] string recordId);
}