using Godot;
using System;

public partial class Card : Control
{
	[Signal] public delegate void CardOnHoverEnteredEventHandler(Card card);
	[Signal] public delegate void CardOnHoverExitedEventHandler(Card card);
	public bool isPlaced = false;
	public Vector2 positionInHand;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var cardManager = GetParentOrNull<CardManager>();
		
		if (cardManager != null)
		{
		cardManager.ConnectCardSignals(this);
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnGrabAreaEntered()
	{
		EmitSignal(SignalName.CardOnHoverEntered, this);
	}

	public void OnGrabAreaExited()
	{
		EmitSignal(SignalName.CardOnHoverExited, this);
	}
	
	public void SetCardData(CardData data)
{
	var portrait = GetNodeOrNull<Sprite2D>("FrontFace/Portrait");
	if (portrait != null) portrait.Texture = data.Portrait;

	var attackLabel = GetNodeOrNull<Label>("Stats/AttackLabel");
	var healthLabel = GetNodeOrNull<Label>("Stats/HealthLabel");
	var costLabel   = GetNodeOrNull<Label>("Stats/CostLabel");
	var nameLabel   = GetNodeOrNull<Label>("Stats/NameLabel");
	if (attackLabel != null) attackLabel.Text = data.Attack.ToString();
	if (healthLabel != null) healthLabel.Text = data.Health.ToString();
	if (costLabel   != null) costLabel.Text   = data.Cost.ToString();
	if (nameLabel   != null) nameLabel.Text   = data.Name.ToString();
}
}
