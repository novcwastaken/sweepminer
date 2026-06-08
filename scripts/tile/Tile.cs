using Godot;
using System;
using System.Collections.Generic;

public partial class Tile : TextureButton
{
	public bool isBomb = false;
	public bool isRevealed = false;
	public bool isFlagged = false;
	//public bool isEmpty => numberLabel.Text == "" && !isBomb;

	public int gridIndex = -1;
	public Vector2 gridCoordinates = Vector2.Zero;

	public List<int> adjacentTileIndexes = new List<int>();
	public List<Vector2> adjacentTileCoordinates = new List<Vector2>();

	public Dictionary<int, Color> numberColors = new Dictionary<int, Color>
	{
		{ 1, Color.FromHtml("#113ee0") },
		{ 2, Color.FromHtml("#0e5614") },
		{ 3, Color.FromHtml("#ec0000") },
		{ 4, Color.FromHtml("#160745") },
		{ 5, Color.FromHtml("#4c0000") },
		{ 6, Color.FromHtml("#155e4e") },
		{ 7, Color.FromHtml("#d75f00") },
		{ 8, Color.FromHtml("#ad00a7") },
	};

	public GameManager gameManager;
	//[Export] public TextureRect emptySprite;
	[Export] public TextureRect bombSprite;
	[Export] public TextureRect coverSprite;
	[Export] public TextureRect flagSprite;
	[Export] public TextureRect wrongFlagSprite;
	[Export] public ColorRect triggeredBombBG;
	[Export] public Label numberLabel;
	[Export] public Label debugIndexLabel;

	override public void _Ready()
	{
		adjacentTileCoordinates = GetAdjacentTileCoordinates();
		
		foreach (var atc in adjacentTileCoordinates) { // Optimization!!!!!!!!
			if (atc.X < 0 || atc.X >= gameManager.currentWidth || atc.Y < 0 || atc.Y >= gameManager.currentHeight) continue;

			int adjacentTileIndex = TileCoordinatesToIndex((int)atc.X, (int)atc.Y, gameManager.currentWidth);
			adjacentTileIndexes.Add(adjacentTileIndex);
		}
	}

	public void Reveal()
	{
		if (isRevealed || isFlagged) return;
		coverSprite.Visible = false;
		isRevealed = true;

		Disabled = true;
	}

	public void SetNumberColor()
	{
		if (!"0123456789".Contains(numberLabel.Text) || numberLabel.Text == "") return;

		int number = numberLabel.Text.ToInt();

		LabelSettings newLabelSettings = (LabelSettings)numberLabel.LabelSettings.Duplicate();
		newLabelSettings.FontColor = numberColors.TryGetValue(number, out Color color) ? color : new Color(0, 0, 0);

		numberLabel.LabelSettings = newLabelSettings;
	}

	public void UpdateBombState()
	{
		bombSprite.Visible = true ? isBomb : false;
	}

	public bool IsEmpty()
	{
		//return numberLabel.Text == "" && !isBomb;
		return numberLabel.Text == "";
	}

	public int GetAdjacentFlagCount()
	{
		int flagCount = 0;

		foreach (int ati in adjacentTileIndexes)
		{
			Tile adjacentTile = gameManager.tileGrid.GetChild<Tile>(ati);
			if (adjacentTile.isFlagged) flagCount++;
		}

		return flagCount;
	}

	public List<Vector2> GetAdjacentTileCoordinates()
	{
		return new List<Vector2> {
			gridCoordinates + Vector2.Up, // Top
			gridCoordinates + Vector2.Down, // Bottom
			gridCoordinates + Vector2.Left, // Left
			gridCoordinates + Vector2.Right, // Right
			gridCoordinates + Vector2.Up + Vector2.Left, // Top-Left
			gridCoordinates + Vector2.Up + Vector2.Right, // Top-Right
			gridCoordinates + Vector2.Down + Vector2.Left, // Bottom-Left
			gridCoordinates + Vector2.Down + Vector2.Right // Bottom-Right
		};
	}

	public Vector2 TileIndexToCoordinates(int index, int width)
	{
		int x = index % width;
		int y = index / width;
		
		return new Vector2(x, y);
	}

	public int TileCoordinatesToIndex(int x, int y, int width)
	{
		return y * width + x;
	}
}
