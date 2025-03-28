using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2
{
    class Register
    {
        byte[] _polynom = [0b01000000, 0b00000000, 0b00000000, 0b00000000, 0b00001000];
        byte[] _registerState;
        public Register(byte[] registerState)
        {
            _registerState = registerState.ToArray();
        }
        public byte GetNext()
        {
            byte result = 0;
            int resultMask = 0b01000000;
            for (int i = 0; i < 7; i++)
            {
                result += (byte)((_registerState[0] & resultMask) << 1);
                resultMask >>= 1;
            }
            result += (byte)((_registerState[1] & 0b10000000) >> 7);
            byte newFirst;
            byte mask;
            byte position;
            for (int biteNumber = 0; biteNumber < 8; biteNumber++) {
                mask = 0b01000000;
                newFirst = (byte)(((_registerState[0] & 0b01000000) >> 6) ^ ((_registerState[_registerState.Length - 1] & 0b00001000) >> 3));   //(byte)(((_polynom[0] & mask) >> 6) & ((_registerState[0] & mask) >> 6));
                /*position = 5;
                mask >>= 1;
                for (int i = 2; i < MainWindow.KEY_LENGTH; i++)
                {
                    newFirst ^= (byte)(((_polynom[i / 8] & mask) >> position) & ((_registerState[i / 8] & mask) >> position));
                    mask = (byte)(((mask >> 1) == 0) ? 0b10000000 : mask >> 1);
                    position = (byte)(position == 0 ? position = 7 : position--);
                }*/
                byte carry = 0, newCarry;

                for (int i = _registerState.Length - 1; i >= 0; i--)
                {
                    newCarry = (byte)((_registerState[i] & 0b10000000) >> 7);
                    _registerState[i] <<= 1;
                    _registerState[i] |= carry;
                    carry = newCarry;
                }
                _registerState[_registerState.Length - 1] = (byte)(_registerState[_registerState.Length - 1] & 0b11111110 | newFirst);
            }

            return result;
            //byte aAndb;
            //for (int i = 0; i < _registerState.Length; i++)
            //{
            //    aAndb = (byte)(_registerState[i] & _polynom[i]);
            //    byte mask = 0b00000001;
            //    for (int j = 0; j < sizeof(byte); j++) 
            //    {
            //        newFirst ^= (byte)(aAndb & mask);
            //        mask = (byte)(mask << 1);
            //    }
            //}
        }
    }
}