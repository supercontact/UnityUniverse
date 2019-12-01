using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MineSweeper {
    public class FlagControl : MonoBehaviour {

        private static readonly float FLAG_SIZE_SCALE = 1.2f;
        private static readonly float FLASH_PERIOD_MIN = 0.9f;
        private static readonly float FLASH_PERIOD_MAX = 1.1f;
        private static readonly float FLASH_DURATION = 1f;

        public enum FlagType {
            Normal,
            Hightlighted,
            Wrong,
        }

        public TileControl parent;
        public GameObject normalFlag;
        public GameObject highlightedFlag;
        public GameObject wrongFlag;

        public bool isCrazy = false;
        public bool isFreezed = false;

        private FlagType currentFlagType = FlagType.Normal;
        private float crazyTimer = 0f;

        public void Init() {
            transform.localPosition = parent.tileHeight * parent.faceNormal;
            transform.localScale = FLAG_SIZE_SCALE * (parent.GetAverageRadius() - parent.tileTopToEdgeDistange) * Vector3.one;
        }

        public void SetFlagType(FlagType flagType) {
            if (flagType == currentFlagType) return;
            transform.localPosition = (flagType == FlagType.Wrong ? 0 : parent.tileHeight) * parent.faceNormal;
            normalFlag.SetActive(flagType == FlagType.Normal);
            highlightedFlag.SetActive(flagType == FlagType.Hightlighted);
            wrongFlag.SetActive(flagType == FlagType.Wrong);
            currentFlagType = flagType;
        }

        private void Update() {
            if (isFreezed) return;

            ObserveCamera camera = Globals.instance.observeCamera;
            transform.rotation = Quaternion.LookRotation(parent.faceNormal, camera.transform.position - parent.faceCenter);

            if (isCrazy) {
                crazyTimer -= Time.deltaTime;
                if (crazyTimer < -FLASH_DURATION) {
                    crazyTimer = Random.Range(FLASH_PERIOD_MIN, FLASH_PERIOD_MAX);
                    SetFlagType(FlagType.Normal);
                } else if (crazyTimer < 0) {
                    SetFlagType(FlagType.Hightlighted);
                }
            }
        }

        private void OnEnable() {
            Update();
        }
    }
}