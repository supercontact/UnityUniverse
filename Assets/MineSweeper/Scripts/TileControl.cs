using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MineSweeper {
    public class TileControl : MonoBehaviour {

        public MineFieldModel.Tile tile;

        public TileBlockControl tileBlock;
        public TileBaseControl tileBase;
        public FlagControl flag;
        public LabelControl number;
        public LabelControl mark;
        public MineControl mine;

        public Vector3 faceCenter { get; private set; }
        public Vector3 faceNormal { get; private set; }
        public float tileHeight;
        public float tileTopToEdgeDistange;

        public bool isExploding { get; private set; }
        public bool isExplodingPaused { get; private set; }
        public bool hasExploded { get; private set; }

        private bool numberInitialized = false;
        private Rigidbody physicsBody;

        public void InitBlock(MineFieldModel.Tile tile, float tileHeight = 0.1f, float tileSideAngle = 60f) {
            this.tile = tile;
            faceCenter = tile.face.CalculateCenter();
            faceNormal = tile.face.CalculateNormal();
            this.tileHeight = tileHeight;
            tileTopToEdgeDistange = tileHeight * Mathf.Cos(tileSideAngle * Mathf.Deg2Rad);

            transform.position = faceCenter;

            tileBlock.Init();
            tileBase.Init();
            flag.Init();
            mark.InitWithMark();
            mine.Init();
            numberInitialized = false;
        }

        public void UpdateState() {
            tileBlock.gameObject.SetActive(!tile.isOpened);
            tileBlock.SetPressed(tile.isPressed);
            tileBase.SetExploded(tile.state == MineFieldModel.TileState.Exploded);

            bool numberShown = tile.state == MineFieldModel.TileState.Discovered && tile.mineNumber > 0;
            number.gameObject.SetActive(numberShown);
            if (numberShown) {
                if (!numberInitialized) {
                    number.InitWithNumber(tile.mineNumber);
                }
                number.SetHighlighted(tile.isHighlighted);
            }
            mark.gameObject.SetActive(tile.state == MineFieldModel.TileState.Marked);
            flag.gameObject.SetActive(tile.state == MineFieldModel.TileState.Flagged || tile.state == MineFieldModel.TileState.WronglyFlagged);
            if (flag.gameObject.activeSelf) {
                if (tile.state == MineFieldModel.TileState.WronglyFlagged) {
                    flag.SetFlagType(FlagControl.FlagType.Wrong);
                } else if (tile.isHighlighted) {
                    flag.SetFlagType(FlagControl.FlagType.Hightlighted);
                } else {
                    flag.SetFlagType(FlagControl.FlagType.Normal);
                }
            }
            mine.gameObject.SetActive(tile.state == MineFieldModel.TileState.Exploded || tile.state == MineFieldModel.TileState.MineRevealed);
        }

        public void SetCrazy(bool isCrazy) {
            number.isCrazy = isCrazy;
            flag.isCrazy = isCrazy;
        }

        public void CountDownToExplode(float duration, Action explodeCallback) {
            if (isExploding || hasExploded) return;

            IEnumerator<YieldInstruction> waitAndExplode() {
                float remainingDuration = duration;
                while (remainingDuration > 0) {
                    yield return new WaitForSeconds(0.1f);
                    if (!isExploding) yield break;
                    if (!isExplodingPaused) {
                        remainingDuration -= 0.1f;
                    }
                }
                hasExploded = true;
                isExploding = false;
                mine.Explode();
                explodeCallback();
            }

            isExploding = true;
            mine.StartBlinking(duration);
            StartCoroutine(waitAndExplode());
        }

        public void ToggleExplodeCountDownPaused() {
            if (!isExploding) return;
            isExplodingPaused = !isExplodingPaused;
            mine.SetBlinkingPaused(isExplodingPaused);
        }

        public void Detach() {
            // Small variation to position and rotation to avoid abnormal collision result.
            transform.position += UnityEngine.Random.Range(0, 0.05f) * faceNormal;
            transform.rotation *= Quaternion.Euler(UnityEngine.Random.Range(0, 0.1f), UnityEngine.Random.Range(0, 0.1f), UnityEngine.Random.Range(0, 0.1f));

            if (physicsBody == null) {
                physicsBody = gameObject.AddComponent<Rigidbody>();
                physicsBody.drag = 0.01f;
                physicsBody.angularDrag = 0.01f;
                physicsBody.useGravity = false;
                physicsBody.maxDepenetrationVelocity = 10;
            }
            flag.isFreezed = true;
            number.isFreezed = true;
            mark.isFreezed = true;
        }

        public void ApplyBoost(Vector3 velocity, Vector3 angularVelocity) {
            if (physicsBody == null) return;
            physicsBody.AddForce(velocity, ForceMode.VelocityChange);
            physicsBody.AddTorque(angularVelocity, ForceMode.VelocityChange);
        }

        public void ResetVisualState() {
            if (physicsBody != null) {
                Destroy(physicsBody);
                physicsBody = null;
            }
            transform.position = faceCenter;
            transform.rotation = Quaternion.identity;
            flag.isFreezed = false;
            number.isFreezed = false;
            mark.isFreezed = false;
            mine.StopBlinking();
            isExploding = false;
            isExplodingPaused = false;
            hasExploded = false;
            SetCrazy(false);
        }

        public float GetIncircleRadius() {
            return tile.face.edges.Select(e => Vector3.ProjectOnPlane(faceCenter - e.vertex.p, e.vector).magnitude).Min();
        }

        public float GetAverageRadius() {
            return tile.face.edges.Select(e => (e.vertex.p - faceCenter).magnitude).Average();
        }
    }
}