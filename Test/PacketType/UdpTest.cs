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
 *  Copyright 2010 Chris Morgan <chmorgan@gmail.com>
 */

using System;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using NUnit.Framework;
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using PacketDotNet.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Test.PacketType
{
    [TestFixture]
    public class UdpTest
    {
        /// <summary>
        /// Test that we can load a udp packet and that the udp properties are
        /// as we expect them
        /// </summary>
        [Test]
        public void UDPData()
        {
            RawCapture rawCapture;
            UdpPacket u;
            Packet p;

            var dev = new CaptureFileReaderDevice("../../CaptureFiles/udp_dns_request_response.pcap");
            dev.Open();

            // check the first packet
            rawCapture = dev.GetNextPacket();

            p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
            Assert.IsNotNull(p);

            u = (UdpPacket)p.Extract(typeof(UdpPacket));
            Assert.IsNotNull(u, "Expected a non-null UdpPacket");
            Assert.AreEqual(41 - u.Header.Length,
                            u.PayloadData.Length, "UDPData.Length mismatch");

            // check the second packet
            rawCapture = dev.GetNextPacket();
            p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

            Assert.IsNotNull(p);

            u = (UdpPacket)p.Extract(typeof(UdpPacket));
            Assert.IsNotNull(u, "Expected u to be a UdpPacket");
            Assert.AreEqual(356 - u.Header.Length,
                            u.PayloadData.Length, "UDPData.Length mismatch");

            Console.WriteLine("u is {0}", u.ToString());

            dev.Close();
        }

        /// <summary>
        /// Test that we can build a udp packet from values, convert it into a byte[]
        /// and then re-parse it back into a UdpPacket.
        ///
        /// Also test that the UdpPacket.Length field is updated properly in the
        /// conversion to a byte[]
        /// </summary>
        [Test]
        public void ConstructUdpPacketFromValuesAndCheckThatLengthIsUpdated()
        {
            // build a udp packet
            UInt16 sourcePort = 200;
            UInt16 destinationPort = 300;
            Byte[] dataBytes = new Byte[32];
            for(Int32 i = 0; i < dataBytes.Length; i++)
            {
                dataBytes[i] = (Byte)i;
            }

            var udpPacket = new UdpPacket(sourcePort, destinationPort);
            udpPacket.PayloadData = dataBytes;

            // retrieve the bytes, this should cause UdpPacket.Length to be updated
            var packetBytes = udpPacket.Bytes;

            // now reparse the packet again
            var udpPacket2 = new UdpPacket(new ByteArraySegment(packetBytes));

            Assert.AreEqual(sourcePort, udpPacket.SourcePort);
            Assert.AreEqual(destinationPort, udpPacket.DestinationPort);

            Console.WriteLine("udpPacket.Length {0}", udpPacket.Length);
            udpPacket.PayloadData = dataBytes;

            Assert.AreEqual(sourcePort, udpPacket.SourcePort);
            Assert.AreEqual(destinationPort, udpPacket.DestinationPort);

            // make sure the data matches up
            Assert.AreEqual(dataBytes, udpPacket2.PayloadData, "PayloadData mismatch");

            // and make sure the length is what we expect
            Assert.AreEqual(dataBytes.Length + UdpFields.HeaderLength, udpPacket2.Length);
        }

        [Test]
        public void RandomPacket()
        {
            UdpPacket.RandomPacket();
        }

        /// <summary>
        /// Test that we can load and parse a UDP packet and that
        /// the computed checksum matches the expected checksum
        /// </summary>
        [Test]
        public void UDPChecksum()
        {
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/udp.pcap");
            dev.Open();

            // checksums from wireshark of the capture file
            Int32[] expectedChecksum = {0x2be9,
                                      0x9e06,
                                      0xd279,
                                      0x4709,
                                      0x61cd,
                                      0x9939,
                                      0x4937,
                                      0x4dfc,
                                      0xb8e6,
                                      0x932c};

            Int32 packetIndex = 0;
            RawCapture rawCapture;
            while ((rawCapture = dev.GetNextPacket()) != null)
            {
                var p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                var t = (UdpPacket)p.Extract (typeof(UdpPacket));
                Assert.IsNotNull(t, "Expected t to not be null");
                Assert.IsTrue(t.ValidChecksum, "t.ValidChecksum isn't true");

                // compare the computed checksum to the expected one
                Assert.AreEqual(expectedChecksum[packetIndex],
                                t.CalculateUDPChecksum(),
                                "Checksum mismatch");

                packetIndex++;
            }

            dev.Close();
        }

        [Test]
        public void UdpPacketInsideOfEthernetPacketWithTrailer()
        {
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/udpPacketWithEthernetTrailers.pcap");
            dev.Open();

            // checksums from wireshark of the capture file
            Int32[] expectedChecksum = {0x61fb};

            Int32 packetIndex = 0;
            RawCapture rawCapture;
            while ((rawCapture = dev.GetNextPacket()) != null)
            {
                var p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
                Console.WriteLine("Converted a raw packet to a Packet");
                Console.WriteLine(p.ToString());
                var u = (UdpPacket)p.Extract (typeof(UdpPacket));
                Assert.IsNotNull(u, "Expected u to not be null");
                Assert.IsTrue(u.ValidChecksum, "u.ValidChecksum isn't true");

                // compare the computed checksum to the expected one
                Assert.AreEqual(expectedChecksum[packetIndex],
                                u.CalculateUDPChecksum(),
                                "Checksum mismatch");

                packetIndex++;
            }

            dev.Close();
        }

        [Test]
        public void PrintString()
        {
            Console.WriteLine("Loading the sample capture file");
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/udp.pcap");
            dev.Open();
            Console.WriteLine("Reading packet data");
            var rawCapture = dev.GetNextPacket();
            var p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

            Console.WriteLine("Parsing");
            var udp = (UdpPacket)p.Extract (typeof(UdpPacket));

            Console.WriteLine("Printing human readable string");
            Console.WriteLine(udp.ToString());
        }

        [Test]
        public void PrintVerboseString()
        {
            Console.WriteLine("Loading the sample capture file");
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/udp.pcap");
            dev.Open();
            Console.WriteLine("Reading packet data");
            var rawCapture = dev.GetNextPacket();
            var p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

            Console.WriteLine("Parsing");
            var udp = (UdpPacket)p.Extract (typeof(UdpPacket));

            Console.WriteLine("Printing human readable string");
            Console.WriteLine(udp.ToString(StringOutputType.Verbose));
        }

        [Test]
        public void BinarySerialization()
        {
            var dev = new CaptureFileReaderDevice("../../CaptureFiles/udp_dns_request_response.pcap");
            dev.Open();

            RawCapture rawCapture;
            Boolean foundudpPacket = false;
            while ((rawCapture = dev.GetNextPacket()) != null)
            {
                var p = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

                var udpPacket = (UdpPacket)p.Extract(typeof(UdpPacket));
                if (udpPacket == null)
                {
                    continue;
                }
                foundudpPacket = true;

                var memoryStream = new MemoryStream();
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(memoryStream, udpPacket);

                memoryStream.Seek (0, SeekOrigin.Begin);
                BinaryFormatter deserializer = new BinaryFormatter();
                UdpPacket fromFile = (UdpPacket)deserializer.Deserialize(memoryStream);

                Assert.AreEqual(udpPacket.Bytes, fromFile.Bytes);
                Assert.AreEqual(udpPacket.BytesHighPerformance.Bytes, fromFile.BytesHighPerformance.Bytes);
                Assert.AreEqual(udpPacket.BytesHighPerformance.BytesLength, fromFile.BytesHighPerformance.BytesLength);
                Assert.AreEqual(udpPacket.BytesHighPerformance.Length, fromFile.BytesHighPerformance.Length);
                Assert.AreEqual(udpPacket.BytesHighPerformance.NeedsCopyForActualBytes, fromFile.BytesHighPerformance.NeedsCopyForActualBytes);
                Assert.AreEqual(udpPacket.BytesHighPerformance.Offset, fromFile.BytesHighPerformance.Offset);
                Assert.AreEqual(udpPacket.Color, fromFile.Color);
                Assert.AreEqual(udpPacket.Header, fromFile.Header);
                Assert.AreEqual(udpPacket.PayloadData, fromFile.PayloadData);
                Assert.AreEqual(udpPacket.DestinationPort, fromFile.DestinationPort);
                Assert.AreEqual(udpPacket.Length, fromFile.Length);
                Assert.AreEqual(udpPacket.SourcePort, fromFile.SourcePort);
                Assert.AreEqual(udpPacket.ValidChecksum, fromFile.ValidChecksum);
                Assert.AreEqual(udpPacket.ValidUDPChecksum, fromFile.ValidUDPChecksum);

                //Method Invocations to make sure that a deserialized packet does not cause 
                //additional errors.

                udpPacket.CalculateUDPChecksum();
                udpPacket.IsValidChecksum(TransportPacket.TransportChecksumOption.None);
                udpPacket.PrintHex();
                udpPacket.UpdateCalculatedValues();
                udpPacket.UpdateUDPChecksum();
            }

            dev.Close();
            Assert.IsTrue(foundudpPacket, "Capture file contained no udpPacket packets");
        }
    }
}
