using Godot;
using System;

public partial class MainMenu : Control
{
	public void _on_collection_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/CardCollection.tscn");
	}
	
	public void _on_button_2_pressed()
	{
		CardCollection.Instance.AddCard(CardDatabase.Instance.AllCards[0]);
	}
	
	public void OnPlayButtonPressed(){
		GetTree().ChangeSceneToFile("res://Scenes/Game.tscn");
	}
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
