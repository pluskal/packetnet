/*
This file is part of PacketDotNet

PacketDotNet is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PacketDotNet is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with PacketDotNet.  If not, see <http://www.gnu.org/licenses/>.
*/
/*
 *  Copyright 2010 Evan Plaice <evanplaice@gmail.com>
 *  Copyright 2010 Chris Morgan <chmorgan@gmail.com>
 */
using System;
using MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.LLDP
{
    /// <summary>
    /// A Time to Live TLV
    /// </summary>
    [Serializable]
    public class TimeToLive : TLV
    {
#if DEBUG
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#else
        // NOTE: No need to warn about lack of use, the compiler won't
        //       put any calls to 'log' here but we need 'log' to exist to compile
#pragma warning disable 0169, 0649
        private static readonly ILogInactive log;
#pragma warning restore 0169, 0649
#endif

        /// <summary>
        /// Number of bytes in the value portion of this tlv
        /// </summary>
        private const Int32 ValueLength = 2;

        #region Constructors

        /// <summary>
        /// Creates a TTL TLV
        /// </summary>
        /// <param name="bytes">
        /// </param>
        /// <param name="offset">
        /// The TTL TLV's offset from the
        /// origin of the LLDP
        /// </param>
        public TimeToLive(Byte[] bytes, Int32 offset) :
            base(bytes, offset)
        {
            log.Debug("");
        }

        /// <summary>
        /// Creates a TTL TLV and sets it value
        /// </summary>
        /// <param name="seconds">
        /// The length in seconds until the LLDP
        /// is refreshed
        /// </param>
        public TimeToLive(UInt16 seconds)
        {
            log.Debug("");

            var bytes = new Byte[TLVTypeLength.TypeLengthLength + ValueLength];
            Int32 offset = 0;
            Int32 length = bytes.Length;
            this.tlvData = new ByteArraySegment(bytes, offset, length);

            this.Type = TLVTypes.TimeToLive;
            this.Seconds = seconds;
        }

        #endregion

        #region Properties

        /// <value>
        /// The number of seconds until the LLDP needs
        /// to be refreshed
        ///
        /// A value of 0 means that the LLDP source is
        /// closed and should no longer be refreshed
        /// </value>
        public UInt16 Seconds
        {
            get => EndianBitConverter.Big.ToUInt16(this.tlvData.Bytes, this.tlvData.Offset + TLVTypeLength.TypeLengthLength);
            set => EndianBitConverter.Big.CopyBytes(value, this.tlvData.Bytes, this.tlvData.Offset + TLVTypeLength.TypeLengthLength);
        }

        /// <summary>
        /// Convert this TTL TLV to a string.
        /// </summary>
        /// <returns>
        /// A human readable string
        /// </returns>
        public override String ToString ()
        {
            return String.Format("[TimeToLive: Seconds={0}]", this.Seconds);
        }

        #endregion
    }
}