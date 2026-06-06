using Godot;
using System;

public partial class CardManager : Node2D
{
	[Export] PlayerHand playerHand;
	[Export] InputManager inputManager;
	[Export] Label cardsLeftInDeckLabel;
	public bool IsDragging() => draggedCard != null;
	public Control GetDraggedCard() => draggedCard;
	Control draggedCard = null;
	Vector2 screenSize;
	System.Collections.Generic.List<Card> hoveredCards = new System.Collections.Generic.List<Card>();
	Card currentlyHighlightedCard = null;
	const uint cardLayerMask = 1;
	const uint slotLayerMask = 2;
	public override void _Ready()
	{
		screenSize = GetViewport().GetVisibleRect().Size;
		if (playerHand == null)
		{
			playerHand = GetParent().GetNodeOrNull<PlayerHand>("PlayerHand");
			if (playerHand == null)
				GD.PrintErr("CardManager: PlayerHand not found!");
		}

	}

    private void OnLeftMouseButtonReleased()
    {
    }

    private void OnLeftMouseButtonPressed()
    {
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
	
	public void DragStarted(Control card)
	{
		if (card is Card cardScript && cardScript.isPlaced) 
		{
			return;
		}

		draggedCard = card;
		card.Scale = new Vector2(1.0f, 1.0f);
		card.ZIndex = 2;
	}


	public void DragEnded(Control card)
	{
		HighlightCard((Card)card, false);
		UpdateHoveredHighlight();

		var cardSlotFound = RaycastCheckForCardSlot();
		if(cardSlotFound is CardSlot slot && !slot.cardInside && slot.isPlayerSlot)
		{
			playerHand.RemoveCardFromHand((Card)card);

			if (card is Card cardScript)
			{
				int cost = cardScript.Data != null ? cardScript.Data.Cost : 0;
				if (cost > 0)
				{
					var studentsInHand = new System.Collections.Generic.List<Card>();
					foreach (var c in playerHand.CardsInHand)
					{
						if (c != cardScript && c.Data != null && c.Data.Cost == 0)
						{
							studentsInHand.Add(c);
						}
					}

					if (studentsInHand.Count < cost)
					{
						cardScript.ZIndex = 1;
						playerHand?.AddCardToHand(cardScript);
						draggedCard = null;
						return;
					}
					else
					{
						for (int i = 0; i < cost; i++)
						{
							var studentToSpend = studentsInHand[i];
							playerHand.RemoveCardFromHand(studentToSpend);
							studentToSpend.QueueFree();
						}
					}
				}

				card.GlobalPosition = cardSlotFound.GlobalPosition - card.PivotOffset;
				slot.PlaceCard(cardScript);
				cardScript.isPlaced = true;

				if (hoveredCards.Contains(cardScript))
				{
					hoveredCards.Remove(cardScript);
				}
				if (currentlyHighlightedCard == cardScript)
				{
					currentlyHighlightedCard = null;
				}
			}
			else
			{
				card.GlobalPosition = cardSlotFound.GlobalPosition - card.PivotOffset;
				slot.cardInside = true;
			}

			var collisionShape = card.GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D");
			if (collisionShape != null)
    			collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		}
		else
		{
			if (card is Card cardScript)
			{
				cardScript.ZIndex = 1;
			}
			playerHand?.AddCardToHand((Card)card);
		}
		draggedCard = null;
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

	public Node2D RaycastCheckForCardSlot()
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D();
		
		parameters.Position = GetGlobalMousePosition();
		parameters.CollideWithAreas = true;
		parameters.CollisionMask = slotLayerMask;

		var result = spaceState.IntersectPoint(parameters);
		
		if(result.Count == 0) { return null; }
		
		result[0].TryGetValue("collider", out var collider);
		return ((Area2D)collider).GetParent<Node2D>();
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
        card.CardOnHoverEntered += OnCardHoverEntered;
        card.CardOnHoverExited += OnCardHoverExited;
    }

	private void OnCardHoverEntered(Card card)
	{
		if (card == null || card.isPlaced) return;
		if (GodotObject.IsInstanceValid(card) && !hoveredCards.Contains(card))
		{
			hoveredCards.Add(card);
			UpdateHoveredHighlight();
		}
	}

	private void OnCardHoverExited(Card card)
	{
		if (card == null || card.isPlaced) return;
		if (hoveredCards.Contains(card))
		{
			hoveredCards.Remove(card);
		}
		UpdateHoveredHighlight();
	}

	private void UpdateHoveredHighlight()
	{
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
