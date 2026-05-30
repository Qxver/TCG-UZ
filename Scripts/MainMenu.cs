using Godot;
using System;

public partial class MainMenu : Control
{
	public void _on_collection_menu_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/CardCollection.tscn");
	}
	
	public void _on_open_card_menu_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/OpenCard.tscn");
	}
	
	public void _on_exit_button_pressed()
	{
		GetTree().Quit();
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
