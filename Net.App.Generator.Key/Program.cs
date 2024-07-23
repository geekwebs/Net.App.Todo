using System;
using System.Security.Cryptography;

public class Program
{
    public static void Main()
    {
        long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var aesKey = GenerateKey();
        Console.WriteLine($"AES Key: {aesKey.Key}");
        Console.WriteLine($"AES IV: {aesKey.IV}");
        Console.WriteLine($"Timestamp: {unixTimestamp}");
    }

    public static (string Key, string IV) GenerateKey()
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();

            string key = Convert.ToHexString(aes.Key);
            string iv = Convert.ToHexString(aes.IV);

            return (key, iv);
        }
    }
}