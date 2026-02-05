using System.Security.Cryptography;
using System.Text;
using UserService.Application.Abstractions.Security;

namespace UserService.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private string GetHash( string input)
    {
        var hashAlgorithm = SHA256.Create();
        // Convert the input string to a byte array and compute the hash.
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        var sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    public bool VerifyPasswordHash(string password, string passwordHash)
    {
        var hashOfInput = GetHash(password);

        // Create a StringComparer an compare the hashes.
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        return comparer.Compare(hashOfInput, passwordHash) == 0;
    }

    public string GenerateHash(string password)
    {
        string hash = GetHash(password);

        return hash;
    }
}