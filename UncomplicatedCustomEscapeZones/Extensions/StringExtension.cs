namespace UncomplicatedEscapeZones.Extensions;

public static class StringExtension
{
    public static string GenerateWithBuffer(this string str, int bufferSize)
    {
        for (int a = str.Length; a < bufferSize; a++)
            str += " ";

        return str;
    }
}