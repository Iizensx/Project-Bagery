using System.Text;

namespace _66022380.Helpers;

public static class PromptPayHelper
{
    public static string GeneratePayload(string phoneOrId, decimal amount = 0)
    {
        // ทำความสะอาดเบอร์โทร เช่น 0812345678 → 66812345678
        var target = phoneOrId.Trim().Replace("-", "").Replace(" ", "");
        if (target.StartsWith("0") && target.Length == 10)
            target = "0066" + target.Substring(1);

        var guid = "A000000677010111";
        var targetTag = FormatTag("01", target);
        var merchantInfo = FormatTag("00", guid) + FormatTag("01", targetTag);
        var payload = FormatTag("29", merchantInfo);

        var result = "000201" + payload + "5303764";

        if (amount > 0)
        {
            var amountStr = amount.ToString("F2");
            result += FormatTag("54", amountStr);
        }

        result += "5802TH6304";
        result += ComputeCRC(result);

        return result;
    }

    private static string FormatTag(string tag, string value)
    {
        return tag + value.Length.ToString("D2") + value;
    }

    private static string ComputeCRC(string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        ushort crc = 0xFFFF;
        foreach (var b in bytes)
        {
            crc ^= (ushort)(b << 8);
            for (int i = 0; i < 8; i++)
                crc = (crc & 0x8000) != 0
                    ? (ushort)((crc << 1) ^ 0x1021)
                    : (ushort)(crc << 1);
        }
        return crc.ToString("X4");
    }
}