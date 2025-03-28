using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Lab2
{
    class LFSRAlgorythm
    {
        public int FullKeyLength { get { return _fullKeyLength; } }
        byte[]? _key;
        byte[]? _fullKey;
        string _fullKeyString = "";
        int _fullKeyLength;
        byte[]? _text;
        byte[] _result;
        Register register;
        readonly Dictionary<char, byte> _converterTyByte = new() { { '0', 0 }, { '1', 1 } };

        public string GenerateFullKey(string key, int length)
        {
            _key = ParseKey(key);
            register = new Register(_key);
            _fullKeyLength = length;
            int fullKeyBytesLength = length % 8 == 0 ? length / 8 : length / 8 + 1;
            _fullKey = new byte[fullKeyBytesLength];
            for (int bytesI = 0; bytesI < fullKeyBytesLength; bytesI++)
            {
                byte nextByte = register.GetNext();
                _fullKey[bytesI] = nextByte;
            }
            _fullKeyString = "";
            if (fullKeyBytesLength > 20)
            {
                for (int i = 0; i < 9; i++)
                {
                    int mask = 0b10000000;
                    for (int j = 0; j < 8; j++)
                    {
                        _fullKeyString += (_fullKey[i] & mask) == 0 ? "0" : "1";
                        mask >>= 1;
                    }
                }

                int mask1 = 0b10000000;
                for (int i = 0; i < 6; i++)
                {
                    _fullKeyString += (_fullKey[9] & mask1) == 0 ? "0" : "1";
                    mask1 >>= 1;
                }

                _fullKeyString += "...";

                mask1 = 0b00100000;
                for (int i = 0; i < 6; i++)
                {
                    _fullKeyString += (_fullKey[_fullKey.Length - 9] & mask1) == 0 ? "0" : "1";
                    mask1 >>= 1;
                }

                for (int i = _fullKey.Length - 9; i < _fullKey.Length; i++)
                {
                    int mask = 0b10000000;
                    for (int j = 0; j < 8; j++)
                    {
                        _fullKeyString += (_fullKey[i] & mask) == 0 ? "0" : "1";
                        mask >>= 1;
                    }
                }
                return _fullKeyString;
                /*int mask = 0b10000000;
                while (mask > 0 && i < length)
                {
                    _fullKeyString += (nextByte & mask) > 0 ? "1" : "0";
                    mask >>= 1;
                    i++;
                }*/
            }
            else
            {
                for (int i = 0; i < fullKeyBytesLength; i++)
                {
                    int mask = 0b10000000;
                    for (int j = 0; j < 8; j++)
                    {
                        _fullKeyString += (_fullKey[i] & mask) == 0 ? "0" : "1";
                        mask >>= 1;
                    }
                }
            }
                return _fullKeyString;
        }

        private byte[]? ParseKey(string key)
        {
            int length = key.Length / 8 + (key.Length % 8 > 0 ? 1 : 0);
            byte[] result = new byte[length];
            int stringPosition = key.Length - 1;
            int position = 0;
            for (int i = MainWindow.KEY_LENGTH; stringPosition >= 0; stringPosition--)
            {
                result[i / 8] |= (byte)(_converterTyByte[key[stringPosition]] << position);
                position++;
                if (position > 7)
                {
                    position = 0;
                }
                i--;
            }
            return result;
        }

        public byte[] Encrypt(byte[] textBytes)
        {
            //_fullKey = ParseString(_fullKeyString);
            if (textBytes != null && _fullKey != null)
            {
                _result = new byte[textBytes.Length];
                for (int i = 0; i < textBytes.Length; i++)
                {
                    _result[i] = (byte)(textBytes[i] ^ _fullKey[i]);
                }
            }
            return _result;
        }

        static byte[] ParseString(string text)
        {
            int bytesLength = text.Length % 8 == 0 ? text.Length / 8 : text.Length / 8 + 1;
            byte[] bytes = new byte[bytesLength];
            int biteNum = text.Length;
            for (int byteI = bytesLength - 1; byteI >= 0; byteI--)
            {
                for (int position = 0; position < 8 && --biteNum >= 0; position++)
                {
                    bytes[byteI] |= (byte)((text[biteNum] == '1' ? 1 : 0) << position);
                }
            }
            return bytes;
        }
    }
}