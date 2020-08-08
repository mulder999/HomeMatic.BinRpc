using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace HomeMaticBinRpc
{
    public class BinRpcDataDecoder
    {

        #region Members

        private readonly MemoryStream ms = new MemoryStream();
        private readonly StreamReader sr;

        #endregion

        #region Constructors

        public BinRpcDataDecoder()
        {
            sr = new StreamReader(ms, Encoding.ASCII);
        }

        #endregion

        #region Public Methods

        public IHomeMaticMessage DecodeMessage()
        {
            string prefix = ReadLength(3);
            if (prefix != BinRpcDataEncoder.c_bin)
            {
                throw new ApplicationException($"Unknown prefix '${prefix}'");
            }

            var command = (BinRpcCommandType)sr.Read();
            int messageLength = ReadInteger();
            return command switch
            {
                BinRpcCommandType.Method => DecodeMethod(),
                BinRpcCommandType.Response => DecodeResponse(),
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


        private object DecodeData()
        {
            var code = (BinRpcDataType)ReadInteger();
            switch(code)
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

        private string ReadLength(int len)
        {
            var buffer = new char[len];
            sr.ReadBlock(buffer, 0, len);
            return new string(buffer);
        }

        private string ReadString()
        {
            int len = ReadInteger();
            return ReadLength(len);
        }

        private bool ReadBool()
        {
            var obj = sr.Read();
            return obj > 0;
        }

        private int ReadInteger()
        {
            var buffer = new byte[4];
            sr.BaseStream.Read(buffer, 0, buffer.Length);
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

            for(int i = 0; i < len; i++)
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
