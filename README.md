# Drunk.KeuVault.LetsEncrypt

## Overview

**Drunk.KeuVault.LetsEncrypt** is a .NET-based project designed to automate the issuance, renewal, and secure storage of SSL certificates using Let's Encrypt. It integrates with Cloudflare to manage DNS records for domain validation, uses Azure Entra ID (Azure Active Directory) for secure authentication, and stores certificates in Azure Key Vault. The project also offers a pre-built Docker image for quick deployment.

## Project Structure

- **Program.cs**: The entry point for the application.
- **Configs/**: Contains configuration files necessary for running the project and managing SSL certificates.
- **Data/**: Handles data related to certificate issuance and storage.
- **Services/**: Contains services for managing SSL certificates, DNS validation, and communication with external APIs (Let's Encrypt, Cloudflare, etc.).
- **appsettings.json**: The configuration file for API keys, domains, and other necessary settings for certificate management.

## Prerequisites

- **.NET SDK** (if building locally): Ensure that the .NET SDK is installed. [Download the SDK](https://dotnet.microsoft.com/download).
- **Cloudflare Account**: A Cloudflare account and an API token for managing DNS records.
- **Let's Encrypt Account**: A registered account with Let's Encrypt to generate SSL certificates.
- **Azure Key Vault**: An Azure Key Vault instance for secure storage of certificates.
- **Azure Entra ID (Azure Active Directory)**: Azure AD is required to authenticate and access the Azure Key Vault securely.
- **Docker**: If using the provided Docker image, ensure Docker is installed.

## How the System Works

### Key Connections:

1. **Cloudflare API**:
   - The project uses the Cloudflare API to manage DNS records, which is necessary for performing DNS challenges required by Let's Encrypt to validate domain ownership.
   - The configuration uses **CfEmail** (Cloudflare account email) and **CfToken** (API token) for authentication.

2. **Let's Encrypt**:
   - The project integrates with Let's Encrypt to issue free SSL certificates. The application handles domain validation by using DNS records created via Cloudflare.
   - Let's Encrypt's certificates are renewed periodically and automatically.

3. **Azure Key Vault and Azure Entra ID**:
   - The project uses **Azure Entra ID** (formerly Azure AD) for secure authentication to access and upload SSL certificates to **Azure Key Vault**.
   - Azure Entra ID provides token-based authentication via either managed identities or service principals.
   - After generating certificates, the application uploads them to Azure Key Vault, where they are securely stored and available for retrieval.

### Docker Image

If you don't want to build the project from source code, you can use the pre-built Docker image hosted on Docker Hub:

- **Docker Hub URL**: [https://hub.docker.com/r/baoduy2412/keyvault-letsencrypt/tags](https://hub.docker.com/r/baoduy2412/keyvault-letsencrypt/tags)

### Docker Usage

To run the project using the Docker image:

1. Pull the latest Docker image:
   ```bash
   docker pull baoduy2412/keyvault-letsencrypt:latest
   ```

2. Here is a sample to run docker with environment variables according to the `appsettings.json`:
```yaml
services:
  app:
    image: baoduy2412/keyvault-letsencrypt:latest
    environment:
      # using Lets encrypt production
      CertManager__ProductionEnabled: true
      # The Cloudflare account email
      CertManager__CfEmail: "random.email@example.com"
      # The cloudflare DNS Zone API Token with permission (Zone:Read, DNS:Edit)
      CertManager__CfToken: "12345ABCDEF67890randomTokenXYZ"
      # The Azure Key Vault url
      # The pod need to have an Azure EntraID access to import certificate to Vault
      CertManager__KeyVaultUrl: "https://random-keyvault-url.vault.azure.net"
      # This is optional in case of using User Assigned Identity.
      CertManager__KeyVaultUID: "entraID-user-assigned-id"
      # The CRS Information
      CertManager__CertInfo__CountryName: "SG"
      CertManager__CertInfo__State: "Singapore"
      CertManager__CertInfo__Locality: "Singapore"
      CertManager__CertInfo__Organization: "DrunkCoding"
      CertManager__CertInfo__OrganizationUnit: "DC"
      # Cloudflare Zones
      CertManager__Zones__0__ZoneId: "123abc456def789ghi012jkl345mno678"
      CertManager__Zones__0__LetsEncryptEmail: "admin@randomdomain.com"
      CertManager__Zones__0__Domains__0: api.randomdomain.com
      CertManager__Zones__0__Domains__1: "*.randomdomain.com"
      CertManager__Zones__1__ZoneId: "456mno789pqr012stu345vwx678yz123"
      CertManager__Zones__1__LetsEncryptEmail: "admin@anotherdomain.com"
      CertManager__Zones__1__Domains__0: api.anotherdomain.com
      CertManager__Zones__1__Domains__0: "*.anotherdomain.com"
```

## Setup and Installation (Local Development)

If you prefer to build and run the project locally, follow these steps:

1. Clone the repository:
   ```bash
   git clone <repository-url>
   ```

2. Navigate to the project folder:
   ```bash
   cd Drunk.KeuVault.LetsEncrypt
   ```

3. Restore the required packages:
   ```bash
   dotnet restore
   ```

4. Update the `appsettings.json` file or provide the required environment variables for Cloudflare, Let's Encrypt, and Azure Key Vault configuration.

5. Build the project:
   ```bash
   dotnet build
   ```

6. Run the project:
   ```bash
   dotnet run
   ```

## Configuration

In addition to setting configuration values via `appsettings.json` (for local development), you can now configure the application using environment variables in Docker.

### Example Configuration with Randomized Values for appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "CertManager": {
    "ProductionEnabled": true,
    "CfEmail": "random.email@example.com",
    "CfToken": "12345ABCDEF67890randomTokenXYZ",
    "Zones": [
      {
        "ZoneId": "123abc456def789ghi012jkl345mno678",
        "LetsEncryptEmail": "admin@randomdomain.com",
        "Domains": [
          "api.randomdomain.com"
        ]
      },
      {
        "ZoneId": "456mno789pqr012stu345vwx678yz123",
        "LetsEncryptEmail": "admin@anotherdomain.com",
        "Domains": [
          "api.anotherdomain.com"
        ]
      }
    ],
    "CertInfo": {
      "CountryName": "US",
      "State": "California",
      "Locality": "Los Angeles",
      "Organization": "RandomCorp",
      "OrganizationUnit": "IT"
    },
    "KeyVaultUrl": "https://random-keyvault-url.vault.azure.net",
    "KeyVaultUID": "entraID-user-assigned-id", //This is optional and useful when deploy to AKS
  }
}
```

## Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request for any improvements or bugs.

## License

This project is licensed under the MIT License. See the LICENSE file for more information.
