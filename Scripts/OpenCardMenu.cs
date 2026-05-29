using Godot;
using System;

public partial class OpenCardMenu : Control
{
	[Export] public Button OpenButton;
	[Export] public Label CooldownLabel;

	private const string SAVE_PATH = "user://Saves/cooldown.save";
	private const double COOLDOWN_TIME = 600.0; // seconds
	private double timeRemaining = 0;
	private bool onCooldown = false;
	private PackedScene cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!onCooldown){
			return;
		}

		timeRemaining -= delta;

		if (timeRemaining <= 0)
		{
			EndCooldown();
		}
		else
		{
			int minutes = (int)(timeRemaining / 60);
			int seconds = (int)(timeRemaining % 60);
			CooldownLabel.Text = $"{minutes}:{seconds}";
		}
	}
	
	public void _on_back_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	public void _on_open_button_pressed()
	{
		if (onCooldown)
		{
			return;
		}

		StartCooldown();
	}

	public void StartCooldown()
	{
		onCooldown = true;
		timeRemaining = COOLDOWN_TIME;
		OpenButton.Disabled = true;
		CooldownLabel.Visible = true;
	}

	public void EndCooldown()
	{
		onCooldown = false;
		timeRemaining = 0;
		OpenButton.Disabled = false;
		CooldownLabel.Visible = false;
		CooldownLabel.Text = "";
	}

}
