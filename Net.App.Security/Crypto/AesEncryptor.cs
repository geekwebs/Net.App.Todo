using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace Net.App.Security.Crypto;
public class AesEncryptor
{
    private readonly string Key;
    private readonly string Iv;

    public AesEncryptor(string Key, string Iv)
    {
        this.Key = Key ?? throw new ArgumentNullException(nameof(Key));;
        this.Iv = Iv ?? throw new ArgumentNullException(nameof(Iv));
    }

    public string Encrypt(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var plainTextBytes = Encoding.UTF8.GetBytes(json);

        using var aes = Aes.Create();
        aes.Key = Convert.FromHexString(Key);
        aes.IV  = Convert.FromHexString(Iv); 

        using var encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(plainTextBytes, 0, plainTextBytes.Length);
            cs.FlushFinalBlock();
        }

        var encrypted = ms.ToArray();
        return Convert.ToHexString(encrypted);
    }

    public T Decrypt<T>(string secureText) {
        if (string.IsNullOrEmpty(secureText))
        {
            throw new ArgumentException("Secure text cannot be null or empty", nameof(secureText));
        }

        try
        {
            var fullCipher = Convert.FromHexString(secureText);

            using var aes = Aes.Create();
            aes.Key = Convert.FromHexString(Key);
            aes.IV = Convert.FromHexString(Iv);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(fullCipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);

            var decrypted = reader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(decrypted);
        }
        catch (JsonException ex)
        {
            // Log the exception or handle it as needed
            throw new CryptographicException("Error decrypting the message", ex);
        }
    }

}
