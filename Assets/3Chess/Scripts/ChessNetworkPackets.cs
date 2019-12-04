using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;

[ProtoContract]
public class PlayerInfoRequest {

    [ProtoMember(1)]
    public string playerName;

    public PlayerInfoRequest() { }
    public PlayerInfoRequest(string playerName) {
        this.playerName = playerName;
    }
}

[ProtoContract]
public class InitChessRequest {

    [ProtoMember(1)]
    public IntVector3 size;
    [ProtoMember(2)]
    public int comboLength;
    [ProtoMember(3)]
    public int scoreToWin;
    [ProtoMember(4)]
    public int controlledPlayer;
    [ProtoMember(5)]
    public int firstPlayer;

    public InitChessRequest() { }
    public InitChessRequest(IntVector3 size, int comboLength, int scoreToWin, int controlledPlayer, int firstPlayer) {
        this.size = size;
        this.comboLength = comboLength;
        this.scoreToWin = scoreToWin;
        this.controlledPlayer = controlledPlayer;
        this.firstPlayer = firstPlayer;
    }
}

[ProtoContract]
public class RestartChessRequest {

    [ProtoMember(1)]
    public int firstPlayer;

    public RestartChessRequest() { }
    public RestartChessRequest(int firstPlayer) {
        this.firstPlayer = firstPlayer;
    }
}

[ProtoContract]
public class PlaceChessRequest {

    [ProtoMember(1)]
    public IntVector3 location;

    public PlaceChessRequest() { }
    public PlaceChessRequest(IntVector3 location) {
        this.location = location;
    }
}

[ProtoContract]
public class ObserveCameraControlRequest {

    [ProtoMember(1)]
    public float targetDistance;
    [ProtoMember(2)]
    public float targetRotationEularX;
    [ProtoMember(3)]
    public float targetRotationEularY;
    [ProtoMember(4)]
    public float targetRotationEularZ;

    public Quaternion targetRotation => Quaternion.Euler(targetRotationEularX, targetRotationEularY, targetRotationEularZ);

    public ObserveCameraControlRequest() { }
    public ObserveCameraControlRequest(float targetDistance, Quaternion targetRotation) {
        this.targetDistance = targetDistance;
        Vector3 eular = targetRotation.eulerAngles;
        targetRotationEularX = eular.x;
        targetRotationEularY = eular.y;
        targetRotationEularZ = eular.z;
    }
}

[ProtoContract]
public class GuideLineDisplayRequest {

    [ProtoMember(1)]
    public bool isOn;

    public GuideLineDisplayRequest() { }
    public GuideLineDisplayRequest(bool isOn) {
        this.isOn = isOn;
    }
}

[ProtoContract]
public class ObserveModeRequest {

    [ProtoMember(1)]
    public bool isOn;

    public ObserveModeRequest() { }
    public ObserveModeRequest(bool isOn) {
        this.isOn = isOn;
    }
}