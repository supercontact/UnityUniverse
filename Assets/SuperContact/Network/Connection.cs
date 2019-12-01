using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using ProtoBuf;
using Wintellect.PowerCollections;

public class Connection {
    private static int READ_BUFFER_SIZE = 1024 * 1024;
    private static int WRITE_BUFFER_SIZE = 1024 * 1024;
    private static int UNHANLDED_PACKET_QUEUE_LENGTH = 64;

    public int networkId { get; private set; }

    private TcpClient client;
    private NetworkStream stream;

    private byte[] readBuffer;
    private byte[] writeBuffer;
    private MultiDictionary<Type, Action<object>> readListeners = new MultiDictionary<Type, Action<object>>(false);
    private Dictionary<object, Action<object>> actionMap = new Dictionary<object, Action<object>>();
    private ConcurrentDictionary<Type, ConcurrentQueue<object>> unhandledPackets = new ConcurrentDictionary<Type, ConcurrentQueue<object>>();

    public Connection(TcpClient client, int networkId) {
        if (!client.Connected) {
            throw new InvalidOperationException("Client is not connected!");
        }

        this.client = client;
        this.networkId = networkId;
        readBuffer = new byte[READ_BUFFER_SIZE];
        writeBuffer = new byte[WRITE_BUFFER_SIZE];
        stream = client.GetStream();
        Read();
    }

    public void Close() {
        client.Close();
    }

    public void Send<T>(T obj) {
        int typeId = NetworkRegistry.packetTypeById.Reverse[typeof(T)];
        Send(typeId, obj);
    }

    public void Send(int typeId, object obj) {
        using (MemoryStream memoryStream = new MemoryStream(writeBuffer, NetworkHeader.BYTE_SIZE, WRITE_BUFFER_SIZE - NetworkHeader.BYTE_SIZE)) {
            Serializer.NonGeneric.Serialize(memoryStream, obj);
            int packetSize = (int)memoryStream.Position;
            NetworkHeader header = new NetworkHeader(typeId, packetSize);
            header.ToBuffer(writeBuffer);
            Logging.Log($"Writting a [{typeId}]{NetworkRegistry.packetTypeById[typeId]} of size {packetSize}...");
            stream.BeginWrite(writeBuffer, 0, NetworkHeader.BYTE_SIZE + packetSize, OnWrite, null);
        }
    }

    private void OnWrite(IAsyncResult asyncResult) {
        stream.EndWrite(asyncResult);
        Logging.Log("Network write done!");
    }

    public void Listen<T>(Action<T> listener) {
        if (actionMap.ContainsKey(listener)) return;

        Action<object> objectListener = new Action<object>(obj => listener((T)obj));
        actionMap.Add(listener, objectListener);
        Listen(typeof(T), objectListener);
    }
    public void Listen<T>(Action<T, Connection> listener) {
        if (actionMap.ContainsKey(listener)) return;

        Action<object> objectListener = new Action<object>(obj => listener((T)obj, this));
        actionMap.Add(listener, objectListener);
        Listen(typeof(T), objectListener);
    }

    private void Listen(Type type, Action<object> listener) {
        readListeners.Add(type, listener);
        if (unhandledPackets.ContainsKey(type)) {
            ConcurrentQueue<object> queue = unhandledPackets[type];
            while (queue.TryDequeue(out object packet)) {
                NetworkManager.GetInstance().QueueAction(listener, packet);
            }
        }
    }

    public void Unlisten<T>(Action<T> listener) {
        if (!actionMap.ContainsKey(listener)) return;

        readListeners.Remove(typeof(T), actionMap[listener]);
        actionMap.Remove(listener);
    }
    public void Unlisten<T>(Action<T, Connection> listener) {
        if (!actionMap.ContainsKey(listener)) return;

        readListeners.Remove(typeof(T), actionMap[listener]);
        actionMap.Remove(listener);
    }

    public void Read() {
        stream.BeginRead(readBuffer, 0, NetworkHeader.BYTE_SIZE, OnReadHeader, null);
    }

    private void OnReadHeader(IAsyncResult asyncResult) {
        int numberOfBytesRead = stream.EndRead(asyncResult);
        if (numberOfBytesRead < NetworkHeader.BYTE_SIZE) {
            throw new Exception("Error when reading packet header!");
        }
        NetworkHeader header = NetworkHeader.FromBuffer(readBuffer);
        if (header.packetSize > READ_BUFFER_SIZE - NetworkHeader.BYTE_SIZE) {
            throw new Exception("Packet is too big!");
        }
        Logging.Log($"Header read, packet size is {header.packetSize} bytes.");
        if (header.packetSize == 0) {
            ExtractProtoFromReadBuffer(header);
            Read();
        } else {
            stream.BeginRead(readBuffer, 0, header.packetSize, OnReadPacket, header);
        }
    }

    private void OnReadPacket(IAsyncResult asyncResult) {
        int numberOfBytesRead = stream.EndRead(asyncResult);
        NetworkHeader header = (NetworkHeader)asyncResult.AsyncState;
        Logging.Log($"Packet bytes read with size = {numberOfBytesRead}. Now deserializing as [{header.typeId}]{NetworkRegistry.packetTypeById[header.typeId]}.");
        ExtractProtoFromReadBuffer(header);
        Read();
    }

    private void ExtractProtoFromReadBuffer(NetworkHeader header) {
        Type type = NetworkRegistry.packetTypeById[header.typeId];
        using (MemoryStream memoryStream = new MemoryStream(readBuffer, 0, header.packetSize)) {
            var obj = Serializer.Deserialize(type, memoryStream);
            Logging.Log("Packet deserialized: " + obj.ToString());

            if (readListeners.ContainsKey(type)) {
                foreach (Action<object> listener in readListeners[type]) {
                    NetworkManager.GetInstance().QueueAction(listener, obj);
                }
            } else {
                Logging.Log($"No listener registered for packet type {type}! Adding to unhandled queue.");
                unhandledPackets.AddOrUpdate(
                    type,
                    (Type key) => {
                        var newQueue = new ConcurrentQueue<object>();
                        newQueue.Enqueue(obj);
                        return newQueue;
                    },
                    (Type key, ConcurrentQueue<object> existingQueue) => {
                        while (existingQueue.Count >= UNHANLDED_PACKET_QUEUE_LENGTH) {
                            Logging.Log("Unhandled queue full! Dropping packet.");
                            existingQueue.TryDequeue(out object discardedPacket);
                        }
                        existingQueue.Enqueue(obj);
                        return existingQueue;
                    });
            }
        }
    }
}