using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    public class BlueprintFurnace : Blueprint
    {
        public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
        {
            base.BlueprintComplete(rootCard, involvedCards, print);

            // Requires 5 stone
            string IngredientToReduceId = "card_stacktorio_ore_resource_stone";
            int TargetValueToReduce = 5;
            
            for (int i = 0; i < involvedCards.Count; i++)
            {
                if (TargetValueToReduce > 0 && involvedCards[i].CardData.Id == IngredientToReduceId)
                {                
                    int reduction = Mathf.Min(TargetValueToReduce, involvedCards[i].CardData.Value);

                    TargetValueToReduce -= reduction;
                    involvedCards[i].CardData.Value -= reduction;
                }
                if (TargetValueToReduce == 0)
                {
                    break;
                }
            }
        }
    }
}