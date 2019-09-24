using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MineFieldControl : MonoBehaviour {

    public float mouseRotationFactorNoAction = 1f;
    public float mouseRotationFactorPressing = -0.05f;
    public float mouseRotationControlDistance = 500f;
    public float mouseFastMovingTrackWindow = 0.1f;
    public float mouseFastMovingThreshold = 0.1f;
    public float explodeDelayInitial = 5f;
    public float explodeDelaySubsequentMin = 1f;
    public float explodeDelaySubsequentMax = 3f;
    public float explodeDetachRadius = 0.5f;
    public float explodeTriggerRadius = 1f;
    public float explodeRadialVelocity = 2f;
    public float explodeNormalVelocity = 2f;
    public float explodeAngularVelocity = 4f;

    private enum MouseState {
        NoAction,
        Pressing,
        DoublePressing,
    }

    public static MineFieldControl instance;

    public MineFieldModel mineField;
    public int numberOfMines;

    public MineSweeperUI UI;
    public GameObject tileBlockPrefab;

    private Dictionary<MineFieldModel.Tile, TileControl> tileBlocks = new Dictionary<MineFieldModel.Tile, TileControl>();
    private float startTime;
    private MouseState mouseState;
    private TileControl previousTileUnderMouse;
    private Queue<Tuple<float, Vector3>> recentMousePositions = new Queue<Tuple<float, Vector3>>();

    private void Awake() {
        instance = this;
    }

    public void InitField(RenderGeometry geometry, int numberOfMines) {
        ClearField();
        mineField = new MineFieldModel(geometry);
        mineField.onTileStateChanged += OnTileStateChanged;
        mineField.onWon += OnWon;
        mineField.onLost += OnLost;
        this.numberOfMines = numberOfMines;

        foreach (MineFieldModel.Tile tile in mineField.tiles.Values) {
            GameObject obj = Instantiate(tileBlockPrefab);
            TileControl tileBlock = obj.GetComponent<TileControl>();
            tileBlock.InitBlock(tile);
            tileBlocks[tile] = tileBlock;
        } 
    }

    public void ResetField() {
        mineField.Reset();
        mouseState = MouseState.NoAction;
        tileBlocks.Values.ForEach(t => t.ResetVisualState());
    }

    public void ClearField() {
        tileBlocks.Values.ForEach(tile => Destroy(tile.gameObject));
        tileBlocks.Clear();
    }

    private void Update() {
        if (mineField == null) return;

        if (Input.GetKeyDown(KeyCode.R)) {
            ResetField();
            return;
        }
        TileControl currentTileUnderMouse = GetTileUnderMouse();
        if (!mineField.hasWon && !mineField.hasLost) {
            if (mouseState == MouseState.NoAction) {
                UpdateDuringNoAction(currentTileUnderMouse);
            }
            if (mouseState == MouseState.Pressing) {
                UpdateDuringPressing(currentTileUnderMouse);
            }
            if (mouseState == MouseState.DoublePressing) {
                UpdateDuringDoublePressing(currentTileUnderMouse);
            }  
        }
        if (Input.GetMouseButtonDown(0) && currentTileUnderMouse != null && currentTileUnderMouse.tile.state == MineFieldModel.TileState.Exploded && currentTileUnderMouse.isExploding) {
            currentTileUnderMouse.ToggleExplodeCountDownPaused();
        }
        previousTileUnderMouse = currentTileUnderMouse;

        UpdateMouseRotationFactor();
        UpdateUI();
    }

    private void UpdateDuringNoAction(TileControl currentTileUnderMouse) {
        if (Input.GetMouseButtonDown(2) || (Input.GetMouseButtonDown(0) && Input.GetMouseButton(1)) || (Input.GetMouseButtonDown(1) && Input.GetMouseButton(0))) {
            StartDoublePressing(currentTileUnderMouse);
            return;
        }
        if (Input.GetMouseButtonDown(0) && currentTileUnderMouse != null && currentTileUnderMouse.tile.isClickable) {
            StartPressing(currentTileUnderMouse);
            return;
        }
        if (Input.GetMouseButtonDown(1) && currentTileUnderMouse != null) {
            mineField.ToggleFlag(currentTileUnderMouse.tile);
        }
    }

    private void UpdateDuringPressing(TileControl currentTileUnderMouse) {
        if (Input.GetMouseButtonDown(2) || (Input.GetMouseButtonDown(1) && Input.GetMouseButton(0))) {
            StartDoublePressing(currentTileUnderMouse);
            return;
        }
        if (!Input.GetMouseButton(0)) {
            EndPressing(currentTileUnderMouse);
            return;
        }
        if (currentTileUnderMouse != previousTileUnderMouse) {
            if (currentTileUnderMouse != null) {
                StartPressing(currentTileUnderMouse);
            } else {
                mineField.Unpress();
            }
        }
        UnpressIfMovingTooFast();
    }

    private void UpdateDuringDoublePressing(TileControl currentTileUnderMouse) {
        if (currentTileUnderMouse != previousTileUnderMouse) {
            if (currentTileUnderMouse != null) {
                StartDoublePressing(currentTileUnderMouse);
            } else {
                mineField.Unpress();
            }
        }
        if (!(Input.GetMouseButton(0) && Input.GetMouseButton(1)) && !Input.GetMouseButton(2)) {
            EndDoublePressing(currentTileUnderMouse);
            return;
        }
        UnpressIfMovingTooFast();
    }

    private void UpdateMouseRotationFactor() {
        bool inverseControl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        Globals.instance.observeCamera.mouseRotationFactor =
            mouseState == MouseState.NoAction ? mouseRotationFactorNoAction : mouseRotationFactorPressing;
        Globals.instance.observeCamera.mouseRotationControlDistance =
            inverseControl ? -mouseRotationControlDistance : mouseRotationControlDistance;
    }

    private void UpdateUI() {
        if (mineField != null && mineField.hasGeneratedMine) {
            if (!mineField.hasWon && !mineField.hasLost) {
                UI.SetTime(Mathf.CeilToInt(Time.time - startTime));
            }
        } else {
            UI.SetTime(0);
        }

        if (mineField != null) {
            UI.SetMineCount(numberOfMines - mineField.GetNumberOfFlaggedTiles(), numberOfMines);
        } else {
            UI.SetMineCount(0, 0);
        }
    }

    private void StartPressing(TileControl tileBlock) {
        mineField.Press(tileBlock.tile, false);
        mouseState = MouseState.Pressing;
    }

    private void StartDoublePressing(TileControl tileBlock) {
        if (tileBlock != null) {
            mineField.Press(tileBlock.tile, true);
        }
        mouseState = MouseState.DoublePressing;
    }

    private void EndPressing(TileControl tileBlock) {
        if (tileBlock != null && tileBlock.tile.isClickable) {
            if (!mineField.hasGeneratedMine) {
                mineField.GenerateMines(numberOfMines, tileBlock.tile);
                startTime = Time.time;
            }
            mineField.Open(tileBlock.tile);
        }
        mineField.Unpress();
        mouseState = MouseState.NoAction;
    }

    private void EndDoublePressing(TileControl tileBlock) {
        if (tileBlock != null) {
            mineField.TryOpenAdjacentTiles(tileBlock.tile);
        }
        mineField.Unpress();
        mouseState = MouseState.NoAction;
    }

    private void UnpressIfMovingTooFast() {
        while (recentMousePositions.Count > 0 && recentMousePositions.Peek().Item1 < Time.unscaledTime - mouseFastMovingTrackWindow) {
            recentMousePositions.Dequeue();
        }
        foreach (Tuple<float, Vector3> mousePositionPair in recentMousePositions.Reverse()) {
            if (Vector3.Distance(Input.mousePosition, mousePositionPair.Item2) > Mathf.Min(Screen.height, Screen.width) * mouseFastMovingThreshold) {
                if (mouseState == MouseState.Pressing) EndPressing(null);
                if (mouseState == MouseState.DoublePressing) EndDoublePressing(null);
            }
        }
        recentMousePositions.Enqueue(Tuple.Create(Time.unscaledTime, Input.mousePosition));
    }

    private void OnTileStateChanged(MineFieldModel.Tile tile) {
        tileBlocks[tile].UpdateState();
    }

    private void OnWon() {
        tileBlocks.Values.ForEach(t => t.SetCrazy(true));
    }

    private void OnLost() {
        TileControl triggeredMineBlock = tileBlocks[mineField.tiles.Values.Where(t => t.state == MineFieldModel.TileState.Exploded).Single()];
        triggeredMineBlock.CountDownToExplode(explodeDelayInitial, () => Explode(triggeredMineBlock.transform.position));
    }

    private void Explode(Vector3 center) {
        foreach (TileControl tile in tileBlocks.Values) {
            float distance = Vector3.Distance(tile.transform.position, center);
            bool wasAttached = false;

            if (distance < explodeDetachRadius) {
                tile.Detach();
                wasAttached = true;
            }
            if (distance < explodeTriggerRadius) {
                float velocityFactor = 1 - distance / explodeTriggerRadius;

                Vector3 dir = (tile.transform.position - center).normalized;
                Vector3 velocity;
                if (wasAttached) {
                    velocity = velocityFactor * explodeRadialVelocity * dir + velocityFactor * explodeNormalVelocity * tile.faceNormal;
                } else {
                    velocity = velocityFactor * explodeRadialVelocity * (dir.sqrMagnitude != 0 ? dir : tile.transform.TransformDirection(tile.faceNormal));
                }
                Vector3 angularVelocity = velocityFactor * explodeAngularVelocity * UnityEngine.Random.insideUnitSphere;
                tile.ApplyBoost(velocity, angularVelocity);

                if (tile.tile.state == MineFieldModel.TileState.MineRevealed) {
                    tile.CountDownToExplode(UnityEngine.Random.Range(explodeDelaySubsequentMin, explodeDelaySubsequentMax), () => Explode(tile.transform.position));
                }
            }
        }
    }

    private TileControl GetTileUnderMouse() {
        Ray ray = Globals.instance.observeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit hitInfo);

        return hitInfo.collider?.GetComponentInParent<TileControl>();
    }
}
