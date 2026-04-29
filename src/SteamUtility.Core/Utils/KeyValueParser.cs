namespace SteamUtility.Core.Utils;

public enum KeyValueType
{
    None = 0,
    String = 1,
    Int32 = 2,
    Float32 = 3,
    Pointer = 4,
    WideString = 5,
    Color = 6,
    UInt64 = 7,
    End = 8
}

public sealed class KeyValue
{
    private static readonly KeyValue InvalidNode = new();

    public string Name { get; set; } = "<root>";

    public KeyValueType Type { get; set; } = KeyValueType.None;

    public object? Value { get; set; }

    public bool Valid { get; set; }

    public List<KeyValue>? Children { get; set; }

    public KeyValue this[string key]
    {
        get
        {
            if (Children is null)
            {
                return InvalidNode;
            }

            return Children.FirstOrDefault(child =>
                string.Equals(child.Name, key, StringComparison.InvariantCultureIgnoreCase)) ?? InvalidNode;
        }
    }

    public string AsString(string defaultValue)
    {
        return !Valid || Value is null ? defaultValue : Value.ToString() ?? defaultValue;
    }

    public int AsInteger(int defaultValue)
    {
        if (!Valid)
        {
            return defaultValue;
        }

        return Type switch
        {
            KeyValueType.String or KeyValueType.WideString => int.TryParse(Value?.ToString(), out var value)
                ? value
                : defaultValue,
            KeyValueType.Int32 => (int)Value!,
            KeyValueType.Float32 => (int)(float)Value!,
            KeyValueType.UInt64 => (int)((ulong)Value! & 0xFFFFFFFF),
            _ => defaultValue
        };
    }

    public bool AsBoolean(bool defaultValue)
    {
        if (!Valid)
        {
            return defaultValue;
        }

        return Type switch
        {
            KeyValueType.String or KeyValueType.WideString => int.TryParse(Value?.ToString(), out var value)
                ? value != 0
                : defaultValue,
            KeyValueType.Int32 => (int)Value! != 0,
            KeyValueType.Float32 => (int)(float)Value! != 0,
            KeyValueType.UInt64 => (ulong)Value! != 0,
            _ => defaultValue
        };
    }

    public float AsFloat(float defaultValue)
    {
        if (!Valid)
        {
            return defaultValue;
        }

        return Type switch
        {
            KeyValueType.String or KeyValueType.WideString => float.TryParse(Value?.ToString(), out var value)
                ? value
                : defaultValue,
            KeyValueType.Int32 => (int)Value!,
            KeyValueType.Float32 => (float)Value!,
            KeyValueType.UInt64 => (ulong)Value!,
            _ => defaultValue
        };
    }

    public static KeyValue? LoadAsBinary(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            using var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var keyValue = new KeyValue();
            return keyValue.ReadAsBinary(input) ? keyValue : null;
        }
        catch
        {
            return null;
        }
    }

    public bool ReadAsBinary(Stream input)
    {
        Children = [];

        try
        {
            while (true)
            {
                var type = (KeyValueType)ReadValueU8(input);
                if (type == KeyValueType.End)
                {
                    break;
                }

                var current = new KeyValue
                {
                    Type = type,
                    Name = ReadStringUnicode(input)
                };

                switch (type)
                {
                    case KeyValueType.None:
                        current.ReadAsBinary(input);
                        break;
                    case KeyValueType.String:
                        current.Valid = true;
                        current.Value = ReadStringUnicode(input);
                        break;
                    case KeyValueType.WideString:
                        throw new FormatException("wstring is unsupported");
                    case KeyValueType.Int32:
                        current.Valid = true;
                        current.Value = ReadValueS32(input);
                        break;
                    case KeyValueType.UInt64:
                        current.Valid = true;
                        current.Value = ReadValueU64(input);
                        break;
                    case KeyValueType.Float32:
                        current.Valid = true;
                        current.Value = ReadValueF32(input);
                        break;
                    case KeyValueType.Color:
                    case KeyValueType.Pointer:
                        current.Valid = true;
                        current.Value = ReadValueU32(input);
                        break;
                    default:
                        throw new FormatException();
                }

                if (input.Position >= input.Length)
                {
                    throw new FormatException();
                }

                Children.Add(current);
            }

            Valid = true;
            return input.Position == input.Length;
        }
        catch
        {
            return false;
        }
    }

    private static byte ReadValueU8(Stream input) => (byte)input.ReadByte();

    private static int ReadValueS32(Stream input)
    {
        Span<byte> data = stackalloc byte[4];
        input.ReadExactly(data);
        return BitConverter.ToInt32(data);
    }

    private static uint ReadValueU32(Stream input)
    {
        Span<byte> data = stackalloc byte[4];
        input.ReadExactly(data);
        return BitConverter.ToUInt32(data);
    }

    private static ulong ReadValueU64(Stream input)
    {
        Span<byte> data = stackalloc byte[8];
        input.ReadExactly(data);
        return BitConverter.ToUInt64(data);
    }

    private static float ReadValueF32(Stream input)
    {
        Span<byte> data = stackalloc byte[4];
        input.ReadExactly(data);
        return BitConverter.ToSingle(data);
    }

    private static string ReadStringUnicode(Stream input)
    {
        var bytes = new List<byte>();

        while (true)
        {
            var next = input.ReadByte();
            if (next == 0)
            {
                break;
            }

            bytes.Add((byte)next);
        }

        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }
}
