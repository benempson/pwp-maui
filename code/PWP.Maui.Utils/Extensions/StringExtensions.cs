using System.Security.Cryptography;
using System.Text;

namespace PWP.Maui.Utils.Extensions;

public static class StringExtensions
{
    private static readonly char[] Base32Alphabet = "abcdefghijklmnopqrstuvwxyz234567".ToCharArray();

    #region ToBase32
    /// <summary>
    /// Converts the given byte array to a Base32 string
    /// </summary>
    /// <param name="data">The data to convert</param>
    /// <returns>Base32 encoded version of the input data</returns>
    public static string ToBase32(this byte[] data)
    {
        int index1 = 0;
        int num1 = 0;
        StringBuilder stringBuilder = new StringBuilder();
        while (index1 < data.Length)
        {
            int num2 = (int)data[index1];
            int index2;
            if (num1 > 3)
            {
                int num3 = 0;
                if (index1 + 1 < data.Length)
                    num3 = (int)data[index1 + 1];
                int num4 = num2 & (int)byte.MaxValue >> num1;
                num1 = (num1 + 5) % 8;
                index2 = num4 << num1 | num3 >> 8 - num1;
                ++index1;
            }
            else
            {
                index2 = num2 >> 8 - (num1 + 5) & 31;
                num1 = (num1 + 5) % 8;
                if (num1 == 0)
                    ++index1;
            }
            stringBuilder.Append(StringExtensions.Base32Alphabet[index2]);
        }

        return stringBuilder.ToString();
    }
    #endregion

    #region ToBase64
    /// <summary>
    /// Converts a byte array to a base64 (UTF8 by default) encoded string.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="encoding">Default to UTF8</param>
    /// <returns>The Base64 encoded version of the input string</returns>
    public static string? ToBase64(this byte[] text, Encoding? encoding = null)
    {
        if (text == null)
            return null;

        encoding = encoding ?? Encoding.UTF8;
        string result = encoding.GetString(text);
        var preamble = encoding.GetString(encoding.GetPreamble());
        if (result.StartsWith(preamble))
            result = result.Remove(0, preamble.Length);

        return result;
    }
    #endregion

    #region ToSHA1Hash
    /// <summary>
    /// Get the SHA1 hash value of the input string
    /// </summary>
    /// <param name="value">The input string</param>
    /// <param name="base32Encode">Whether or not to Base32 encode the resulting hash</param>
    /// <param name="maximumHashCharacters">The maximum number of characters you want in the resulting hash</param>
    /// <returns>The hashed version of the input string</returns>
    public static string ToSHA1Hash(this string value, bool base32Encode = true, int maximumHashCharacters = 24)
    {
        value = value ?? string.Empty;
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        byte[] hash;

        using (SHA1 sha1 = SHA1.Create())
            hash = sha1.ComputeHash(bytes);

        if (base32Encode)
            return hash.ToBase32().Substring(0, maximumHashCharacters);

        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte num in hash)
            stringBuilder.Append(num.ToString("X2"));

        return stringBuilder.ToString().ToLower().Substring(0, maximumHashCharacters);
    }
    #endregion
}
