using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace TimeId.Operations;

public static class TwoFactor
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    public static string NewSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20); var output = new StringBuilder();
        int buffer = 0, bits = 0;
        foreach (byte b in bytes) { buffer = (buffer << 8) | b; bits += 8; while (bits >= 5) { bits -= 5; output.Append(Alphabet[(buffer >> bits) & 31]); } }
        return output.ToString();
    }
    private static byte[] Decode(string secret)
    {
        var output = new List<byte>(); int buffer = 0, bits = 0;
        foreach (char c in secret) { int index = Alphabet.IndexOf(c); if (index < 0) throw new FormatException("Chave inválida."); buffer = (buffer << 5) | index; bits += 5; if (bits >= 8) { bits -= 8; output.Add((byte)(buffer >> bits)); } }
        return output.ToArray();
    }
    public static string Code(string secret, long step)
    {
        Span<byte> counter = stackalloc byte[8]; BinaryPrimitives.WriteInt64BigEndian(counter, step);
        byte[] hash = HMACSHA1.HashData(Decode(secret), counter);
        int offset = hash[^1] & 15;
        int value = ((hash[offset] & 127) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (value % 1000000).ToString("D6");
    }
    public static bool Verify(string secret, string? code)
    {
        if (code is null || code.Length != 6 || !code.All(char.IsAsciiDigit)) return false;
        long step = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        return Enumerable.Range(-1, 3).Any(offset => CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Code(secret, step + offset)), Encoding.ASCII.GetBytes(code)));
    }
    public static string RecoveryHash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code.Replace("-", "").Trim().ToUpperInvariant())));
    public static bool VerifyAccount(Account account, string? code)
    {
        if (account.TwoFactorSecret is null) return true;
        if (Verify(account.TwoFactorSecret, code?.Trim())) return true;
        if (string.IsNullOrWhiteSpace(code)) return false;
        string hash = RecoveryHash(code);
        return account.RecoveryHashes.Remove(hash);
    }
}
