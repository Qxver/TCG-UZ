using Godot;
using System.Collections.Generic;

public partial class CardCollection : Node{
	public static CardCollection Instance;

	private Dictionary<string, int> cards = new();

	public override void _Ready(){
		Instance = this;
	}

	public void AddCard(CardData card){
		if (!cards.ContainsKey(card.Id))
			cards[card.Id] = 0;

		cards[card.Id]++;
	}

	public int GetAmount(string cardId){
		return cards.GetValueOrDefault(cardId, 0);
	}

	public Dictionary<string, int> GetAllCards(){
		return cards;
	}
}
