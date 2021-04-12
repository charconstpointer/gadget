using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Gadget.Server.Authorization.Providers
{
    public class PrMockProvider : ILoginProvider
    {
        private readonly Dictionary<string, string> _users;

        public PrMockProvider()
        {
            _users = new Dictionary<string, string>
            {
                {"test", "5a105e8b9d40e1329780d62ea2265d8a"},
                {"lucek", "ff29a82b9ea498210089965fb5216806"}
            };
        }

        public bool PasswordValid(string userName, string password)
        {
            _users.TryGetValue(userName, out var passwordHashed);
            return passwordHashed == HashMd5(password);
        }


        private static string HashMd5(string value)
        {
            var x = new MD5CryptoServiceProvider();
            var data = Encoding.ASCII.GetBytes(value);
            data = x.ComputeHash(data);

            var sBuilder = new StringBuilder();

            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}