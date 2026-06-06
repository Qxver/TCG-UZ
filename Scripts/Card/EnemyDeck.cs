using Godot;
using System.Collections.Generic;

public partial class EnemyDeck : Node2D
{
    [Export] public EnemyHand EnemyHand;
    [Export] public Node2D CardManager;
    [Export] public Godot.Collections.Array<CardData> StartingCards = new();

    [Export] public CardData StudentCardData;

    private List<CardData> characterPile = new();
    private List<CardData> studentPile = new();  

    private PackedScene cardScene = GD.Load<PackedScene>("res://Scenes/EnemyCard.tscn");

    public int RemainingCards => characterPile.Count;

    [Export] public Vector2 SpawnOffset = Vector2.Zero;

    public override void _Ready()
    {
        foreach (var card in StartingCards)
            characterPile.Add(card);

        Shuffle();
        GD.Print($"EnemyDeck ready with {characterPile.Count} cards.");
    }

    private void Shuffle()
    {
        var rng = new System.Random();
        for (int i = characterPile.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (characterPile[i], characterPile[j]) = (characterPile[j], characterPile[i]);
        }
    }

    public bool DrawCharacterCard()
    {
        if (characterPile.Count == 0)
        {
            GD.Print("EnemyDeck: No character cards left!");
            return false;
        }

        var cardData = characterPile[0];
        characterPile.RemoveAt(0);
        SpawnCard(cardData, this.GlobalPosition + SpawnOffset);
        GD.Print($"EnemyDeck: Drew character '{cardData.Name}', {characterPile.Count} remaining.");
        return true;
    }
    public bool DrawStudentCard(Godot.Variant startPos = default)
    {
        if (StudentCardData == null)
        {
            GD.PrintErr("EnemyDeck: StudentCardData not assigned!");
            return false;
        }

        Vector2 spawnPos = this.GlobalPosition + SpawnOffset;
        if (startPos.VariantType == Godot.Variant.Type.Vector2)
        {
            spawnPos = startPos.AsVector2();
        }

        SpawnCard(StudentCardData, spawnPos);
        GD.Print("EnemyDeck: Drew a student.");
        return true;
    }

    private void SpawnCard(CardData cardData, Vector2 startPos)
    {
        var card = cardScene.Instantiate<EnemyCard>();
        CardManager.AddChild(card);
        card.SetCardData(cardData);
        card.InitCombatStats();
        card.GlobalPosition = startPos;
        card.SetFaceDown(true);
        card.hasDrawnAnimPlayed = false;
        card.ZIndex = 1;
        EnemyHand.AddCardToHand(card);
    }
}