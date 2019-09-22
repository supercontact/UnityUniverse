using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MineFieldController : MonoBehaviour {

    public float mouseRotationFactorNoAction = 1f;
    public float mouseRotationFactorPressing = -0.05f;
    public float mouseRotationControlDistance = 500f;
    public float mouseFastMovingTrackWindow = 0.1f;
    public float mouseFastMovingThreshold = 0.1f;

    private enum MouseState {
        NoAction,
        Pressing,
        DoublePressing,
    }

    public static MineFieldController instance;

    public MineFieldModel mineField;
    public int numberOfMines;

    public MineSweeperUI UI;
    public GameObject tileBlockPrefab;

    private Dictionary<MineFieldModel.Tile, TileBlock> tileBlocks = new Dictionary<MineFieldModel.Tile, TileBlock>();
    private float startTime;
    private MouseState mouseState;
    private TileBlock previousTileUnderMouse;
    private Queue<Tuple<float, Vector3>> recentMousePositions = new Queue<Tuple<float, Vector3>>();

    private void Awake() {
        instance = this;
    }

    public void InitField(RenderGeometry geometry, int numberOfMine) {
        ClearField();
        mineField = new MineFieldModel(geometry);
        mineField.onTileStateChanged += OnTileStateChanged;
        mineField.onWon += OnWon;
        mineField.onLost += OnLost;
        this.numberOfMines = numberOfMine;

        foreach (MineFieldModel.Tile tile in mineField.tiles.Values) {
            GameObject obj = Instantiate(tileBlockPrefab);
            TileBlock tileBlock = obj.GetComponent<TileBlock>();
            tileBlock.InitBlock(tile);
            tileBlocks[tile] = tileBlock;
        } 
    }

    public void ResetField() {
        mineField.Reset();
        mouseState = MouseState.NoAction;
        tileBlocks.Values.ForEach(t => t.SetCrazy(false));
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
        if (!mineField.hasWon && !mineField.hasLost) {
            TileBlock currentTileUnderMouse = GetTileUnderMouse();
            if (mouseState == MouseState.NoAction) {
                UpdateDuringNoAction(currentTileUnderMouse);
            }
            if (mouseState == MouseState.Pressing) {
                UpdateDuringPressing(currentTileUnderMouse);
            }
            if (mouseState == MouseState.DoublePressing) {
                UpdateDuringDoublePressing(currentTileUnderMouse);
            }
            previousTileUnderMouse = currentTileUnderMouse;
        }    
        UpdateMouseRotationFactor();
        UpdateUI();
    }

    private void UpdateDuringNoAction(TileBlock currentTileUnderMouse) {
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

    private void UpdateDuringPressing(TileBlock currentTileUnderMouse) {
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

    private void UpdateDuringDoublePressing(TileBlock currentTileUnderMouse) {
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

    private void StartPressing(TileBlock tileBlock) {
        mineField.Press(tileBlock.tile, false);
        mouseState = MouseState.Pressing;
    }

    private void StartDoublePressing(TileBlock tileBlock) {
        if (tileBlock != null) {
            mineField.Press(tileBlock.tile, true);
        }
        mouseState = MouseState.DoublePressing;
    }

    private void EndPressing(TileBlock tileBlock) {
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

    private void EndDoublePressing(TileBlock tileBlock) {
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

    }

    private TileBlock GetTileUnderMouse() {
        Ray ray = Globals.instance.observeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit hitInfo);

        return hitInfo.collider?.GetComponentInParent<TileBlock>();
    }
}
