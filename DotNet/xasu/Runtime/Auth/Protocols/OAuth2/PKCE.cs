using Xasu.Util;

namespace Xasu.Auth.Protocols.OAuth2
{
    public class PKCE : Delegate<IPKCE, BasePKCE>
    {
        static PKCE()
        {
            InitInstance(Factories.Id.PKCE);
        }

        public static void GenerateOrGetSaved(PKCETypes pkceType, out string codeVerifier, out string codeChallenge)
        {
            _instance.GenerateOrGetSaved(pkceType, out codeVerifier, out codeChallenge);
        }
    }
}
