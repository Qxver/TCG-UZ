using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System;

public partial class CollectionMenu : Control
{
	[Export] public PackedScene Card;
	[Export] public GridContainer Grid;
	[Export] public Button DeckButton;
	[Export] public Label DeckSizeLabel;

	private static readonly string LockedTexturePath = "res://Sprites/Card/card_locked.png";
	private const string DECK_SAVE_PATH = "user://Saves/deck.save";
	private const int MAX_DECK_SIZE = 20;
	private const int DISPLAY_DECK_SIZE = 15;

	private bool deckMode = false;
	private Dictionary<string, int> deckCounts = new();

	public void _on_back_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	public override void _Ready()
	{
		LoadDeck();
		Refresh();
		UpdateDeckLabel();
	}

	public void _on_deck_button_pressed()
	{
		deckMode = !deckMode;
		DeckButton.Text = deckMode ? "✓ Deck" : "Deck";
		Refresh();
	}

	private void UpdateDeckLabel()
	{
		int total = 0;
		foreach (var (id, count) in deckCounts)
		{
			if (id == "0") continue;
			total += count;
		}
		DeckSizeLabel.Text = $"{total}/{DISPLAY_DECK_SIZE}";
	}

	private void OnCardClicked(CardData data)
	{
		if (!deckMode) return;

		var collection = CardCollection.Instance.GetAllCards();
		if (!collection.TryGetValue(data.Id, out int owned)) return;

		deckCounts.TryGetValue(data.Id, out int inDeck);

		int total = 0;
		foreach (var v in deckCounts.Values) total += v;

		if (inDeck >= 1 || total >= MAX_DECK_SIZE)
		{
			if (inDeck > 0)
			{
				deckCounts[data.Id] = inDeck - 1;
				if (deckCounts[data.Id] <= 0)
				{
					deckCounts.Remove(data.Id);
				}
				SaveDeck();
				UpdateDeckLabel();
				Refresh();
			}
			return;
		}

		deckCounts[data.Id] = inDeck + 1;
		SaveDeck();
		UpdateDeckLabel();
		Refresh();
	}

	public void Refresh()
	{
		foreach (Node child in Grid.GetChildren())
			child.QueueFree();

		if (CardDatabase.Instance == null) return;

		var allCards = new List<CardData>(CardDatabase.Instance.AllCards);
		allCards.Sort((a, b) =>
		{
			int rarityCompare = a.Rarity.CompareTo(b.Rarity);
			if (rarityCompare != 0) return rarityCompare;
			bool aNum = int.TryParse(a.Id, out int aId);
			bool bNum = int.TryParse(b.Id, out int bId);
			if (aNum && bNum) return aId.CompareTo(bId);
			return string.Compare(a.Id, b.Id, System.StringComparison.Ordinal);
		});

		var collection = CardCollection.Instance.GetAllCards();
		if (collection == null) return;

		var lockedTexture = ResourceLoader.Load<Texture2D>(LockedTexturePath);

		foreach (var data in allCards)
		{
			if (data.Id == "0") continue;
			bool isUnlocked = collection.TryGetValue(data.Id, out int amount) && amount > 0;
			deckCounts.TryGetValue(data.Id, out int inDeck);
			bool isInDeck = inDeck > 0;

			var card = Card.Instantiate<Card>();
			Grid.AddChild(card);
			card.SetCardData(data);

			var portrait     = card.GetNodeOrNull<Sprite2D>("FrontFace/Portrait");
			var stats        = card.GetNodeOrNull<Control>("Stats");
			var innerColor   = card.GetNodeOrNull<CanvasItem>("FrontFace/InnerColor");
			var portraitItem = card.GetNodeOrNull<CanvasItem>("FrontFace/Portrait");
			var frontFace    = card.GetNodeOrNull<CanvasItem>("FrontFace");

			if (portrait != null)
			{
				portrait.Texture = isUnlocked ? data.Portrait : lockedTexture;
				stats.Visible = isUnlocked;
			}

			if (!isUnlocked)
			{
				if (innerColor   != null) innerColor.Modulate   = new Color(0.4f, 0.4f, 0.4f);
				if (portraitItem != null) portraitItem.Modulate  = new Color(0.4f, 0.4f, 0.4f);
			}
			else if (deckMode && isInDeck)
			{
				// Podświetlenie karty w talii
				if (frontFace != null) frontFace.Modulate = new Color(0.6f, 1.0f, 0.6f);
			}

			var amountLabel = card.GetNodeOrNull<Label>("AmountLabel");
			if (amountLabel != null)
			{
				if (deckMode && isUnlocked)
					amountLabel.Text = $"{inDeck}/{amount}";
				else
					amountLabel.Text = isUnlocked ? $"x{amount}" : "";
			}

			if (deckMode && isUnlocked)
			{
				var cardData = data;
				card.GuiInput += (inputEvent) =>
				{
					if (inputEvent is InputEventMouseButton mb &&
						mb.ButtonIndex == MouseButton.Left && mb.Pressed)
					{
						OnCardClicked(cardData);
					}
				};
			}
		}
	}

	private void SaveDeck()
	{
		string json = JsonSerializer.Serialize(deckCounts);
		using var file = FileAccess.Open(DECK_SAVE_PATH, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}

	private void LoadDeck()
	{
		if (!FileAccess.FileExists(DECK_SAVE_PATH))
		{
			deckCounts["0"] = 5;
			SaveDeck();
			return;
		} 
		using var file = FileAccess.Open(DECK_SAVE_PATH, FileAccess.ModeFlags.Read);
		string json = file.GetAsText().Trim();
		if (string.IsNullOrEmpty(json)) return;
		deckCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new();
	}
}
