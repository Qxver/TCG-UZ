using Godot;

public partial class InputManager : Node2D
{
    [Export] public Deck CharacterDeck;
    [Export] public Deck StudentDeck;
    [Export] public CardManager cardManager;

    private bool hasDrawnThisTurn = false;

    const uint cardLayerMask = 1;
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
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                RaycastAtCursor();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
            {
                if (cardManager != null && cardManager.IsDragging())
                    cardManager.DragEnded(cardManager.GetDraggedCard());
            }
        }
    }

    public void RaycastAtCursor()
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Position = GetGlobalMousePosition(),
            CollideWithAreas = true
        };

        var result = spaceState.IntersectPoint(parameters);
        if (result.Count == 0) return;

        result[0].TryGetValue("collider", out var resultCollision);
        var area = (Area2D)resultCollision;
        uint mask = area.CollisionMask;

        if (mask == cardLayerMask)
        {
            cardManager?.DragStarted(area.GetParent<Card>());
        }
        else if (mask == deckLayerMask)
        {
            TryDrawFromDeck(area.GetParent<Deck>());
        }
    }

    private void TryDrawFromDeck(Deck deck)
    {
        if (hasDrawnThisTurn)
        {
            GD.Print("Already drew a card this turn!");
            return;
        }
        if (deck.DrawCard())
            hasDrawnThisTurn = true;
    }

    // Wire this to your End Turn button
    public void OnEndTurn()
    {
        hasDrawnThisTurn = false;
        GD.Print("Turn ended.");
    }
}