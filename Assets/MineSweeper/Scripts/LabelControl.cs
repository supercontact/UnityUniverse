using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MineSweeper {
    public class LabelControl : MonoBehaviour {

        private static readonly float LABEL_SIZE_SCALE = 2f;
        private static readonly float FLOATING_DISTANCE = 0.01f;
        private static readonly float MAX_SPINNING_SPEED = 720f;
        private static readonly float MAX_SPINNING_ACCELERATION = 720f;
        private static readonly float WAVE_PERIOD = 500f;
        private static readonly float WAVE_HEIGHT = 0.1f;

        public TileControl parent;
        public bool isCrazy = false;
        public bool isFreezed = false;

        private int currentNumber;
        private Vector3 originalPosition;
        private float spinningSpeed = 0f;
        private float waveFactor = 0f;

        public void InitWithMark() {
            originalPosition = (parent.tileHeight + FLOATING_DISTANCE) * parent.faceNormal;
            transform.localPosition = originalPosition;
            transform.localScale = LABEL_SIZE_SCALE * (parent.GetIncircleRadius() - parent.tileTopToEdgeDistange) * Vector3.one;
            Update();
        }

        public void InitWithNumber(int number) {
            currentNumber = number;
            GetComponent<MeshRenderer>().sharedMaterial = NumberMaterials.GetNumberMaterial(number);
            originalPosition = FLOATING_DISTANCE * parent.faceNormal;
            transform.localPosition = originalPosition;
            transform.localScale = LABEL_SIZE_SCALE * parent.GetIncircleRadius() * Vector3.one;
            Update();
        }

        public void SetHighlighted(bool isHighlighted) {
            GetComponent<MeshRenderer>().sharedMaterial = isHighlighted
                ? NumberMaterials.GetHighlightedNumberMaterial(currentNumber)
                : NumberMaterials.GetNumberMaterial(currentNumber);
        }

        private void Update() {
            if (isFreezed) return;

            if (!isCrazy) {
                ObserveCamera camera = Globals.instance.observeCamera;
                Vector3 cameraRight = Vector3.Cross(camera.transform.up, parent.faceCenter - camera.transform.position);
                transform.rotation = Quaternion.LookRotation(-parent.faceNormal, Vector3.Cross(cameraRight, parent.faceNormal));
                transform.localPosition = originalPosition;
                spinningSpeed = 0;
                waveFactor = 0;
            } else {
                spinningSpeed += (Random.Range(0f, 1f) - spinningSpeed / MAX_SPINNING_SPEED) * MAX_SPINNING_ACCELERATION * Time.deltaTime;
                waveFactor += spinningSpeed * Time.deltaTime;
                transform.rotation = Quaternion.AngleAxis(spinningSpeed * Time.deltaTime, parent.faceNormal) * transform.rotation;
                transform.localPosition = (FLOATING_DISTANCE + WAVE_HEIGHT * (-Mathf.Cos(waveFactor / WAVE_PERIOD * Mathf.PI * 2) + 1)) * parent.faceNormal;
            }
        }
    }
}