using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class NetworkHeader {
    public static int BYTE_SIZE = 8;

    public int typeId;
    public int packetSize;

    public NetworkHeader(int typeId, int packetSize) {
        this.typeId = typeId;
        this.packetSize = packetSize;
    }

    public static NetworkHeader FromBuffer(byte[] buffer) {
        return new NetworkHeader(BytesToInt(buffer, 0), BytesToInt(buffer, 4));
    }

    public void ToBuffer(byte[] buffer) {
        IntToBytes(typeId, buffer, 0);
        IntToBytes(packetSize, buffer, 4);
    }

    public override string ToString() {
        return $"[NetworkHeader typeId={typeId} packetSize={packetSize}]";
    }

    private static int BytesToInt(byte[] bytes, int index) {
        return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, index));
    }

    private static void IntToBytes(int number, byte[] bytes, int index) {
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(number)).CopyTo(bytes, index);
    }
}
