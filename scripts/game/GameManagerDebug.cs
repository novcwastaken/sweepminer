using Godot;
using System;

public partial class GameManager : Node
{
	void OnGenerateMapButtonPressed()
	{
		int newWidth = widthInput.Text.ToInt();
		int newHeight = heightInput.Text.ToInt();
		int newMineCount = bombCountInput.Text.ToInt();

		foreach (var child in tileGrid.GetChildren())
		{
			child.Free(); // Use Free(), NOT evil ass rape QueueFree()
		}
		
		var startTime = Time.GetTicksMsec();
		GenerateMap(newWidth, newHeight, newMineCount);
		var endTime = Time.GetTicksMsec();

		double elapsed = endTime - startTime;
		genTimeLabel.Text = $"Took {elapsed} ms";
	}
}
