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
		CardData card = DrawWeightedCard(allCards, random);
		CardCollection.Instance.AddCard(card);
		pulled.Add(card);
	}

	return pulled;
	}
	
	private CardData DrawWeightedCard(List<CardData> allCards, Random random)
{
	
	
	int DrawWeightedCard(int rarity) => rarity switch
	{
		1 => 40,
		2 => 20,
		3 => 10,
		4 => 5,
		_ => 1
	};

	int totalWeight = 0;
	foreach (var card in allCards)
		totalWeight += DrawWeightedCard(card.Rarity);

	int roll = random.Next(totalWeight);
	int total = 0;

	foreach (var card in allCards)
	{
		total += DrawWeightedCard(card.Rarity);
		if (roll < total)
			return card;
	}

	return allCards[0];
}
}
