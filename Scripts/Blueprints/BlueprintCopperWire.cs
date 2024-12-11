﻿using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    /*
    * Recepi:
    * 0.5 seconds +
    * 1x copper plate
    */
    public class BlueprintCopperWire : Blueprint
    {
        public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
        {
            base.BlueprintComplete(rootCard, involvedCards, print);

            List<string> ingredientIds = new List<string>
            {   "card_stacktorio_material_plate_copper"
            };

            List<int> ingredientQuantities = new List<int>
            {   1
            };

            for (int i = 0; i < involvedCards.Count; i++)
            {
                int index = ingredientIds.IndexOf(involvedCards[i].CardData.Id);
                
                if (index >= 0 && ingredientQuantities[index] > 0)
                {    
                    int reduction = Mathf.Min(ingredientQuantities[index], involvedCards[i].CardData.Value);
                    ingredientQuantities[index] -= reduction;
                    involvedCards[i].CardData.Value -= reduction;
                }                
            }

            // Create 1 extra copper wire as per recepi
            CardData cardData = WorldManager.instance.CreateCard(rootCard.transform.position, "card_stacktorio_crafting_component_wire_copper", checkAddToStack: false);
            cardData.Value = 1;
            WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
        }
    }
}