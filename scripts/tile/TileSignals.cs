using Godot;
using System;

public partial class Tile : Button
{
	bool isHovered = false;

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton && inputEvent.IsPressed())
		{
			if (!isHovered || gameManager.gameState != GameManager.GameState.ACTIVE) return;
			InputEventMouseButton inputEventMouseButton = inputEvent as InputEventMouseButton;

			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				if (isBomb)
				{
					gameManager.GameLost(gridIndex);
					return;
				}

				if (!isRevealed && !isFlagged) gameManager.FloodFill(gridIndex);
			} else if (inputEventMouseButton.ButtonIndex == MouseButton.Right)
			{
				if (isRevealed) return;

				isFlagged = !isFlagged;
				flagSprite.Visible = isFlagged;
			}  else if (inputEventMouseButton.ButtonIndex == MouseButton.Middle)
			{
				if (!isRevealed || isFlagged || IsEmpty()) return;
				if (GetAdjacentFlagCount() != numberLabel.Text.ToInt()) return;

				foreach (int ati in adjacentTileIndexes)
				{
					Tile adjacentTile = gameManager.tileGrid.GetChild<Tile>(ati);
					adjacentTile.Reveal();
				}
			}
		}
	}

	void OnMouseEntered()
	{
		isHovered = true;
	}

	void OnMouseExited()
	{
		isHovered = false;
	}

	void ____OnTileButtonDown()
	{
		GD.Print("@@@ ----- TILE DOWN -----");

		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			GD.Print("@@@ TILE LC");
		}
		
		if (Input.IsMouseButtonPressed(MouseButton.Right))
		{
			if (isFlagged)
			{
				flagSprite.Visible = false;
				isFlagged = false;
				GD.Print("@@@ FLAG OFF");
				
			} else
			{
				flagSprite.Visible = true;
				isFlagged = true;
				GD.Print("@@@ FLAG ON");
			}
			//isFlagged = !isFlagged;
			//flagSprite.Visible = isFlagged;
		}
		
		 if (Input.IsMouseButtonPressed(MouseButton.Middle))
		{
			GD.Print("@@@ TILE MC");
		}
	}

	
}
