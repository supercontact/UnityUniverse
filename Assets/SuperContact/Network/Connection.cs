using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ProtoBuf;
using Wintellect.PowerCollections;

public class Connection {
    private static int READ_BUFFER_SIZE = 1024 * 1024;
    private static int WRITE_BUFFER_SIZE = 1024 * 1024;
    private static int UNHANLDED_PACKET_QUEUE_LENGTH = 64;

    public int networkId { get; private set; }

    private TcpClient client;
    private NetworkStream stream;

    private byte[] writeBuffer;
    private byte[] readBuffer;
    private bool isWriting;
    private ConcurrentQueue<KeyValuePair<int, object>> writeObjectQueue = new ConcurrentQueue<KeyValuePair<int, object>>();
    private int readStartsAt = 0;
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
        writeObjectQueue.Enqueue(new KeyValuePair<int, object>(typeId, obj));

        if (!isWriting) {
            WriteQueuedPackets();
        }
    }

    private void WriteQueuedPackets() {
        isWriting = true;
        if (!writeObjectQueue.TryDequeue(out KeyValuePair<int, object> packet)) return;

        using (MemoryStream memoryStream = new MemoryStream(writeBuffer, NetworkHeader.BYTE_SIZE, WRITE_BUFFER_SIZE - NetworkHeader.BYTE_SIZE)) {
            Serializer.NonGeneric.Serialize(memoryStream, packet.Value);
            int packetSize = (int)memoryStream.Position;
            NetworkHeader header = new NetworkHeader(packet.Key, packetSize);
            header.ToBuffer(writeBuffer);
            //Logging.Log($"Writting a [{typeId}]{NetworkRegistry.packetTypeById[typeId]} of size {packetSize}...");
            try {
                stream.BeginWrite(writeBuffer, 0, NetworkHeader.BYTE_SIZE + packetSize, OnWrite, null);
            } catch (Exception e) {
                Logging.LogError(e);
                throw e;
            }
        }
    }

    private void OnWrite(IAsyncResult asyncResult) {
        stream.EndWrite(asyncResult);
        if (writeObjectQueue.Count > 0) {
            WriteQueuedPackets();
        } else {
            isWriting = false;
        }
        //Logging.Log("Network write done!");
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
        readStartsAt = 0;
        try {
            stream.BeginRead(readBuffer, 0, NetworkHeader.BYTE_SIZE, OnReadHeader, null);
        } catch (Exception e) {
            Logging.LogError(e);
            throw e;
        }
    }

    private void OnReadHeader(IAsyncResult asyncResult) {
        int numberOfBytesRead = stream.EndRead(asyncResult);
        if (numberOfBytesRead == 0) {
            ConnectionEnded();
            return;
        }
        if (readStartsAt + numberOfBytesRead < NetworkHeader.BYTE_SIZE) {
            readStartsAt += numberOfBytesRead;
            try {
                stream.BeginRead(readBuffer, readStartsAt, NetworkHeader.BYTE_SIZE - readStartsAt, OnReadHeader, null);
            } catch (Exception e) {
                Logging.LogError(e);
                throw e;
            }
            return;
        }
        NetworkHeader header = NetworkHeader.FromBuffer(readBuffer);
        if (header.packetSize > READ_BUFFER_SIZE) {
            Logging.LogError($"Packet is too big! Header: {header}");
            throw new Exception($"Packet is too big! Header: {header}");
        }
        //Logging.Log($"Header read, packet size is {header.packetSize} bytes.");
        if (header.packetSize == 0) {
            try {
                ExtractProtoFromReadBuffer(header);
            } catch (Exception e) {
                Logging.LogError(e);
            }
            Read();
        } else {
            readStartsAt = 0;
            try {
                stream.BeginRead(readBuffer, 0, header.packetSize, OnReadPacket, header);
            } catch (Exception e) {
                Logging.LogError(e);
                throw e;
            }
        }
    }

    private void OnReadPacket(IAsyncResult asyncResult) {
        int numberOfBytesRead = stream.EndRead(asyncResult);
        if (numberOfBytesRead == 0) {
            ConnectionEnded();
            return;
        }
        NetworkHeader header = (NetworkHeader)asyncResult.AsyncState;
        if (readStartsAt + numberOfBytesRead < header.packetSize) {
            readStartsAt += numberOfBytesRead;
            try {
                stream.BeginRead(readBuffer, readStartsAt, header.packetSize - readStartsAt, OnReadHeader, header);
            } catch (Exception e) {
                Logging.LogError(e);
                throw e;
            }
            return;
        }
        //Logging.Log($"Packet bytes read with size = {numberOfBytesRead}. Now deserializing as [{header.typeId}]{NetworkRegistry.packetTypeById[header.typeId]}.");
        try {
            ExtractProtoFromReadBuffer(header);
        } catch (Exception e) {
            Logging.LogError(e);
        }
        Read();
    }

    private void ExtractProtoFromReadBuffer(NetworkHeader header) {
        Type type = NetworkRegistry.packetTypeById[header.typeId];
        using (MemoryStream memoryStream = new MemoryStream(readBuffer, 0, header.packetSize)) {
            var obj = Serializer.Deserialize(type, memoryStream);
            //Logging.Log("Packet deserialized: " + obj.ToString());

            if (readListeners.ContainsKey(type)) {
                foreach (Action<object> listener in readListeners[type]) {
                    NetworkManager.GetInstance().QueueAction(listener, obj);
                }
            } else {
                Logging.LogWarning($"No listener registered for packet type {type}! Adding to unhandled queue.");
                unhandledPackets.AddOrUpdate(
                    type,
                    (Type key) => {
                        var newQueue = new ConcurrentQueue<object>();
                        newQueue.Enqueue(obj);
                        return newQueue;
                    },
                    (Type key, ConcurrentQueue<object> existingQueue) => {
                        while (existingQueue.Count >= UNHANLDED_PACKET_QUEUE_LENGTH) {
                            Logging.LogWarning("Unhandled queue full! Dropping packet.");
                            existingQueue.TryDequeue(out object discardedPacket);
                        }
                        existingQueue.Enqueue(obj);
                        return existingQueue;
                    });
            }
        }
    }

    private void ConnectionEnded() {
        // Event
        Logging.LogWarning("Connection ended!");
    }
}