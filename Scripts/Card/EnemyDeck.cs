using Godot;
using System.Collections.Generic;

public partial class EnemyDeck : Node2D
{
    [Export] public EnemyHand EnemyHand;
    [Export] public Node2D CardManager;
    [Export] public Godot.Collections.Array<CardData> StartingCards = new();

    private List<CardData> drawPile = new();
    private PackedScene cardScene = GD.Load<PackedScene>("res://Scenes/EnemyCard.tscn");

    public int RemainingCards => drawPile.Count;

    public override void _Ready()
    {
        foreach (var card in StartingCards)
            drawPile.Add(card);

        Shuffle();
        GD.Print($"EnemyDeck ready with {drawPile.Count} cards.");
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

    public bool DrawCard()
    {
        if (drawPile.Count == 0)
        {
            GD.Print("EnemyDeck: Empty!");
            return false;
        }

        if (EnemyHand == null || CardManager == null)
        {
            GD.PrintErr("EnemyDeck: EnemyHand or CardManager not assigned!");
            return false;
        }

        var cardData = drawPile[0];
        drawPile.RemoveAt(0);

        var card = cardScene.Instantiate<EnemyCard>();
        CardManager.AddChild(card);
        card.SetCardData(cardData);
        card.GlobalPosition = this.GlobalPosition;
        card.SetFaceDown(true);   // always face-down until played
        card.ZIndex = 1;

        EnemyHand.AddCardToHand(card);
        GD.Print($"EnemyDeck: Drew '{cardData.Name}', {drawPile.Count} remaining.");
        return true;
    }
}