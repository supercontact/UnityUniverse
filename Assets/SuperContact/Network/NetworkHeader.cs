using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
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

    public static async Task<NetworkHeader> FromStream(Stream stream) {
        byte[] buffer = new byte[BYTE_SIZE];
        await stream.ReadAsync(buffer, 0, BYTE_SIZE);
        return FromBuffer(buffer);
    }

    public void ToBuffer(byte[] buffer) {
        IntToBytes(typeId, buffer, 0);
        IntToBytes(packetSize, buffer, 4);
    }

    public async Task ToStream(Stream stream) {
        byte[] buffer = new byte[BYTE_SIZE];
        ToBuffer(buffer);
        await stream.WriteAsync(buffer, 0, BYTE_SIZE);
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
