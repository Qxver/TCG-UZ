using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class CardCollection : Node
{
	public static CardCollection Instance;

	private Dictionary<string, int> cards = new();

	private const string SAVE_PATH = "user://Saves/collection.save";

	public override void _Ready()
	{
		Instance = this;

		LoadCollection();	
	}

	public void AddCard(CardData card)
	{
		if (!cards.ContainsKey(card.Id))
			cards[card.Id] = 0;

		cards[card.Id]++;

		SaveCollection();
	}

	public int GetAmount(string cardId)
	{
		return cards.GetValueOrDefault(cardId, 0);
	}

	public Dictionary<string, int> GetAllCards()
	{
		return cards;
	}

	public void SaveCollection()
	{
		string json = JsonSerializer.Serialize(cards);

		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);

		file.StoreString(json);

		GD.Print("Collection saved.");
	}

	public void LoadCollection()
	{
		if (!FileAccess.FileExists(SAVE_PATH))
		{
			GD.Print("No save file found.");
			return;
		}

		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);

		string json = file.GetAsText();

		cards = JsonSerializer.Deserialize<Dictionary<string, int>>(json);

	}
	
}
