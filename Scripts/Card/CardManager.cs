using Godot;
using System;

public partial class CardManager : Node2D
{
	Control draggedCard = null;
	Vector2 screenSize;
	System.Collections.Generic.List<Card> hoveredCards = new System.Collections.Generic.List<Card>();
	Card currentlyHighlightedCard = null;
	uint cardLayerMask = 1;
	public override void _Ready()
	{
		screenSize = GetViewport().GetVisibleRect().Size;
	}

	public override void _Process(double delta)
	{
		if(draggedCard != null)
		{
			var mousePos = GetGlobalMousePosition();
			Vector2 targetPos = new Vector2(
  				Mathf.Clamp(mousePos.X, 0, screenSize.X), 
   				Mathf.Clamp(mousePos.Y, 0, screenSize.Y)
			);
			Vector2 adjustedTarget = targetPos - draggedCard.PivotOffset;
        	draggedCard.GlobalPosition = draggedCard.GlobalPosition.Lerp(adjustedTarget, 0.5f);
		}
	}

	public override void _Input(InputEvent @event)
	{

		if(@event is InputEventMouseButton mouseEvent)
		{
			if(mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed == true)
			{
				var card = RaycastCheckForCard();
				if(card != null)
				{
					DragStarted(card);
				}
				
			}
			else if(mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed == false)
			{
				if (draggedCard != null)
    			{
        			DragEnded(draggedCard);
    			}
			}
		}


		if (@event.IsActionPressed("ui_accept"))
		{
			GD.Print("Accept button pressed");
		}
	}
	
	public void DragStarted(Control card)
	{
		draggedCard = card;
		card.Scale = new Vector2(1.0f, 1.0f);
		card.ZIndex = 2;
		card.GetParent().MoveChild(card, -1); // To do zmiany w momencie dodania ręki z kartami
	}


	public void DragEnded(Control card)
	{
		draggedCard = null;
		HighlightCard((Card)card, false);
		UpdateHoveredHighlight();
	}


	public Control RaycastCheckForCard()
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D();
		
		parameters.Position = GetGlobalMousePosition();
		parameters.CollideWithAreas = true;
		parameters.CollisionMask = cardLayerMask;

		var result = spaceState.IntersectPoint(parameters);
		
		if(result.Count == 0) { return null; }
		
		return HighestZIndexCard(result);
	}

	private Card HighestZIndexCard(Godot.Collections.Array<Godot.Collections.Dictionary> cards)
	{
		_ = cards[0].TryGetValue("collider", out var highestCardCollider);
		var highestCard = ((Area2D)highestCardCollider).GetParent<Card>();

		for (int i = 1; i < cards.Count; i++)
		{
			_ = cards[i].TryGetValue("collider", out var currentCollider);
			var currentCard = ((Area2D)currentCollider).GetParent<Card>();
			
			if (currentCard.ZIndex > highestCard.ZIndex)
			{
				highestCard = currentCard;
			}
			else if (currentCard.ZIndex == highestCard.ZIndex)
			{
				if (IsDrawnOnTop(highestCard, currentCard))
				{
					highestCard = currentCard;
				}
			}
		}
		return highestCard;			
	}

	private bool IsDrawnOnTop(Node nodeA, Node nodeB)
	{
		if (nodeA == nodeB) return false;

		var pathA = new System.Collections.Generic.List<Node>();
		var currA = nodeA;
		while (currA != null)
		{
			pathA.Add(currA);
			currA = currA.GetParent();
		}
		pathA.Reverse();

		var pathB = new System.Collections.Generic.List<Node>();
		var currB = nodeB;
		while (currB != null)
		{
			pathB.Add(currB);
			currB = currB.GetParent();
		}
		pathB.Reverse();

		int minLength = Math.Min(pathA.Count, pathB.Count);
		int lcaIndex = -1;
		for (int i = 0; i < minLength; i++)
		{
			if (pathA[i] == pathB[i])
			{
				lcaIndex = i;
			}
			else
			{
				break;
			}
		}

		if (lcaIndex == -1)
		{
			return false;
		}

		if (lcaIndex == pathA.Count - 1)
		{
			return true; 
		}
		if (lcaIndex == pathB.Count - 1)
		{
			return false;
		}

		var siblingA = pathA[lcaIndex + 1];
		var siblingB = pathB[lcaIndex + 1];
		return siblingB.GetIndex() > siblingA.GetIndex();
	}

	public void ConnectCardSignals(Card card)
    {
        // TAK podłącza się sygnały w Godot 4 C#
        card.CardOnHoverEntered += OnCardHoverEntered;
        card.CardOnHoverExited += OnCardHoverExited;
    }

	private void OnCardHoverEntered(Card card)
	{
		if (GodotObject.IsInstanceValid(card) && !hoveredCards.Contains(card))
		{
			hoveredCards.Add(card);
			UpdateHoveredHighlight();
		}
	}

	private void OnCardHoverExited(Card card)
	{
		if (hoveredCards.Contains(card))
		{
			hoveredCards.Remove(card);
		}
		UpdateHoveredHighlight();
	}

	private void UpdateHoveredHighlight()
	{
		// Clean up invalid card references
		hoveredCards.RemoveAll(c => !GodotObject.IsInstanceValid(c));

		if (draggedCard != null)
		{
			if (currentlyHighlightedCard != null)
			{
				HighlightCard(currentlyHighlightedCard, false);
				currentlyHighlightedCard = null;
			}
			return;
		}

		Card topCard = null;
		foreach (var card in hoveredCards)
		{
			if (topCard == null)
			{
				topCard = card;
			}
			else
			{
				if (card.ZIndex > topCard.ZIndex)
				{
					topCard = card;
				}
				else if (card.ZIndex == topCard.ZIndex)
				{
					if (IsDrawnOnTop(topCard, card))
					{
						topCard = card;
					}
				}
			}
		}

		if (topCard != currentlyHighlightedCard)
		{
			if (currentlyHighlightedCard != null)
			{
				HighlightCard(currentlyHighlightedCard, false);
			}
			currentlyHighlightedCard = topCard;
			if (currentlyHighlightedCard != null)
			{
				HighlightCard(currentlyHighlightedCard, true);
			}
		}
	}

	private void HighlightCard(Card card, bool highlight)
	{
		if (highlight)
		{
			card.Scale = new Vector2(1.05f, 1.05f);
			card.ZIndex = 2;
		}
		else
		{
			card.Scale = new Vector2(1f, 1f);
			card.ZIndex = 1;
		}
	}
}
