using Godot;
using System;

public partial class CardSlot : Node2D
{
	[Export] public bool isPlayerSlot = true;
	public bool cardInside = false;

	public bool IsEmpty()
	{
		return !cardInside;
	}

	public void SpawnEnemy()
	{
		var cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");
		var cardData = GD.Load<CardData>("res://Cards/hladowski.tres"); // Default enemy
		if (cardScene != null && cardData != null)
		{
			var card = cardScene.Instantiate<Card>();
			GetTree().Root.GetNode("Game/CardManager").AddChild(card);
			card.SetCardData(cardData);
			card.GlobalPosition = this.GlobalPosition - card.PivotOffset;
			cardInside = true;
			card.isPlaced = true;
		}
	}
}
