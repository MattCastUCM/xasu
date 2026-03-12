namespace Xasu.Auth.Protocols.OAuth2
{
    public interface IPKCE
    {
        void GenerateOrGetSaved(PKCETypes pkceType, out string codeVerifier, out string codeChallenge);
    }
}
