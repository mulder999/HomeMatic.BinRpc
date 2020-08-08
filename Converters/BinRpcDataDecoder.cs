using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HomeMaticBinRpc.Converters
{
    public class BinRpcDataDecoder
    {

        #region Members

        private readonly Stream stream;
        private readonly Encoding encoding = Encoding.ASCII;

        #endregion

        #region Constructors

        public BinRpcDataDecoder(Stream stream)
        {
            this.stream = stream;
        }

        #endregion

        #region Public Methods

        public IHomeMaticMessage DecodeMessage()
        {
            string prefix = ReadStringFixedLength(3);
            if (prefix != BinRpcDataEncoder.c_bin)
            {
                throw new ApplicationException($"Unknown prefix '${prefix}'");
            }

            var command = (BinRpcCommandType)ReadByte();
            int messageLength = ReadInteger();
            return command switch
            {
                BinRpcCommandType.Method => DecodeMethod(),
                BinRpcCommandType.Response => DecodeResponse(),
                BinRpcCommandType.Error => DecodeError(),
                _ => throw new NotSupportedException(command.ToString()),
            };
        }

        #endregion

        #region Private Methods

        private HomeMaticMessageRpc DecodeMethod()
        {
            return new HomeMaticMessageRpc()
            {
                Method = (string)DecodeData(),
                Parameters = (object[])DecodeData(),
            };
        }

        private HomematicMessageResponse DecodeResponse()
        {
            return new HomematicMessageResponse()
            {
                Response = DecodeData()
            };
        }

        private HomematicMessageError DecodeError()
        {
            var result = (Dictionary<string, object>)DecodeData();
            return new HomematicMessageError()
            {
                FaultCode = (int)result["faultCode"],
                FaultString = (string)result["faultString"]
            };
        }


        private object DecodeData()
        {
            var code = (BinRpcDataType)ReadInteger();
            switch (code)
            {
                case BinRpcDataType.Array:
                    return ReadArray();
                case BinRpcDataType.Bool:
                    return ReadBool();
                case BinRpcDataType.Double:
                    return ReadDouble();
                case BinRpcDataType.Integer:
                    return ReadInteger();
                case BinRpcDataType.String:
                    return ReadString();
                case BinRpcDataType.Struct:
                    return ReadStruct();
                default:
                    throw new NotSupportedException(code.ToString());
            }
        }

        private string ReadStringFixedLength(int len)
        {
            var buffer = new byte[len];
            int len2 = stream.Read(buffer, 0, len);
            if (len != len2)
            {
                throw new EndOfStreamException();
            }
            return encoding.GetString(buffer);
        }

        private string ReadString()
        {
            int len = ReadInteger();
            return ReadStringFixedLength(len);
        }

        private byte ReadByte()
        {
            var buffer = new byte[1];
            int len = stream.Read(buffer, 0, buffer.Length);

            if (len <= 0)
            {
                throw new EndOfStreamException();
            }
            return buffer[0];
        }

        private bool ReadBool()
        {
            byte val = ReadByte();
            return val > 0;
        }

        private int ReadInteger()
        {
            var buffer = new byte[4];
            var len = stream.Read(buffer, 0, buffer.Length);
            if (len != buffer.Length)
            {
                throw new EndOfStreamException();
            }
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            return BitConverter.ToInt32(buffer);
        }

        private double ReadDouble()
        {
            var mant = ReadInteger();
            var exp = ReadInteger();
            return Math.Pow(2, exp) * (mant / (1 << 30));
        }

        private object[] ReadArray()
        {
            var len = ReadInteger();
            var arr = new object[len];
            for (int i = 0; i < len; i++)
            {
                arr[i] = DecodeData();
            }
            return arr;
        }

        private Dictionary<string, object> ReadStruct()
        {
            var len = ReadInteger();
            var map = new Dictionary<string, object>(len);

            for (int i = 0; i < len; i++)
            {
                var key = ReadString();
                var value = DecodeData();
                map.Add(key, value);
            }

            return map;
        }

        #endregion
    }
}
