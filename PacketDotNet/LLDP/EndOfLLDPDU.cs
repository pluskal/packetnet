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
namespace PacketDotNet.LLDP
{
    /// <summary>
    /// An End Of LLDPDU TLV
    /// </summary>
    [Serializable]
    public class EndOfLLDPDU : TLV
    {
        #region Constructors

        /// <summary>
        /// Parses bytes into an End Of LLDPDU TLV
        /// </summary>
        /// <param name="bytes">
        /// TLV bytes
        /// </param>
        /// <param name="offset">
        /// The End Of LLDPDU TLV's offset from the
        /// origin of the LLDP
        /// </param>
        public EndOfLLDPDU(Byte[] bytes, Int32 offset) :
            base(bytes, offset)
        {
            this.Type = 0;
            this.Length = 0;
        }

        /// <summary>
        /// Creates an End Of LLDPDU TLV
        /// </summary>
        public EndOfLLDPDU()
        {
            var bytes = new Byte[TLVTypeLength.TypeLengthLength];
            var offset = 0;
            var length = bytes.Length;
            this.tlvData = new Utils.ByteArraySegment(bytes, offset, length);

            this.Type = 0;
            this.Length = 0;
        }

        /// <summary>
        /// Convert this TTL TLV to a string.
        /// </summary>
        /// <returns>
        /// A human readable string
        /// </returns>
        public override String ToString ()
        {
            return String.Format("[EndOfLLDPDU]");
        }

        #endregion
    }
}