using Godot;
using System.Collections.Generic;

public partial class Deck : Node2D
{
    [Export] public PlayerHand PlayerHand;
    [Export] public Node2D CardManager;

    [Export] public Godot.Collections.Array<CardData> StartingCards = new();

    [Export] public bool IsInfinite = false;
    [Export] public bool ShowRearFace = true;

    private List<CardData> drawPile = new();
    private PackedScene cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");

    public int RemainingCards => drawPile.Count;

    public override void _Ready()
    {
        // Copy exported array into the draw pile and shuffle
        foreach (var card in StartingCards)
            drawPile.Add(card);

        Shuffle();
        GD.Print($"{Name}: Deck ready with {drawPile.Count} cards.");

        var rearFace = GetNodeOrNull<Control>("RearFace");
        if (rearFace != null)
        {
            rearFace.Visible = ShowRearFace;
        }
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
        {
            drawPile.Add(cardData);
        }

        var card = cardScene.Instantiate<Card>();
        CardManager.AddChild(card);
        card.Initialize(cardData);
        
        card.GlobalPosition = this.GlobalPosition;
        card.SetFaceDown(true);
        card.hasDrawnAnimPlayed = false;
        card.ZIndex = 1;

        PlayerHand.AddCardToHand(card);

        if (IsInfinite)
            GD.Print($"{Name}: Drew '{cardData.Name}', Infinite remaining.");
        else
            GD.Print($"{Name}: Drew '{cardData.Name}', {drawPile.Count} remaining.");
            
        return true;
    }
}