﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace AspNet.Security.OpenIdConnect.Server {
    internal static class OpenIdConnectServerHelpers {
        public static X509Certificate2 GetCertificate(StoreName name, StoreLocation location, string thumbprint) {
            var store = new X509Store(name, location);

            try {
                store.Open(OpenFlags.ReadOnly);

                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);

                return certificates.OfType<X509Certificate2>().SingleOrDefault();
            }

            finally {
#if NETSTANDARD1_4
                store.Dispose();
#else
                store.Close();
#endif
            }
        }

        public static string GetIssuer(this HttpContext context, OpenIdConnectServerOptions options) {
            var issuer = options.Issuer;
            if (issuer == null) {
                if (!Uri.TryCreate(context.Request.Scheme + "://" + context.Request.Host +
                                   context.Request.PathBase, UriKind.Absolute, out issuer)) {
                    throw new InvalidOperationException("The issuer address cannot be inferred from the current request");
                }
            }

            return issuer.AbsoluteUri;
        }

        public static string AddPath(this string address, PathString path) {
            if (address.EndsWith("/")) {
                address = address.Substring(0, address.Length - 1);
            }

            return address + path;
        }

        public static bool IsSupportedAlgorithm(this SecurityKey key, string algorithm) {
            return key.CryptoProviderFactory.IsSupportedAlgorithm(algorithm, key);
        }

        public static HashAlgorithm GetHashAlgorithm(string algorithm) {
            if (string.IsNullOrEmpty(algorithm)) {
                throw new ArgumentNullException(nameof(algorithm));
            }

            switch (algorithm) {
                case SecurityAlgorithms.RsaSha256:
                case SecurityAlgorithms.HmacSha256:
                case SecurityAlgorithms.EcdsaSha256:
                case SecurityAlgorithms.RsaSha256Signature:
                case SecurityAlgorithms.HmacSha256Signature:
                case SecurityAlgorithms.EcdsaSha256Signature:
                    return SHA256.Create();

                case SecurityAlgorithms.RsaSha384:
                case SecurityAlgorithms.HmacSha384:
                case SecurityAlgorithms.EcdsaSha384:
                case SecurityAlgorithms.RsaSha384Signature:
                case SecurityAlgorithms.HmacSha384Signature:
                case SecurityAlgorithms.EcdsaSha384Signature:
                    return SHA384.Create();

                case SecurityAlgorithms.RsaSha512:
                case SecurityAlgorithms.HmacSha512:
                case SecurityAlgorithms.EcdsaSha512:
                case SecurityAlgorithms.RsaSha512Signature:
                case SecurityAlgorithms.HmacSha512Signature:
                case SecurityAlgorithms.EcdsaSha512Signature:
                    return SHA512.Create();
            }

            throw new NotSupportedException($"The hash algorithm cannot be inferred from the '{algorithm}' signature algorithm.");
        }

        public static string GetJwtAlgorithm(string algorithm) {
            if (string.IsNullOrEmpty(algorithm)) {
                throw new ArgumentNullException(nameof(algorithm));
            }

            switch (algorithm) {
                case SecurityAlgorithms.HmacSha256:
                case SecurityAlgorithms.HmacSha256Signature:
                    return SecurityAlgorithms.HmacSha256;

                case SecurityAlgorithms.HmacSha384:
                case SecurityAlgorithms.HmacSha384Signature:
                    return SecurityAlgorithms.HmacSha384;

                case SecurityAlgorithms.HmacSha512:
                case SecurityAlgorithms.HmacSha512Signature:
                    return SecurityAlgorithms.HmacSha512;

                case SecurityAlgorithms.RsaSha256:
                case SecurityAlgorithms.RsaSha256Signature:
                    return SecurityAlgorithms.RsaSha256;

                case SecurityAlgorithms.RsaSha384:
                case SecurityAlgorithms.RsaSha384Signature:
                    return SecurityAlgorithms.RsaSha384;

                case SecurityAlgorithms.RsaSha512:
                case SecurityAlgorithms.RsaSha512Signature:
                    return SecurityAlgorithms.RsaSha512;

                case SecurityAlgorithms.RsaOaepKeyWrap:
                    return "RSA-OAEP";

                case SecurityAlgorithms.RsaV15KeyWrap:
                    return "RSA1_5";

                default:
                    return null;
            }
        }

        public static string GenerateKey(this RandomNumberGenerator generator, int length) {
            if (generator == null) {
                throw new ArgumentNullException(nameof(generator));
            }

            var bytes = new byte[length];
            generator.GetBytes(bytes);

            return Base64UrlEncoder.Encode(bytes);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool AreEqual(string first, string second) {
            // Note: these null checks can be theoretically considered as early checks
            // (which would defeat the purpose of a time-constant comparison method),
            // but the expected string length is the only information an attacker
            // could get at this stage, which is not critical where this method is used.

            if (first == null && second == null) {
                return true;
            }

            if (first == null || second == null) {
                return false;
            }

            if (first.Length != second.Length) {
                return false;
            }

            var result = true;

            for (var index = 0; index < first.Length; index++) {
                result &= first[index] == second[index];
            }

            return result;
        }
    }
}
