using Godot;
using System.Collections.Generic;
using System.Text.Json;

public partial class DeckLoader : Node
{
    public static DeckLoader Instance;

    private const string DECK_SAVE_PATH = "user://Saves/deck.save";

    // Filled on Ready, consumed by Deck.cs
    public List<CardData> LoadedDeck { get; private set; } = new();

    public override void _Ready()
    {
        Instance = this;
        LoadedDeck = BuildDeck();
    }

    private List<CardData> BuildDeck()
    {
        var result = new List<CardData>();

        if (!FileAccess.FileExists(DECK_SAVE_PATH))
        {
            GD.Print("DeckLoader: No deck save found, using empty deck.");
            return result;
        }

        using var file = FileAccess.Open(DECK_SAVE_PATH, FileAccess.ModeFlags.Read);
        string json = file.GetAsText().Trim();
        if (string.IsNullOrEmpty(json)) return result;

        var deckCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
        if (deckCounts == null) return result;

        // CardDatabase must be loaded before this — it's also an autoload so it is
        foreach (var (id, count) in deckCounts)
        {
            if (id == "0") continue; // skip students, they're added separately

            var cardData = CardDatabase.Instance.AllCards.Find(c => c.Id == id);
            if (cardData == null)
            {
                GD.PrintErr($"DeckLoader: Card ID '{id}' not found in database.");
                continue;
            }

            for (int i = 0; i < count; i++)
                result.Add(cardData);
        }

        GD.Print($"DeckLoader: Loaded {result.Count} cards into player deck.");
        return result;
    }
}