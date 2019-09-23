using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MineFieldModel {

    public enum TileState {
        Undiscovered,
        Discovered,
        Flagged,
        Marked,
        Exploded,
        MineRevealed,
        WronglyFlagged,
    }

    public class Tile {
        public readonly MineFieldModel field;
        public readonly Face face;

        public bool hasMine;

        private TileState _state = TileState.Undiscovered;
        public TileState state {
            get => _state;
            set {
                if (value != _state) {
                    _state = value;
                    field.onTileStateChanged?.Invoke(this);
                }
            }
        }

        public bool isClickable => state == TileState.Undiscovered || state == TileState.Marked;
        public bool isPressed => field.pressedTiles.Contains(this);
        public bool isHighlighted => field.highlightedTiles.Contains(this);
        public bool isOpened => state == TileState.Discovered || state == TileState.Exploded || state == TileState.MineRevealed || state == TileState.WronglyFlagged;

        public List<Tile> adjacentTiles => face.edges
            .SelectMany(e => e.vertex.edges)
            .Where(e => !e.isBoundary)
            .Select(e => field.tiles[e.face])
            .Distinct()
            .Where(t => t != this)
            .ToList();

        public int mineNumber => adjacentTiles.Where(t => t.hasMine).Count();

        public Tile(Face face, MineFieldModel field) {
            this.face = face;
            this.field = field;
        }

        public void Reset() {
            hasMine = false;
            state = TileState.Undiscovered;
        }
    }

    public RenderGeometry geometry;
    public Dictionary<Face, Tile> tiles = new Dictionary<Face, Tile>();
    public HashSet<Tile> pressedTiles = new HashSet<Tile>();
    public HashSet<Tile> highlightedTiles = new HashSet<Tile>();

    public bool hasGeneratedMine { get; private set; }
    public bool hasWon { get; private set; }
    public bool hasLost { get; private set; }

    public event Action<Tile> onTileStateChanged;
    public event Action onWon;
    public event Action onLost;

    public MineFieldModel(RenderGeometry geometry) {
        InitField(geometry);
    }

    public void InitField(RenderGeometry geometry) {
        Reset();
        this.geometry = geometry;
        tiles.Clear();
        foreach (Face face in geometry.faces) {
            tiles.Add(face, new Tile(face, this));
        }
    } 

    public void Reset() {
        tiles.Values.ForEach(t => t.Reset());
        pressedTiles.Clear();
        highlightedTiles.Clear();
        hasGeneratedMine = false;
        hasLost = false;
        hasWon = false;
    }

    public void GenerateMines(int number, Tile initialTile) {
        ClearMines();
        var excludedTiles = new HashSet<Tile>(initialTile.adjacentTiles) { initialTile };
        tiles.Values
            .Where(t => !excludedTiles.Contains(t))
            .OrderBy(_ => UnityEngine.Random.value)
            .Take(number)
            .ForEach(t => t.hasMine = true);
        hasGeneratedMine = true;
    }

    public void ClearMines() {
        tiles.Values.Select(t => t.hasMine = false);
    }

    public void Press(Tile tile, bool isDoublePress) {
        var oldPressedTiles = new HashSet<Tile>(pressedTiles);
        var oldHighlightedTiles = new HashSet<Tile>(highlightedTiles);
        pressedTiles.Clear();
        highlightedTiles.Clear();

        if (tile.isClickable) {
            pressedTiles.Add(tile);
        }
        if (isDoublePress) {
            pressedTiles.UnionWith(tile.adjacentTiles.Where(t => t.isClickable));
            highlightedTiles.UnionWith(tile.adjacentTiles.Where(t => t.state == TileState.Flagged));
            if (tile.state == TileState.Discovered || tile.state == TileState.Flagged) {
                highlightedTiles.Add(tile);
            }
        }
        pressedTiles.Except(oldPressedTiles).ForEach(t => onTileStateChanged?.Invoke(t));
        oldPressedTiles.Except(pressedTiles).ForEach(t => onTileStateChanged?.Invoke(t));
        highlightedTiles.Except(oldHighlightedTiles).ForEach(t => onTileStateChanged?.Invoke(t));
        oldHighlightedTiles.Except(highlightedTiles).ForEach(t => onTileStateChanged?.Invoke(t));
    }

    public void Unpress() {
        var oldPressedTiles = new List<Tile>(pressedTiles);
        pressedTiles.Clear();
        oldPressedTiles.ForEach(t => onTileStateChanged?.Invoke(t));

        var oldHighlightedTiles = new List<Tile>(highlightedTiles);
        highlightedTiles.Clear();
        oldHighlightedTiles.ForEach(t => onTileStateChanged?.Invoke(t));
    }

    public void Open(Tile tile) {
        if (!tile.isClickable) return;
        if (tile.hasMine) {
            Lose(tile);
            return;
        }
        tile.state = TileState.Discovered;
        CheckIfWin();
        if (tile.mineNumber == 0) {
            tile.adjacentTiles.ForEach(Open);
        }
    }

    public void TryOpenAdjacentTiles(Tile tile) {
        if (tile.state != TileState.Discovered) return;

        if (tile.adjacentTiles.Where(t => t.state == TileState.Flagged).Count() == tile.mineNumber) {
            tile.adjacentTiles.Where(t => t.isClickable).ForEach(Open);
        }
    }

    public void ToggleFlag(Tile tile) {
        if (tile.state == TileState.Undiscovered) {
            tile.state = TileState.Flagged;
            CheckIfWin();
        } else if (tile.state == TileState.Flagged) {
            tile.state = TileState.Marked;
        } else if (tile.state == TileState.Marked) {
            tile.state = TileState.Undiscovered;
        }
    }

    public int GetNumberOfFlaggedTiles() {
        return tiles.Values.Where(t => t.state == TileState.Flagged).Count();
    }

    private void CheckIfWin() {
        bool allSafeTileOpened = tiles.Values.Where(t => !t.hasMine).All(t => t.state == TileState.Discovered);
        bool allMinesCorrectlyFlagged =
            tiles.Values.Where(t => t.hasMine).All(t => t.state == TileState.Flagged) &&
            tiles.Values.Where(t => t.state == TileState.Flagged).All(t => t.hasMine);

        if (allSafeTileOpened || allMinesCorrectlyFlagged) {
            Win();
        }
    }

    private void Win() {
        tiles.Values.Where(t => t.hasMine).ForEach(t => t.state = TileState.Flagged);
        tiles.Values.Where(t => !t.isOpened && !t.hasMine).ForEach(t => t.state = TileState.Discovered);
        hasWon = true;
        onWon?.Invoke();
    }

    private void Lose(Tile explodedTile) {
        explodedTile.state = TileState.Exploded;
        tiles.Values
            .Where(t => t.hasMine && t.isClickable)
            .ForEach(t => t.state = TileState.MineRevealed);
        tiles.Values
            .Where(t => !t.hasMine && t.state == TileState.Flagged)
            .ForEach(t => t.state = TileState.WronglyFlagged);
        hasLost = true;
        onLost?.Invoke();
    }
}
