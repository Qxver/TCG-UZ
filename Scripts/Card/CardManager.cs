using Godot;
using System;

public partial class CardManager : Node2D
{
	Control draggedCard = null;
	Vector2 screenSize;
	uint cardLayerMask = 1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		screenSize = GetViewport().GetVisibleRect().Size;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
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
					draggedCard = card;
				}
				
			}
			else if(mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed == false)
			{
				draggedCard = null;
			}
		}


		if (@event.IsActionPressed("ui_accept"))
		{
			GD.Print("Accept button pressed");
		}
	}

	public Control RaycastCheckForCard(){
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D();
		
		parameters.Position = GetGlobalMousePosition();
		parameters.CollideWithAreas = true;
		parameters.CollisionMask = cardLayerMask;

		var result = spaceState.IntersectPoint(parameters);
		
		if(result.Count == 0) { return null; }
		
		result[0].TryGetValue("collider", out var collider);
		
		Area2D area = (Area2D)collider;
		return area.GetParent<Control>();
	}
}
