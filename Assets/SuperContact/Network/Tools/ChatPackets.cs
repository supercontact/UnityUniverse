using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtoContract]
public class ChatRequest {

    [ProtoMember(1)]
    public string playerName;
    [ProtoMember(2)]
    public string message;

    public ChatRequest() { }
    public ChatRequest(string playerName, string message) {
        this.playerName = playerName;
        this.message = message;
    }
}