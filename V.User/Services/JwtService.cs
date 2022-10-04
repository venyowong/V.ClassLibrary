using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace V.User.Services
{
    public class JwtService
    {
        private byte[] key;

        public JwtService(string secret)
        {
            this.key = Encoding.ASCII.GetBytes(secret);
        }

        public string GenerateToken(Dictionary<string, string> claims, TimeSpan? expiration = null)
        {
            var handler = new JsonWebTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.Select(x => new Claim(x.Key, x.Value)).ToArray()),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(this.key), SecurityAlgorithms.HmacSha256Signature)
            };
            if (expiration != null)
            {
                descriptor.Expires = DateTime.UtcNow.Add(expiration.Value);
            }
            else
            {
                descriptor.Expires = DateTime.UtcNow.AddYears(99);
            }
            return handler.CreateToken(descriptor);
        }

        public Dictionary<string, object> ParseToken(string token)
        {
            var handler = new JsonWebTokenHandler();
            try
            {
                var result = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(this.key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                });
                if (result.Claims.ContainsKey("exp"))
                {
                    var exp = result.Claims["exp"];
                    if (exp != null)
                    {
                        var start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        start = start.AddSeconds((long)exp).ToLocalTime();
                        if (start < DateTime.Now)
                        {
                            return null;
                        }
                    }
                }

                return result.Claims.ToDictionary(x => x.Key, x => x.Value);
            }
            catch
            {
                return null;
            }
        }

        public Dictionary<string, object> GetTokenClaimsFromContext(HttpContext context)
        {
            var request = context.Request;
            var token = request.Query["token"].ToString();
            if (string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    token = request.Form["token"].ToString();
                }
                catch { }
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                token = request.Headers["token"].ToString();
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                request.EnableBuffering();
                try
                {
                    using (var reader = new StreamReader(request.Body, Encoding.UTF8, false, -1, true))
                    {
                        request.Body.Position = 0;
                        var body = reader.ReadToEndAsync().Result;
                        request.Body.Position = 0;
                        token = JsonConvert.DeserializeObject<JObject>(body)["token"].ToString();
                    }
                }
                catch { }
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            return this.ParseToken(token);
        }
    }
}
