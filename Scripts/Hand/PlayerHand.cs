using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHand : Node2D
{
    [Export] public Node2D CardManager { get; set; }
    
    const int cardWidth = 75; 
    const int handYPosition = 260; 
    int handSize = 15;
    
    float screenCenterX; 
    
    PackedScene cardScene = GD.Load<PackedScene>("res://Scenes/Card.tscn");
    List<Card> cardsInHand = new List<Card>();

    public override void _Ready()
{
    screenCenterX = GetViewportRect().Size.X / 2;

    for (int i = 0; i < handSize; i++)
    {
        var card = cardScene.Instantiate<Card>();
        card.Name = $"Card_{i}";
        CardManager.AddChild(card);
        
        cardsInHand.Add(card); 
    }

    UpdateCardPositions();
}

    public override void _Process(double delta)
    {
    }

    public void AddCardToHand(Card card)
    {
		if(!cardsInHand.Contains(card)){
			cardsInHand.Add(card);
			UpdateCardPositions();
		}
		else{
			AnimateCardToPosition(card, card.positionInHand);
		}
        
    }

	public void RemoveCardFromHand(Card card)
	{
		if (cardsInHand.Contains(card))
		{
			cardsInHand.Remove(card);
			UpdateCardPositions();
		}
	}

    private void UpdateCardPositions()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            var newPosition = new Vector2(CalculateCardPositions(i), handYPosition);
            var card = cardsInHand[i];
            card.positionInHand = newPosition;
            AnimateCardToPosition(card, newPosition);
        }
        RearrangeNodesInManager();
    }

    private void AnimateCardToPosition(Card card, Vector2 newPosition)
    {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", newPosition, 0.5);
    }

    private float CalculateCardPositions(int index)
    {
        if (cardsInHand.Count == 1)
        {
            return screenCenterX - (cardWidth / 2f);
        }

        float maxHandWidth = 500f; 

        float idealWidth = (cardsInHand.Count - 1) * cardWidth;

        float actualCardSpacing = cardWidth;
        if (idealWidth > maxHandWidth)
        {
            actualCardSpacing = maxHandWidth / (cardsInHand.Count - 1);
        }

        float finalHandWidth = (cardsInHand.Count - 1) * actualCardSpacing;

        float xOffset = screenCenterX - (finalHandWidth / 2f) + (index * actualCardSpacing) - (cardWidth / 2f);

        return xOffset;
    }

    public void RearrangeNodesInManager()
    {
        if (CardManager == null) return;

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            CardManager.MoveChild(cardsInHand[i], i);
        }
    }
}