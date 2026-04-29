namespace SteamUtility.Core.Vdf;

public static class SimpleVdfReader
{
    public static VdfObject Parse(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var tokenizer = new Tokenizer(content);
        var root = new VdfObject("__root__");

        while (tokenizer.TryReadString(out var key))
        {
            if (tokenizer.TryPeekOpenBrace())
            {
                tokenizer.ReadOpenBrace();
                var child = ReadObject(tokenizer, key);
                root.AddChild(key, child);
            }
            else if (tokenizer.TryReadString(out var value))
            {
                root.AddValue(key, value);
            }
            else
            {
                throw new FormatException($"Unexpected token after key '{key}'.");
            }
        }

        return root;
    }

    private static VdfObject ReadObject(Tokenizer tokenizer, string name)
    {
        var obj = new VdfObject(name);

        while (true)
        {
            tokenizer.SkipWhitespace();

            if (tokenizer.TryReadCloseBrace())
            {
                return obj;
            }

            if (!tokenizer.TryReadString(out var key))
            {
                throw new FormatException($"Unexpected end of VDF object '{name}'.");
            }

            if (tokenizer.TryPeekOpenBrace())
            {
                tokenizer.ReadOpenBrace();
                obj.AddChild(key, ReadObject(tokenizer, key));
                continue;
            }

            if (!tokenizer.TryReadString(out var value))
            {
                throw new FormatException($"Expected value for key '{key}' in '{name}'.");
            }

            obj.AddValue(key, value);
        }
    }

    private sealed class Tokenizer(string content)
    {
        private readonly string _content = content;
        private int _index;

        public void SkipWhitespace()
        {
            while (_index < _content.Length)
            {
                var ch = _content[_index];
                if (!char.IsWhiteSpace(ch))
                {
                    break;
                }

                _index++;
            }
        }

        public bool TryPeekOpenBrace()
        {
            SkipWhitespace();
            return _index < _content.Length && _content[_index] == '{';
        }

        public void ReadOpenBrace()
        {
            SkipWhitespace();
            if (_index >= _content.Length || _content[_index] != '{')
            {
                throw new FormatException("Expected '{'.");
            }

            _index++;
        }

        public bool TryReadCloseBrace()
        {
            SkipWhitespace();
            if (_index < _content.Length && _content[_index] == '}')
            {
                _index++;
                return true;
            }

            return false;
        }

        public bool TryReadString(out string value)
        {
            SkipWhitespace();
            value = string.Empty;

            if (_index >= _content.Length || _content[_index] != '"')
            {
                return false;
            }

            _index++;
            var start = _index;

            while (_index < _content.Length)
            {
                if (_content[_index] == '"')
                {
                    var backslashCount = 0;
                    var j = _index - 1;
                    while (j >= start && _content[j] == '\\')
                    {
                        backslashCount++;
                        j--;
                    }

                    if (backslashCount % 2 == 0)
                    {
                        value = _content[start.._index];
                        _index++;
                        return true;
                    }
                }

                _index++;
            }

            throw new FormatException("Unterminated quoted string in VDF content.");
        }
    }
}
