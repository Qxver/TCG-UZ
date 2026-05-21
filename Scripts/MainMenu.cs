using Godot;
using System;

public partial class MainMenu : Control
{
	public void _on_collection_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/CardCollection.tscn");
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
