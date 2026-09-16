using System.Security.Cryptography;
using System.Text;

namespace Alphabet.Shared.Helpers;

public static class HashHelper
{
    public static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
