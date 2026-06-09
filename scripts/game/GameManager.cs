using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public int currentWidth = 9;
	public int currentHeight = 9;
	public int currentBombCount = 10;

	public int currentRemainingFlagCount = 0;

	private List<int> visitedFloodFillIndexes = new List<int>();

	Random random = new Random();

	public GameState gameState = GameState.INACTIVE;
	public bool isGameOver = false;

	[Export] public GridContainer tileGrid;
	[Export] PackedScene tileScene;
	[Export] NinePatchRect tileGridBorder;
	[Export] NinePatchRect infoPanelBorder;

	[Export] public Label flagCountLabel;

	[ExportGroup("Debug")]
	[ExportSubgroup("Map Generation")]
	[Export] TextEdit widthInput;
	[Export] TextEdit heightInput;
	[Export] TextEdit bombCountInput;
	[Export] Timer genTimer;
	[Export] Label genTimeLabel;
	[Export] Label stateLabel;

	public enum GameState
	{
		INACTIVE, ACTIVE, WON, LOST
	}

	public override void _Ready()
	{
		GenerateEmptyMap(9, 9, 10);
	}

	public override void _Process(double _delta)
	{
		stateLabel.Text = $"State: {gameState}";
	}

	public void GenerateEmptyMap(int width, int height, int mineCount)
	{
		gameState = GameState.INACTIVE;
		UpdateIsGameOver();

		currentWidth = width;
		currentHeight = height;
		currentBombCount = mineCount;

		tileGrid.Columns = width;

		// Adjust 9-slice grid border & 9-slice info panel border
		tileGridBorder.SetSize(new Vector2(16 + currentWidth * 16, 16 + currentHeight * 16));
		tileGridBorder.SetPosition(GetViewport().GetVisibleRect().Size / 2 - tileGridBorder.GetSize() / 2);

		infoPanelBorder.SetSize(new Vector2(16 + currentWidth * 16, 40));
		infoPanelBorder.SetPosition(GetViewport().GetVisibleRect().Size / 2 - infoPanelBorder.GetSize() / 2 + new Vector2(0, -(tileGridBorder.Size.Y / 2) - 12));

		currentRemainingFlagCount = 0;
		flagCountLabel.Text = PadTo3Digits(currentBombCount);

		// Add tiles
		for (int i = 0; i < width * height; i++)
		{
			Tile newTile = tileScene.Instantiate<Tile>();		
			
			newTile.gridIndex = i;
			newTile.debugIndexLabel.Text = i.ToString();
			newTile.gridCoordinates = newTile.TileIndexToCoordinates(i, width);
			newTile.gameManager = this;

			newTile.Name = $"Tile_{i}";
			tileGrid.AddChild(newTile);
		}
	}

	public void GenerateBombs(int width, int height, int mineCount, int firstClickIndex)
	{
		gameState = GameState.ACTIVE;
		UpdateIsGameOver();

		// Set safe indexes for guaranteed safe open
		List<int> safeIndexes = [firstClickIndex, .. tileGrid.GetChild<Tile>(firstClickIndex).adjacentTileIndexes];

		// Add bombs
		mineCount = Math.Min(mineCount, width * height);

		for (int i = 0; i < mineCount; i++)
		{
			int index = random.Next(0, width * height);
			Tile newBombTile = tileGrid.GetChild<Tile>(index);

			while (newBombTile.isBomb || safeIndexes.Contains(index))
			{
				index = random.Next(0, width * height);
				newBombTile = tileGrid.GetChild<Tile>(index);
			}

			newBombTile.isBomb = true;
			newBombTile.UpdateBombState();
		}

		// Set numbers
		for (int i = 0; i < width * height; i++)
		{
			int adjacentBombCount = 0;
			Tile tile = tileGrid.GetChild<Tile>(i);

			foreach (var ati in tile.adjacentTileIndexes)
			{
				Tile adjacentTile = tileGrid.GetChild<Tile>(ati);
				if (adjacentTile.isBomb) adjacentBombCount++;
			}

			tile.numberLabel.Text = adjacentBombCount > 0 && !tile.isBomb ? adjacentBombCount.ToString() : "";
			tile.SetNumberColor();
		}
	}

	public void FloodFill(int initialIndex)
	{
		visitedFloodFillIndexes.Clear();
		FloodFillRecursive(initialIndex);
	}

	private void FloodFillRecursive(int initialIndex)
	{
		if (visitedFloodFillIndexes.Contains(initialIndex)) return;
		visitedFloodFillIndexes.Add(initialIndex);

		Tile initialTile = tileGrid.GetChild<Tile>(initialIndex);

		if (!initialTile.isBomb && !initialTile.isFlagged)
		{
			initialTile.Reveal();

			if (!initialTile.IsEmpty()) return;

			foreach (int adjacentTileIndex in initialTile.adjacentTileIndexes)
			{
				FloodFillRecursive(adjacentTileIndex);
			}
		}		
	}

	private void UpdateIsGameOver()
	{
		if (gameState == GameState.LOST || gameState == GameState.WON) isGameOver = true;
	}

	public void GameLost(int triggeredBombTileIndex)
	{
		gameState = GameState.LOST;
		UpdateIsGameOver();

		Tile triggeredBombTile = tileGrid.GetChild<Tile>(triggeredBombTileIndex);
		triggeredBombTile.Reveal();
		triggeredBombTile.triggeredBombBG.Visible = true;

		foreach (var t in tileGrid.GetChildren())
		{
			Tile tile = t as Tile;
			if (tile.isBomb) tile.Reveal();

			if (tile.isFlagged && !tile.isBomb)
			{
				tile.flagSprite.Visible = false;
				tile.wrongFlagSprite.Visible = true;
			}
		}
	}

	public string PadTo3Digits(int number)
	{
		if (number < 10) return "00" + number;
		else if (number < 100) return "0" + number;
		else return number.ToString();
	}

	void OnResetButtonPressed()
	{
		GetTree().ReloadCurrentScene();
	}
}
