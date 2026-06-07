using Godot;
using System;
using System.Collections.Generic;

public partial class OpenCardMenu : Control
{
	[Export] public Button OpenButton;
	[Export] public Label CooldownLabel;
	[Export] public Control ResultPanel;
	[Export] public HBoxContainer CardContainer;
	[Export] public OpenCardPack PackOpener;

	private const string SAVE_PATH = "user://Saves/cooldown.save";
	private const double COOLDOWN_TIME = 300.0; // seconds
	private double timeRemaining = 0;
	private bool onCooldown = false;
	private PackedScene cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ResultPanel.Visible = false;
		CooldownLabel.Visible = false;
		LoadCooldown();
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

		var cards = PackOpener.OpenPack();
		ShowCards(cards);
		StartCooldown();
	}

	private void ShowCards(List<CardData> cards)
	{
		// Wyczyść poprzednie karty
		foreach (Node child in CardContainer.GetChildren())
			child.QueueFree();

		ResultPanel.Visible = true;

		foreach (var data in cards)
		{
			var card = cardScene.Instantiate<Card>();
			card.SetCardData(data);
			CardContainer.AddChild(card);

			var portrait = card.GetNodeOrNull<Sprite2D>("FrontFace/Portrait");
			if (portrait != null)
				portrait.Texture = data.Portrait;

			var attackLabel = card.GetNodeOrNull<Label>("Stats/AttackLabel");
			var healthLabel = card.GetNodeOrNull<Label>("Stats/HealthLabel");
			var costLabel   = card.GetNodeOrNull<Label>("Stats/CostLabel");
			if (attackLabel != null) attackLabel.Text = data.Attack.ToString();
			if (healthLabel != null) healthLabel.Text = data.Health.ToString();
			if (costLabel   != null) costLabel.Text   = data.Cost.ToString();
		}
	}


	private void LoadCooldown()
	{
		if (!FileAccess.FileExists(SAVE_PATH))
		{
			return;
		} 

		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
		string content = file.GetAsText().Trim();

		if (!long.TryParse(content, out long endTime)) return;

		long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		double remaining = endTime - now;

		if (remaining > 0)
		{
			onCooldown = true;
			timeRemaining = remaining;
			OpenButton.Disabled = true;
			CooldownLabel.Visible = true;
		}
	}
	
	private void SaveCooldown()
	{
		long endTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)COOLDOWN_TIME;
		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
		file.StoreString(endTime.ToString());
	}
	
	public void StartCooldown()
	{
		onCooldown = true;
		timeRemaining = COOLDOWN_TIME;
		OpenButton.Disabled = true;
		CooldownLabel.Visible = true;
		SaveCooldown();
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
