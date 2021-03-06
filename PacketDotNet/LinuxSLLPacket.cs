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
 *  Copyright 2009 Chris Morgan <chmorgan@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Text;
using MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    /// Represents a Linux cooked capture packet, the kinds of packets
    /// received when capturing on an 'any' device
    ///
    /// See http://github.com/mcr/libpcap/blob/master/pcap/sll.h
    /// </summary>
    public class LinuxSLLPacket : InternetLinkLayerPacket
    {
        /// <value>
        /// Information about the packet direction
        /// </value>
        public LinuxSLLType Type
        {
            get => (LinuxSLLType)EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + LinuxSLLFields.PacketTypePosition);

            set
            {
                var theValue = (Int16)value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + LinuxSLLFields.PacketTypePosition);
            }
        }

        /// <value>
        /// The
        /// </value>
        public Int32 LinkLayerAddressType
        {
            get => EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressTypePosition);

            set
            {
                var theValue = (Int16)value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressTypePosition);
            }
        }

        /// <value>
        /// Number of bytes in the link layer address of the sender of the packet
        /// </value>
        public Int32 LinkLayerAddressLength
        {
            get => EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressLengthPosition);

            set
            {
                // range check
                if((value < 0) || (value > 8))
                {
                    throw new InvalidOperationException("value of " + value + " out of range of 0 to 8");
                }

                var theValue = (Int16)value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressLengthPosition);
            }
        }

        /// <value>
        /// Link layer header bytes, maximum of 8 bytes
        /// </value>
        public Byte[] LinkLayerAddress
        {
            get
            {
                var headerLength = this.LinkLayerAddressLength;
                var theHeader = new Byte[headerLength];
                Array.Copy(this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressPosition,
                           theHeader, 0,
                           headerLength);
                return theHeader;
            }

            set
            {
                // update the link layer length
                this.LinkLayerAddressLength = value.Length;

                // copy in the new link layer header bytes
                Array.Copy(value, 0, this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressPosition,
                           value.Length);
            }
        }

        /// <value>
        /// The encapsulated protocol type
        /// </value>
        public EthernetPacketType EthernetProtocolType
        {
            get => (EthernetPacketType)EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + LinuxSLLFields.EthernetProtocolTypePosition);

            set
            {
                var theValue = (Int16)value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + LinuxSLLFields.EthernetProtocolTypePosition);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bas">
        /// A <see cref="ByteArraySegment"/>
        /// </param>
        public LinuxSLLPacket(ByteArraySegment bas)
        {
            this.header = new ByteArraySegment(bas);
            this.header.Length = LinuxSLLFields.SLLHeaderLength;

            // parse the payload via an EthernetPacket method
            this.payloadPacketOrData = EthernetPacket.ParseEncapsulatedBytes(this.header, this.EthernetProtocolType);
        }

        /// <summary cref="Packet.ToString(StringOutputType)" />
        public override String ToString (StringOutputType outputFormat)
        {
            var buffer = new StringBuilder();
            String color = "";
            String colorEscape = "";

            if(outputFormat == StringOutputType.Colored || outputFormat == StringOutputType.VerboseColored)
            {
                color = this.Color;
                colorEscape = AnsiEscapeSequences.Reset;
            }

            if(outputFormat == StringOutputType.Normal || outputFormat == StringOutputType.Colored)
            {
                // build the output string
                buffer.AppendFormat("[{0}LinuxSLLPacket{1}: Type={2}, LinkLayerAddressType={3}, LinkLayerAddressLength={4}, Source={5}, ProtocolType={6}]",
                    color,
                    colorEscape, this.Type, this.LinkLayerAddressType, this.LinkLayerAddressLength,
                    BitConverter.ToString(this.LinkLayerAddress, 0), this.EthernetProtocolType);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // collect the properties and their value
                Dictionary<String,String> properties = new Dictionary<String,String>();
                properties.Add("type", this.Type.ToString() + " (" + ((Int32) this.Type).ToString() + ")");
                properties.Add("link layer address type", this.LinkLayerAddressType.ToString());
                properties.Add("link layer address length", this.LinkLayerAddressLength.ToString());
                properties.Add("source", BitConverter.ToString(this.LinkLayerAddress));
                properties.Add("protocol", this.EthernetProtocolType.ToString() + " (0x" + this.EthernetProtocolType.ToString("x") + ")");


                // calculate the padding needed to right-justify the property names
                Int32 padLength = RandomUtils.LongestStringLength(new List<String>(properties.Keys));

                // build the output string
                buffer.AppendLine("LCC:  ******* LinuxSLL - \"Linux Cooked Capture\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("LCC:");
                foreach(var property in properties)
                {
                    buffer.AppendLine("LCC: " + property.Key.PadLeft(padLength) + " = " + property.Value);
                }
                buffer.AppendLine("LCC:");
            }

            // append the base output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }
    }
}
