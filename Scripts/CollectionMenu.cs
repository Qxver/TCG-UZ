using Godot;

public partial class CollectionMenu : Control
{
	[Export] public PackedScene Card;
	[Export] public GridContainer Grid;

public void _on_back_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	public override void _Ready()
	{
		Refresh();
	}

	public void Refresh()
	{
		foreach (Node child in Grid.GetChildren())
			child.QueueFree();

		var collection = CardCollection.Instance.GetAllCards();

		foreach (var pair in collection)
		{
			string cardId = pair.Key;

			CardData data = FindCard(cardId);
			if (data == null)
				continue;

			var card = Card.Instantiate<Node>();

			Grid.AddChild(card);

			var frontFace = card.GetNode("FrontFace");
			var stats = card.GetNodeOrNull<Node>("Stats");
			var rearFace = card.GetNodeOrNull<Node>("RearFace");

			var portrait = card.GetNodeOrNull<Sprite2D>("FrontFace/Portrait");

			if (portrait != null)
			{
				portrait.Texture = data.Portrait;
			}
			
		}
	}

	private CardData FindCard(string id)
	{
		foreach (var card in CardDatabase.Instance.AllCards)
		{
			if (card.Id == id)
				return card;
		}

		return null;
	}
}
