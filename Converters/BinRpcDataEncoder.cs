using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeMaticBinRpc.Converters
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
            sw = new StreamWriter(ms, Encoding.ASCII)
            {
                AutoFlush = true
            };
        }

        #endregion

        #region Public Methods

        public void EncodeRequest(string method, params object[] data)
        {
            method = method ?? throw new ArgumentNullException(nameof(method));

            EncodeMessage(BinRpcCommandType.Method, () =>
            {
                EncodeString(method, skipPrefix: true);
                EncodeArray(data, skipPrefix: true);
            });
        }

        public void EncodeResponse(object data)
        {
            EncodeMessage(BinRpcCommandType.Response, () =>
            {
                EncodeData(data);
            });
        }

        public async Task Write(Stream stream)
        {
            await sw.FlushAsync();
            await stream.WriteAsync(ms.GetBuffer(), 0, (int)ms.Length);
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

            WriteString(c_bin);
            Write8((byte)commandType);

            var sizeIndex = sw.BaseStream.Position;
            int messageSize = 0; // This will be updated at the end
            Write32(messageSize);

            messageAction();
            sw.Flush();

            // Compute message size
            var stopIndex = sw.BaseStream.Position;
            messageSize = (int)(stopIndex - startIndex);
            sw.BaseStream.Seek(sizeIndex, SeekOrigin.Begin);
            Write32(messageSize);
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
            WriteType(BinRpcDataType.Struct);
            Write32(struc.Count);
            foreach (var kv in struc)
            {
                EncodeStructKey(kv.Key);
                EncodeData(kv.Value);
            }
        }

        private void EncodeStructKey(string key)
        {
            Write32(key.Length);
            WriteString(key);
        }

        private void EncodeArray(object[] arr, bool skipPrefix = false)
        {
            if (!skipPrefix)
            {
                WriteType(BinRpcDataType.Array);
            }
            Write32(arr.Length);
            foreach (var el in arr)
            {
                EncodeData(el);
            }
        }

        private void EncodeString(string str, bool skipPrefix = false)
        {
            if (!skipPrefix)
            {
                WriteType(BinRpcDataType.String);
            }
            Write32(str.Length);
            WriteString(str);
        }

        private void EncodeBool(bool b)
        {
            WriteType(BinRpcDataType.Bool);
            byte val = (byte)(b ? 1 : 0);
            Write8(val);
        }

        private void EncodeInteger(int i)
        {
            WriteType(BinRpcDataType.Integer);
            Write32(i);
        }

        private void EncodeDouble(double d)
        {
            var exp = Math.Floor(Math.Log(Math.Abs(d)) / Math.Log(2)) + 1;
            var man = Math.Floor(d * Math.Pow(2, -exp) * (1 << 30));

            WriteType(BinRpcDataType.Double);
            Write32((int)man);
            Write32((int)exp);
        }

        private void WriteType(BinRpcDataType type)
        {
            Write32((int)type);
        }

        private void Write8(byte value)
        {
            sw.BaseStream.WriteByte(value);
        }

        private void Write32(int value)
        {
            var buffer = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            sw.BaseStream.Write(buffer, 0, buffer.Length);
        }

        private void WriteString(string str)
        {
            sw.Write(str);
        }
        #endregion
    }
}
