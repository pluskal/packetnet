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
 */
using System;
using MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Tcp
{
    /// <summary>
    /// SACK (Selective Ack) Option
    ///  Provides a means for a receiver to notify the sender about
    ///  all the segments that have arrived successfully.
    ///  Used to cut down on the number of unnecessary re-transmissions.
    /// </summary>
    /// <remarks>
    /// References:
    ///  http://datatracker.ietf.org/doc/rfc2018/
    ///  http://datatracker.ietf.org/doc/rfc2883/
    /// </remarks>
    public class SACK : Option
    {
        #region Constructors

        /// <summary>
        /// Creates a SACK (Selective Ack) Option
        /// </summary>
        /// <param name="bytes">
        /// A <see cref="T:System.Byte[]"/>
        /// </param>
        /// <param name="offset">
        /// A <see cref="System.Int32"/>
        /// </param>
        /// <param name="length">
        /// A <see cref="System.Int32"/>
        /// </param>
        public SACK(Byte[] bytes, Int32 offset, Int32 length) :
            base(bytes, offset, length)
        { }

        #endregion

        #region Properties

        /// <summary>
        /// Contains an array of SACK (Selective Ack) Blocks
        /// </summary>
        public UInt16[] SACKBlocks
        {
            get
            {
                Int32 numOfBlocks = (this.Length - SACKBlocksFieldOffset) / BlockLength;
                UInt16[] blocks = new UInt16[numOfBlocks];
                Int32 offset = 0;
                for(Int32 i = 0; i < numOfBlocks; i++)
                {
                    offset = SACKBlocksFieldOffset + (i * BlockLength);
                    blocks[i] = EndianBitConverter.Big.ToUInt16(this.Bytes, offset);
                }
                return blocks;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the Option info as a string
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/>
        /// </returns>
        public override String ToString()
        {
            String output = "[" + this.Kind.ToString() + ": ";

            for(Int32 i = 0; i < this.SACKBlocks.Length; i++)
            {
                output += "Block" + i + "=" + this.SACKBlocks[i].ToString() + " ";
            }

            output.TrimEnd();
            output += "]";

            return output;
        }

        #endregion
        
        #region Members

        // the length (in bytes) of a SACK block
        const Int32 BlockLength = 2;

        // the offset (in bytes) of the ScaleFactor Field
        const Int32 SACKBlocksFieldOffset = 2;

       #endregion
    }
}