namespace OpenIddictSetUp.OpenIddict
{
    public class OpenIddictConfiguration
    {
        public Dictionary<string, OpenIddictClientConfiguration>? ClientConfiguration { get; set; }
        public OpenIdCertificateInfo? SigningCertificate { get; set; }
        public OpenIdCertificateInfo? EncryptionCertificate { get; set; }
        public OpenIdCertificateInfo? FileCertificate { get; set; }

        /// <summary>
        /// Public URL to be able to use relative URLs in Client's RedirectUri
        /// </summary>
        public string PublicUrl { get; set; } = string.Empty;

    }

    /// <summary>
    /// DTO for certificate information
    /// </summary>
    public class OpenIdCertificateInfo
    {
        public string CertificateFile { get; set; } = string.Empty;
        /// <summary>
        /// Certificate in base64 format (so that it could be injected via env. variables)
        /// </summary>
        public string Base64Certificate { get; set; } = string.Empty;

        /// <summary>
        /// Certificate password
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
