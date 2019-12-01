using ProtoBuf;

[ProtoContract]
public class StringPacket {
    [ProtoMember(1)]
    public string value;

    public static explicit operator StringPacket(string value) => new StringPacket() { value = value };

    public override string ToString() => value;
}
