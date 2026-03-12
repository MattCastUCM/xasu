using UnityEngine;
using Xasu.Auth.Protocols.OAuth2;

namespace Xasu.Util
{
    internal class UnityPCKE : BasePKCE
    {
        public override void GenerateOrGetSaved(PKCETypes pkceType, out string codeVerifier, out string codeChallenge)
        {
            if (WebGLUtility.IsWebGLListening())
            {
                Debug.Log("Getting saved PKCE");
                codeVerifier = PersistentPrefs.GetString(PKCE_CODE_VERIFIER_KEY);
                codeChallenge = PersistentPrefs.GetString(PKCE_CODE_CHALLENGE_KEY);

                return;
            }

            base.GenerateOrGetSaved(pkceType, out codeVerifier, out codeChallenge);
        }
    }
}
