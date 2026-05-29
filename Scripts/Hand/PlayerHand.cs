using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHand : Node2D
{
    [Export] public Node2D CardManager { get; set; }
    
    const int cardWidth = 75; 
    const int handYPosition = 220; 
    int handSize = 5;
    
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
        
        cardsInHand.Insert(0, card); 
    }

    UpdateCardPositions();
}

    public override void _Process(double delta)
    {
    }

    public void AddCardToHand(Card card)
    {
		if(!cardsInHand.Contains(card)){
			cardsInHand.Insert(0, card);
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
    }

    private void AnimateCardToPosition(Card card, Vector2 newPosition)
    {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(card, "position", newPosition, 0.5);
    }

    private float CalculateCardPositions(int index)
	{
		var totalWidth = (cardsInHand.Count - 1) * cardWidth;
		
		var xOffset = screenCenterX - (totalWidth / 2f) + (index * cardWidth);

		return xOffset - (cardWidth / 2f); 
	}
}