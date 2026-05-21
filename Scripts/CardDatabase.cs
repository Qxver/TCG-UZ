using Godot;
using System.Collections.Generic;

public partial class CardDatabase : Node{
	public static CardDatabase Instance;

	public List<CardData> AllCards = new();

	public override void _Ready(){
		Instance = this;

		LoadCards();
	}

	private void LoadCards(){
		var dir = DirAccess.Open("res://Cards");

		if (dir == null)
			return;

		dir.ListDirBegin();

		while (true){
			string file = dir.GetNext();

			if (file == "")
				break;

			if (file.EndsWith(".tres")){
				var card = ResourceLoader.Load<CardData>(
					$"res://Cards/{file}"
				);

				if (card != null)
					AllCards.Add(card);
			}
		}
	}
}
