using Godot;

public enum TurnState { PlayerTurn, EnemyTurn }

public partial class BattleManager : Node
{
    [Export] public InputManager InputManager;
    [Export] public EnemyHand EnemyHand;
    [Export] public EnemyDeck EnemyDeck;
    [Export] public Node2D EnemySlots;
    [Export] public Timer BattleTimer;
    [Export] public Button EndTurnButton;

    public TurnState CurrentTurn { get; private set; } = TurnState.PlayerTurn;

    private const float EnemyThinkTime = 1.2f; // seconds before enemy acts

    public override void _Ready()
    {
        BattleTimer.Timeout += OnEnemyThinkFinished;
		// In BattleManager._Ready, before StartPlayerTurn():
		for (int i = 0; i < 4; i++)
			InputManager.CharacterDeck?.DrawCard();
        StartPlayerTurn();
    }

    // ── Player turn ──────────────────────────────────────────────

    private void StartPlayerTurn()
    {
        CurrentTurn = TurnState.PlayerTurn;
        GD.Print("=== PLAYER TURN ===");

        InputManager.OnStartTurn();   // draw card, reset hasDrawnThisTurn
        EndTurnButton.Disabled = false;
    }

    public void OnEndTurn()  // hooked to the button in Game.tscn
    {
        if (CurrentTurn != TurnState.PlayerTurn) return;

        EndTurnButton.Disabled = true;
        InputManager.OnEndTurn();
        StartEnemyTurn();
    }

    // ── Enemy turn ───────────────────────────────────────────────

    private void StartEnemyTurn()
    {
        CurrentTurn = TurnState.EnemyTurn;
        GD.Print("=== ENEMY TURN ===");

        EnemyDeck.DrawCard();

        // Short delay so it doesn't feel instant
        BattleTimer.WaitTime = EnemyThinkTime;
        BattleTimer.OneShot = true;
        BattleTimer.Start();
    }

    private void OnEnemyThinkFinished()
    {
        PlayEnemyCard();
        // You can chain another timer here for multi-card turns later
        StartPlayerTurn();
    }

    private void PlayEnemyCard()
    {
        if (EnemyHand.CardsInHand.Count == 0) return;

        // Find a free enemy slot
        CardSlot freeSlot = null;
        foreach (Node child in EnemySlots.GetChildren())
        {
            if (child is CardSlot slot && slot.IsEmpty() && !slot.isPlayerSlot)
            {
                freeSlot = slot;
                break;
            }
        }

        if (freeSlot == null)
        {
            GD.Print("Enemy: no free slots.");
            return;
        }

        var card = EnemyHand.TakeCard(0); // take first card
        card.SetFaceDown(false);           // reveal when played
        card.GlobalPosition = freeSlot.GlobalPosition - card.PivotOffset;
        card.isPlaced = true;
        freeSlot.cardInside = true;

        GD.Print($"Enemy played: {card.Data?.Name}");
    }
}