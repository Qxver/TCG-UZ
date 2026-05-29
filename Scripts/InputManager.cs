using Godot;
using Microsoft.VisualBasic;
using System;

public partial class InputManager : Node2D
{
	[Signal] public delegate void LeftMouseButtonPressedEventHandler();
	[Signal] public delegate void LeftMouseButtonReleasedEventHandler();

	[Export] Deck CharacterDeck;
	[Export] CardManager cardManager;
	const uint cardLayerMask = 1;
	const uint slotLayerMask = 2;
	const uint deckLayerMask = 4;

	public override void _Ready()
	{
		if (cardManager == null)
		{
			cardManager = GetParent().GetNodeOrNull<CardManager>("CardManager");
			if (cardManager == null)
				GD.PrintErr("InputManager: CardManager not found!");
		}
	}

	public override void _Input(InputEvent @event)
	{

		if(@event is InputEventMouseButton mouseEvent)
		{
			if(mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed == true)
			{
				EmitSignal(SignalName.LeftMouseButtonPressed);
				RaycastAtCursor();
			}
			else if(mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed == false)
			{
				EmitSignal(SignalName.LeftMouseButtonReleased);
			}
		}
	}

	public void RaycastAtCursor()
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D();
		
		parameters.Position = GetGlobalMousePosition();
		parameters.CollideWithAreas = true;

		var result = spaceState.IntersectPoint(parameters);
		
		if(result.Count > 0) 
		{
			result[0].TryGetValue("collider", out var resultCollision);
        
			uint mask = ((Area2D)resultCollision).CollisionMask;

			if(mask == cardLayerMask){
				Card card = ((Area2D)resultCollision).GetParent<Card>();
				cardManager.DragStarted(card);
			}
			else if(mask == slotLayerMask){
				CardSlot slot = ((Area2D)resultCollision).GetParent<CardSlot>();
			}
			else if(mask == deckLayerMask){
				CharacterDeck.DrawCard();
			}
			
		}
	}
}
