using Godot;
using System;

public partial class Tile : TextureButton {

	void OnGuiInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton && inputEvent.IsPressed() && !gameManager.isGameOver)
		{
			switch ((inputEvent as InputEventMouseButton).ButtonIndex) {
				case MouseButton.Left:
					if (gameManager.gameState == GameManager.GameState.INACTIVE)
					{
						gameManager.GenerateBombs(gameManager.currentWidth, gameManager.currentHeight, gameManager.currentBombCount, gridIndex);

						if (!isRevealed && !isFlagged) gameManager.FloodFill(gridIndex);
					}

					if (isBomb)
					{
						gameManager.GameLost(gridIndex);
						return;
					}

					break;

				case MouseButton.Right:
					if (isRevealed) return;

					isFlagged = !isFlagged;
					flagSprite.Visible = isFlagged;

					gameManager.currentRemainingFlagCount = gameManager.currentBombCount + (isFlagged ? -1 : 1);
					gameManager.flagCountLabel.Text = gameManager.PadTo3Digits(gameManager.currentRemainingFlagCount);

					break;

				case MouseButton.Middle:
					if (!isRevealed || isFlagged || IsEmpty()) return;
					if (GetAdjacentFlagCount() != numberLabel.Text.ToInt()) return;

					foreach (int ati in adjacentTileIndexes)
					{
						Tile adjacentTile = gameManager.tileGrid.GetChild<Tile>(ati);
						adjacentTile.Reveal();
					}

					break;
			}
		}
	}	
}
