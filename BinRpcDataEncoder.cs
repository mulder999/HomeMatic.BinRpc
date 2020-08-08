using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HomeMaticBinRpc
{
    public class BinRpcDataEncoder : IDisposable
    {
        #region Private Members

        public const string c_bin = "Bin";

        private readonly MemoryStream ms = new MemoryStream();
        private readonly StreamWriter sw;
        private bool disposedValue;

        #endregion

        #region Properties

        public byte[] Buffer { get => ms.GetBuffer(); }

        #endregion

        #region Constructors

        public BinRpcDataEncoder()
        {
            sw = new StreamWriter(ms, Encoding.ASCII);
        }

        #endregion

        #region Public Methods

        public void EncodeRequest(string method, params object[] data)
        {
            method = method ?? throw new ArgumentNullException(nameof(method));

            EncodeMessage(BinRpcCommandType.Method, () =>
            {
                EncodeString(method);
                EncodeArray(data);
            });
        }

        public void EncodeResponse(object data)
        {
            EncodeMessage(BinRpcCommandType.Response, () =>
            {
                EncodeData(data);
            });
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sw.Dispose();
                    ms.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        private void EncodeMessage(BinRpcCommandType commandType, Action messageAction)
        {
            var startIndex = sw.BaseStream.Position;

            sw.Write(c_bin);
            sw.Write(commandType);

            var sizeIndex = sw.BaseStream.Position;
            int messageSize = 0; // This will be updated at the end
            sw.Write(messageSize);

            messageAction();
            sw.Flush();

            // Compute message size
            var stopIndex = sw.BaseStream.Position;
            messageSize = (int)(stopIndex - startIndex);
            sw.BaseStream.Seek(sizeIndex, SeekOrigin.Begin);
            sw.Write(messageSize);
            sw.Flush();

            sw.BaseStream.Seek(stopIndex, SeekOrigin.Begin);
        }

        private void EncodeData(object obj)
        {
            var type = obj.GetType();
            var code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Boolean:
                    EncodeBool((bool)obj);
                    break;
                case TypeCode.Decimal:
                    EncodeDouble((double)(decimal)obj);
                    break;
                case TypeCode.Single:
                    EncodeDouble((float)obj);
                    break;
                case TypeCode.Double:
                    EncodeDouble((double)obj);
                    break;
                case TypeCode.Char:
                    var c = (char)obj;
                    EncodeString(c.ToString());
                    break;
                case TypeCode.String:
                    EncodeString((string)obj);
                    break;
                case TypeCode.Object:
                    if (type.IsArray)
                    {
                        EncodeArray((object[])obj);
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        var enumerable = (IEnumerable)obj;
                        var arr = enumerable.Cast<object>().ToArray();
                        EncodeArray(arr);
                    }
                    else
                    {
                        EncodeStruct(obj);
                    }
                    break;
                case TypeCode.Byte:
                    EncodeInteger((byte)obj);
                    break;
                case TypeCode.SByte:
                    EncodeInteger((sbyte)obj);
                    break;
                case TypeCode.Int16:
                    EncodeInteger((short)obj);
                    break;
                case TypeCode.UInt16:
                    EncodeInteger((ushort)obj);
                    break;
                case TypeCode.Int32:
                    EncodeInteger((int)obj);
                    break;

                default:
                    throw new NotSupportedException(code.ToString());
            }
        }

        private void EncodeStruct(object obj)
        {
            var type = obj.GetType();
            var map = type.GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(obj));
            EncodeStruct(map);
        }

        private void EncodeStruct(Dictionary<string, object> struc)
        {
            sw.Write(BinRpcDataType.Struct);
            sw.Write(struc.Count);
            foreach(var kv in struc)
            {
                EncodeStructKey(kv.Key);
                EncodeData(kv.Value);
            }
        }

        private void EncodeStructKey(string key)
        {
            sw.Write(key.Length);
            sw.Write(key);
        }

        private void EncodeArray(object[] arr)
        {
            sw.Write(BinRpcDataType.Array);
            sw.Write(arr.Length);
            foreach(var el in arr)
            {
                EncodeData(el);
            }
        }

        private void EncodeString(string str)
        {
            sw.Write(BinRpcDataType.String);
            sw.Write(str.Length);
            sw.Write(str);

        }

        private void EncodeBool(bool b)
        {
            sw.Write(BinRpcDataType.Bool);
            sw.Write(b ? (byte)1 : (byte)0);

        }

        private void EncodeInteger(int i)
        {
            sw.Write(BinRpcDataType.Integer);
            sw.Write(i);
        }

        private void EncodeDouble(double d)
        {
            var exp = Math.Floor(Math.Log(Math.Abs(d)) / Math.Log(2)) + 1;
            var man = Math.Floor((d * Math.Pow(2, -exp)) * (1 << 30));

            sw.Write(BinRpcDataType.Double);
            sw.Write((int)man);
            sw.Write((int)exp);
        }
        #endregion
    }
}
