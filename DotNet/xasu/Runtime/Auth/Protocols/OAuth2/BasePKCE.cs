using System;
using System.Security.Cryptography;
using System.Text;
using Xasu.Util;

namespace Xasu.Auth.Protocols.OAuth2
{
    public class BasePKCE : IPKCE
    {
        protected const string PKCE_CODE_VERIFIER_KEY = "pkce_code_verifier";
        protected const string PKCE_CODE_CHALLENGE_KEY = "pkce_code_challenge";
        protected const string PKCE_TYPE_NOT_SUPPORTED_MESSAGE = "PKCE type \"{0}\" not supported. Please use \"S256\" type.";


        public virtual void GenerateOrGetSaved(PKCETypes pkceType, out string codeVerifier, out string codeChallenge)
        {
            // Create random code verifier
            var codeVerifierBytes = new byte[32];
            RandomNumberGenerator.Create().GetBytes(codeVerifierBytes);
            codeVerifier = Base64Url.Encode(codeVerifierBytes);

            // Create code challenge
            switch (pkceType)
            {
                case PKCETypes.S256:
                    using (var sha256 = SHA256.Create())
                    {
                        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                        codeChallenge = Base64Url.Encode(challengeBytes);

                        // We save the codes in case we need them later
                        PersistentPrefs.SetString(PKCE_CODE_VERIFIER_KEY, codeVerifier);
                        PersistentPrefs.SetString(PKCE_CODE_CHALLENGE_KEY, codeChallenge);
                        PersistentPrefs.Save();
                    }
                    break;
                default:
                    throw new NotSupportedException(string.Format(PKCE_TYPE_NOT_SUPPORTED_MESSAGE, pkceType));
            }
        }
    }

}
