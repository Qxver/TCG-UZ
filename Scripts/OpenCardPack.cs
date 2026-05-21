using Godot;
using System;

public partial class OpenCardPack : Node{
	[Export] public int CardsPerPack = 5;

	public void OpenPack(){
		var allCards = CardDatabase.Instance.AllCards;

		Random random = new();

		for (int i = 0; i < CardsPerPack; i++){
			int card_index = random.Next(allCards.Count);

			CardData card = allCards[card_index];

			CardCollection.Instance.AddCard(card);

			GD.Print($"Pulled: {card.Name}");
			}
		}
	}
