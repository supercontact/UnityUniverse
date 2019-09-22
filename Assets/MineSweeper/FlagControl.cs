using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlagControl : MonoBehaviour {

    private static readonly float FLAG_SIZE_SCALE = 1.2f;

    public TileBlock parent;
    public GameObject goodFlag;
    public GameObject wrongFlag;

    public void Init() {
        transform.localPosition = parent.tileHeight * parent.faceNormal;
        transform.localScale = FLAG_SIZE_SCALE * (parent.GetAverageRadius() - parent.tileTopToEdgeDistange) * Vector3.one;
    }

    public void SetWrongFlag(bool isWrongFlag) {
        transform.localPosition = (isWrongFlag ? 0 : parent.tileHeight) * parent.faceNormal;
        goodFlag.SetActive(!isWrongFlag);
        wrongFlag.SetActive(isWrongFlag);
    }

    private void Update() {
        ObserveCamera camera = Globals.instance.observeCamera;
        transform.rotation = Quaternion.LookRotation(parent.faceNormal, camera.transform.position - parent.faceCenter);
    }

    private void OnEnable() {
        Update();
    }
}
