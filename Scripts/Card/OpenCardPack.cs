using Godot;
using System;
using System.Collections.Generic;

public partial class OpenCardPack : Node{
	[Export] public int CardsPerPack = 5;

	public List<CardData> OpenPack()
	{
	var allCards = CardDatabase.Instance.AllCards;
	var pulled = new List<CardData>();
	Random random = new();

	for (int i = 0; i < CardsPerPack; i++)
	{
		int index = random.Next(allCards.Count);
		CardData card = allCards[index];
		CardCollection.Instance.AddCard(card);
		pulled.Add(card);
	}

	return pulled;
	}
}
