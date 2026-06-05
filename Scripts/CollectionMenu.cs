using Godot;
using System.Collections.Generic;

public partial class CollectionMenu : Control
{
	[Export] public PackedScene Card;
	[Export] public GridContainer Grid;

	private static readonly string LockedTexturePath = "res://Sprites/Card/card_locked.png";

	public void _on_back_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	public override void _Ready()
	{
		Refresh();
	}

	public void Refresh()
{
	foreach (Node child in Grid.GetChildren())
		child.QueueFree();

	if (CardDatabase.Instance == null) {
		GD.Print("CardDatabase instance is null.");
		return;
	}

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
	if (collection == null) {
		GD.Print("Collection is null.");
		return;
	}

	var lockedTexture = ResourceLoader.Load<Texture2D>(LockedTexturePath);

	foreach (var data in allCards)
	{
		bool isUnlocked = collection.TryGetValue(data.Id, out int amount) && amount > 0;

		var card = Card.Instantiate<Card>();
		Grid.AddChild(card);
		card.SetCardData(data);

		var portrait    = card.GetNodeOrNull<Sprite2D>("FrontFace/Portrait");
		var stats       = card.GetNodeOrNull<Control>("Stats");
		var innerColor  = card.GetNodeOrNull<CanvasItem>("FrontFace/InnerColor");
		var portraitItem = card.GetNodeOrNull<CanvasItem>("FrontFace/Portrait");
		var outerSpacer = card.GetNodeOrNull<Sprite2D>("OuterSpacer");

		if (portrait != null)
		{
			portrait.Texture = isUnlocked ? data.Portrait : lockedTexture;
			stats.Visible = isUnlocked;
		}
		
		if (!isUnlocked)
		{
			if (innerColor   != null) innerColor.Modulate  = new Color(0.4f, 0.4f, 0.4f);
			if (portraitItem != null) portraitItem.Modulate = new Color(0.4f, 0.4f, 0.4f);
		}

		var amountLabel = card.GetNodeOrNull<Label>("AmountLabel");
		if (amountLabel != null)
			amountLabel.Text = isUnlocked ? $"x{amount}" : "";
	}
}
}
