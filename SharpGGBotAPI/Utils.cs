using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGGBotAPI
{
    /// <summary>
    /// Narzędzia pomocnicze.
    /// </summary>
    public class Utils
    {
        //SOURCE: http://www.sanity-free.com/12/crc32_implementation_in_csharp.html
        private static class Crc32
        {
            private static uint[] table;

            public static uint ComputeChecksum(byte[] bytes)
            {
                uint crc = 0xffffffff;
                for (int i = 0; i < bytes.Length; ++i)
                {
                    byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                    crc = (uint)((crc >> 8) ^ table[index]);
                }
                return ~crc;
            }
            public static byte[] ComputeChecksumBytes(byte[] bytes)
            {
                return BitConverter.GetBytes(ComputeChecksum(bytes));
            }

            static Crc32()
            {
                uint poly = 0xedb88320;
                table = new uint[256];
                uint temp = 0;
                for (uint i = 0; i < table.Length; ++i)
                {
                    temp = i;
                    for (int j = 8; j > 0; --j)
                    {
                        if ((temp & 1) == 1)
                        {
                            temp = (uint)((temp >> 1) ^ poly);
                        }
                        else
                        {
                            temp >>= 1;
                        }
                    }
                    table[i] = temp;
                }
            }
        }
        /// <summary>
        /// Oblicza sumę kontrolną CRC32 z podanych danych.
        /// </summary>
        /// <param name="data">Dane.</param>
        /// <returns>Suma kontrolna CRC32.</returns>
        public static long ComputeCrc32(byte[] data)
        {
            return Crc32.ComputeChecksum(data);
        }
        /// <summary>
        /// Oblicz hash obrazka.
        /// </summary>
        /// <param name="crc32">Suma kontrolna CRC32.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        /// <returns>Hash obrazka.</returns>
        public static string ComputeHash(uint crc32, uint length)
        {
            return crc32.ToString("X8") + length.ToString("X8");
        }
        /// <summary>
        /// Parsuj hash obrazka.
        /// </summary>
        /// <param name="hash">Hash.</param>
        /// <param name="crc32">Suma kontrolna CRC32.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        public static void ParseImageHash(string hash, out uint crc32, out uint length)
        {
            crc32 = 0;
            length = 0;
            try
            {
                if (hash.Length != 16) throw new InvalidOperationException("Bad hash length");

                crc32 = Convert.ToUInt32(hash.Remove(8), 16);
                length = Convert.ToUInt32(hash.Remove(0, 8), 16);
            }
            catch { throw new InvalidOperationException("Bad hash"); }
        }

        internal static uint ToInternalStatus(Status status, bool description)
        {
            switch (status)
            {
                case Status.Advertising: return Container.BOTAPI_STATUS_ADVERTISING;
                case Status.Available: return (description ? Container.BOTAPI_STATUS_AVAILABLE_DESCR : Container.BOTAPI_STATUS_AVAILABLE);
                case Status.Busy: return (description ? Container.BOTAPI_STATUS_BUSY_DESCR : Container.BOTAPI_STATUS_BUSY);
                case Status.DoNotDisturb: return (description ? Container.BOTAPI_STATUS_DND_DESCR : Container.BOTAPI_STATUS_DND);
                case Status.FreeForCall: return (description ? Container.BOTAPI_STATUS_FFC_DESCR : Container.BOTAPI_STATUS_FFC);
                case Status.Invisible: return (description ? Container.BOTAPI_STATUS_INVISIBLE_DESCR : Container.BOTAPI_STATUS_INVISIBLE);
                case Status.None:
                default: return Container.BOTAPI_STATUS_CURRENT;
            }
        }
        internal static Status ToPublicStatus(uint status, out bool description)
        {
            description = false;
            switch (status)
            {
                case Container.BOTAPI_STATUS_ADVERTISING: return Status.Advertising;
                case Container.BOTAPI_STATUS_AVAILABLE: return Status.Available;
                case Container.BOTAPI_STATUS_AVAILABLE_DESCR: description = true; return Status.Available;
                case Container.BOTAPI_STATUS_BUSY: return Status.Busy;
                case Container.BOTAPI_STATUS_BUSY_DESCR: description = true; return Status.Busy;
                case Container.BOTAPI_STATUS_DND: return Status.DoNotDisturb;
                case Container.BOTAPI_STATUS_DND_DESCR: description = true; return Status.DoNotDisturb;
                case Container.BOTAPI_STATUS_FFC: return Status.FreeForCall;
                case Container.BOTAPI_STATUS_FFC_DESCR: description = true; return Status.FreeForCall;
                case Container.BOTAPI_STATUS_INVISIBLE: return Status.Invisible;
                case Container.BOTAPI_STATUS_INVISIBLE_DESCR: description = true; return Status.Invisible;
                case Container.BOTAPI_STATUS_CURRENT:
                default: return Status.None;
            }
        }
    }
}
