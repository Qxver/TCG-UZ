using Godot;
using System.Collections.Generic;

public partial class Deck : Node2D
{
	[Export] public PlayerHand PlayerHand;
	[Export] public Node2D CardManager;

	[Export] public Godot.Collections.Array<CardData> StartingCards = new();
	[Export] public bool UsePlayerSavedDeck = false;
	[Export] public bool IsInfinite = false;
	[Export] public bool ShowRearFace = true;

	private List<CardData> drawPile = new();
	private PackedScene cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

	public int RemainingCards => drawPile.Count;

	public override void _Ready()
	{
		if (UsePlayerSavedDeck && DeckLoader.Instance?.LoadedDeck.Count > 0)
		{
			foreach (var card in DeckLoader.Instance.LoadedDeck)
				drawPile.Add(card);
		}
		else
		{
			foreach (var card in StartingCards)
				drawPile.Add(card);
		}

		Shuffle();

		var rearFace = GetNodeOrNull<Control>("RearFace");
		if (rearFace != null)
			rearFace.Visible = ShowRearFace;

		UpdateDeckVisuals();
	}

	public bool DrawCard()
	{
		if (drawPile.Count == 0)
		{
			GD.Print($"{Name}: Deck is empty!");
			return false;
		}

		if (PlayerHand == null || CardManager == null)
		{
			GD.PrintErr($"{Name}: PlayerHand or CardManager not assigned!");
			return false;
		}

		var cardData = drawPile[0];
		drawPile.RemoveAt(0);

		if (IsInfinite)
			drawPile.Add(cardData);

		var card = cardScene.Instantiate<Card>();
		CardManager.AddChild(card);
		card.SetCardData(cardData);
		card.InitCombatStats();
		card.GlobalPosition = this.GlobalPosition;
		card.SetFaceDown(true);
		card.hasDrawnAnimPlayed = false;
		card.ZIndex = 1;

		PlayerHand.AddCardToHand(card);
		UpdateDeckVisuals();

		return true;
	}

	private void UpdateDeckVisuals()
	{
		var label = GetNodeOrNull<Label>("CardsLeft");
		if (label != null)
			label.Text = drawPile.Count.ToString();

		var frontFace = GetNodeOrNull<Control>("FrontFace");
		var rearFace  = GetNodeOrNull<Control>("RearFace");
		bool isEmpty  = drawPile.Count == 0 && !IsInfinite;

		if (frontFace != null) frontFace.Visible = !isEmpty;
		if (rearFace  != null) rearFace.Visible  = !isEmpty && ShowRearFace;
		if (label     != null) label.Visible     = !isEmpty;
	}

	public void Shuffle()
	{
		var rng = new System.Random();
		for (int i = drawPile.Count - 1; i > 0; i--)
		{
			int j = rng.Next(i + 1);
			(drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
		}
	}
}
