using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	public int currentWidth = 9;
	public int currentHeight = 9;

	private List<int> visitedFloodFillIndexes = new List<int>();

	Random random = new Random();

	[Export] public GridContainer tileGrid;
	[Export] PackedScene tileScene;

	[ExportGroup("Debug")]
	[ExportSubgroup("Map Generation")]
	[Export] TextEdit widthInput;
	[Export] TextEdit heightInput;
	[Export] TextEdit bombCountInput;
	[Export] Timer genTimer;
	[Export] Label genTimeLabel;

	public override void _Ready()
	{
		GenerateMap(9, 9, 10);
	}

	void GenerateMap(int width, int height, int mineCount)
	{
		currentWidth = width;
		currentHeight = height;

		tileGrid.Columns = width;

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

		// Add bombs
		mineCount = Math.Min(mineCount, width * height);

		for (int i = 0; i < mineCount; i++)
		{
			int index = random.Next(0, width * height);
			Tile newBombTile = tileGrid.GetChild<Tile>(index);

			while (newBombTile.isBomb)
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
		initialTile.Reveal();

		GD.Print($"@@@ Initial tile adjacent indexes: {string.Join(", ", initialTile.adjacentTileIndexes)}");

		GD.Print($"@@@ Looping through adjacent tiles around initial index {initialIndex}");
		foreach (int adjacentTileIndex in initialTile.adjacentTileIndexes)
		{
			GD.Print($"@@@ --- Current iteration index: {adjacentTileIndex}");

			Tile currentTile = tileGrid.GetChild<Tile>(adjacentTileIndex);
			if (!currentTile.IsEmpty()) continue; // !!!
			
			currentTile.Reveal();

			GD.Print($"@@@ --- Is empty? {currentTile.IsEmpty()}");
			GD.Print($"@@@ --- Is flagged? {currentTile.isFlagged}");

			if (currentTile.IsEmpty() && !currentTile.isFlagged)
			{
				GD.Print($"@@@ --- RECURSIVE FLOOD FILL TRIGGERED FOR INDEX {adjacentTileIndex}");
				FloodFillRecursive(adjacentTileIndex);
			}
		}
	}
}
