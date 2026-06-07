using Godot;
using System.Collections.Generic;


public enum TurnState { PlayerTurn, EnemyTurn }

public partial class BattleManager : Node
{
	[Export] public InputManager InputManager;
	[Export] public EnemyHand EnemyHand;
	[Export] public EnemyDeck EnemyDeck;
	[Export] public Node2D EnemySlots;
	[Export] public Node2D PlayerSlots;
	[Export] public Timer BattleTimer;
	[Export] public Button EndTurnButton;
	[Export] public Label PlayerHealthLabel;
	[Export] public Label EnemyHealthLabel;

	[Export] public int StartingCharacterCards = 5;
	[Export] public int StartingStudentCards = 0;
	[Export] public int PlayerMaxHealth = 20;
	[Export] public int EnemyMaxHealth = 20;
	
	[Export] public CanvasLayer GameOverLayer;
	[Export] public Label ResultLabel;

	private int playerHealth;
	private int enemyHealth;

	public TurnState CurrentTurn { get; private set; } = TurnState.PlayerTurn;

	private const float EnemyThinkTime = 1.5f;

	public override void _Ready()
	{
		playerHealth = PlayerMaxHealth;
		enemyHealth = EnemyMaxHealth;
		UpdateHealthLabels();

		BattleTimer.Timeout += OnEnemyThinkFinished;

		for (int i = 0; i < StartingCharacterCards; i++)
			EnemyDeck.DrawCharacterCard();
		for (int i = 0; i < StartingStudentCards; i++)
			EnemyDeck.DrawStudentCard();

		for (int i = 0; i < StartingCharacterCards; i++)
			InputManager.CharacterDeck.DrawCard();
		for (int i = 0; i < StartingStudentCards; i++)
			InputManager.StudentDeck.DrawCard();

		StartPlayerTurn();
	}

	// ── Player turn ──────────────────────────────────────────────

	private void StartPlayerTurn()
	{
		if (playerHealth <= 0 || enemyHealth <= 0) return;
		CurrentTurn = TurnState.PlayerTurn;
		GD.Print("=== PLAYER TURN ===");
		InputManager.OnStartTurn();
		EndTurnButton.Disabled = false;
	}

	public void OnEndTurn()
	{
		if (CurrentTurn != TurnState.PlayerTurn) return;
		EndTurnButton.Disabled = true;
		InputManager.OnEndTurn();
		StartEnemyTurn();
	}

	// ── Enemy turn ───────────────────────────────────────────────

	private void StartEnemyTurn()
	{
		if (playerHealth <= 0 || enemyHealth <= 0) return;
		CurrentTurn = TurnState.EnemyTurn;
		GD.Print("=== ENEMY TURN ===");

		bool needsStudent = EnemyNeedsStudent();
		if (needsStudent || EnemyDeck.RemainingCards == 0)
			EnemyDeck.DrawStudentCard();
		else
			EnemyDeck.DrawCharacterCard();

		BattleTimer.WaitTime = EnemyThinkTime;
		BattleTimer.OneShot = true;
		BattleTimer.Start();
	}

	private async void OnEnemyThinkFinished()
	{
		PlayEnemyCard();
		await ResolveCombat();
		CheckWinCondition();
		StartPlayerTurn();
	}

	// ── Combat ───────────────────────────────────────────────────

	private async System.Threading.Tasks.Task ResolveCombat()
	{
		var playerSlotList = GetSlotsInOrder(PlayerSlots);
		var enemySlotList  = GetSlotsInOrder(EnemySlots);

		int count = Mathf.Min(playerSlotList.Count, enemySlotList.Count);

		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

		for (int i = 0; i < count; i++)
		{
			var playerSlot = playerSlotList[i];
			var enemySlot  = enemySlotList[i];

			bool hasPlayer = !playerSlot.IsEmpty() && playerSlot.OccupyingCard != null;
			bool hasEnemy  = !enemySlot.IsEmpty()  && enemySlot.OccupyingCard  != null;

			if (hasPlayer || hasEnemy)
			{
				if (hasPlayer) 
				{
					var tween = GetTree().CreateTween();
					tween.TweenProperty(playerSlot.OccupyingCard, "position", playerSlot.OccupyingCard.Position + new Vector2(0, -30), 0.15f);
					tween.TweenProperty(playerSlot.OccupyingCard, "position", playerSlot.OccupyingCard.Position, 0.15f);
				}
				if (hasEnemy) 
				{
					var tween = GetTree().CreateTween();
					tween.TweenProperty(enemySlot.OccupyingCard, "position", enemySlot.OccupyingCard.Position + new Vector2(0, 30), 0.15f);
					tween.TweenProperty(enemySlot.OccupyingCard, "position", enemySlot.OccupyingCard.Position, 0.15f);
				}

				await ToSignal(GetTree().CreateTimer(0.15f), "timeout");

				if (hasPlayer && hasEnemy)
				{
					var playerCard = playerSlot.OccupyingCard;
					var enemyCard  = enemySlot.OccupyingCard;

					bool enemyDies  = enemyCard.TakeDamage(playerCard.Data.Attack);
					bool playerDies = playerCard.TakeDamage(enemyCard.Data.Attack);

					GD.Print($"Combat: {playerCard.Data.Name} ({playerCard.Data.Attack} atk) vs {enemyCard.Data.Name} ({enemyCard.Data.Attack} atk)");

					if (playerDies) KillCard(playerSlot);
					if (enemyDies)  KillCard(enemySlot);
				}
				else if (hasPlayer && !hasEnemy)
				{
					int dmg = playerSlot.OccupyingCard.Data.Attack;
					enemyHealth -= dmg;
					GD.Print($"{playerSlot.OccupyingCard.Data.Name} hits enemy for {dmg}. Enemy HP: {enemyHealth}");
				}
				else if (!hasPlayer && hasEnemy)
				{
					int dmg = enemySlot.OccupyingCard.Data.Attack;
					playerHealth -= dmg;
					GD.Print($"{enemySlot.OccupyingCard.Data.Name} hits player for {dmg}. Player HP: {playerHealth}");
				}

				UpdateHealthLabels();
				
				await ToSignal(GetTree().CreateTimer(0.35f), "timeout");
			}
		}
		
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
	}

	private void KillCard(CardSlot slot)
	{
		if (slot.OccupyingCard == null) return;
		GD.Print($"{slot.OccupyingCard.Data.Name} dies.");
		
		var tween = GetTree().CreateTween();
		tween.TweenProperty(slot.OccupyingCard, "modulate:a", 0.0f, 0.2f);
		tween.TweenCallback(Callable.From(() => {
			if (GodotObject.IsInstanceValid(slot.OccupyingCard)) {
				slot.OccupyingCard.QueueFree();
			}
		}));
		
		slot.ClearSlot();
	}

	private List<CardSlot> GetSlotsInOrder(Node2D slotsParent)
	{
		var list = new List<CardSlot>();
		foreach (Node child in slotsParent.GetChildren())
		{
			if (child is CardSlot slot)
				list.Add(slot);
		}
		list.Sort((a, b) => a.GlobalPosition.X.CompareTo(b.GlobalPosition.X));
		return list;
	}

	// ── Win condition ────────────────────────────────────────────

	private void CheckWinCondition()
	{
		if (playerHealth <= 0 && enemyHealth <= 0)
		{
			ShowGameOver("Draw!");
		}
		else if (playerHealth <= 0)
		{
			ShowGameOver("You lost!");
		}
		else if (enemyHealth <= 0)
		{
			ShowGameOver("You won!");
		}
	}
	
	private void ShowGameOver(string message)
	{
		EndTurnButton.Disabled = true;

		var layer = GetParent().GetNodeOrNull<CanvasLayer>("GameOverLayer");
		if (layer == null) return;

		var label = layer.GetNodeOrNull<Label>("Panel/ResultLabel");
		if (label != null) label.Text = message;

		layer.Visible = true;
	}

	private void UpdateHealthLabels()
	{
		if (PlayerHealthLabel != null) PlayerHealthLabel.Text = $"HP: {playerHealth}";
		if (EnemyHealthLabel  != null) EnemyHealthLabel.Text  = $"HP: {enemyHealth}";
	}

	// ── Enemy AI helpers ──────────────────────────

	private bool EnemyNeedsStudent()
	{
		foreach (var card in EnemyHand.CardsInHand)
		{
			if (card.Data != null && card.Data.Cost > 0)
			{
				int cost = card.Data.Cost;
				int students = 0;
				foreach (var c in EnemyHand.CardsInHand)
					if (c.Data != null && c.Data.Cost == 0) students++;
				if (students < cost) return true;
			}
		}
		return false;
	}

	private void PlayEnemyCard()
	{
		Card cardToPlay = null;
		foreach (var card in EnemyHand.CardsInHand)
		{
			if (card.Data != null && card.Data.Cost > 0) { cardToPlay = card; break; }
		}
		if (cardToPlay == null) { GD.Print("Enemy: no character card to play."); return; }

		int cost = cardToPlay.Data.Cost;
		var students = new List<Card>();
		foreach (var card in EnemyHand.CardsInHand)
			if (card != cardToPlay && card.Data != null && card.Data.Cost == 0)
				students.Add(card);

		if (students.Count < cost) { GD.Print($"Enemy: can't afford '{cardToPlay.Data.Name}'."); return; }

		CardSlot freeSlot = null;
		foreach (Node child in EnemySlots.GetChildren())
			if (child is CardSlot slot && slot.IsEmpty() && !slot.isPlayerSlot)
			{ freeSlot = slot; break; }

		if (freeSlot == null) { GD.Print("Enemy: no free slots."); return; }

		EnemyHand.RemoveCardFromHand((EnemyCard)cardToPlay);
		for (int i = 0; i < cost; i++) { EnemyHand.RemoveCardFromHand((EnemyCard)students[i]); students[i].QueueFree(); }

		cardToPlay.SetFaceDown(false);
		cardToPlay.GlobalPosition = freeSlot.GlobalPosition - cardToPlay.PivotOffset;
		cardToPlay.isPlaced = true;
		freeSlot.PlaceCard(cardToPlay);

		GD.Print($"Enemy played: {cardToPlay.Data.Name}");
	}
	
	public void _on_play_again_button_pressed()
	{
		GetTree().ReloadCurrentScene();
	}
	
	public void _on_main_menu_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}
}
