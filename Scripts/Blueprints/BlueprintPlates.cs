using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    /*
    * Blueprint script to make sure the value of 1 iron_ore is reduced. 
    * Without destroying all cards (I made the iron_ore cards of type "Structures" to avoid deletion upon recepi-completion).
    * Without destroying the whole stack (= all involvedCards), even the ones that were not relevant to the recepi
    * --> As a result I had to hard-code the exact card-id to look for & break ofter 1 ore-card was encountered --> UGLY but it will do (for now).
    */
    public class BlueprintPlates : Blueprint
    {
        public override void BlueprintComplete(GameCard rootCard, List<GameCard> involvedCards, Subprint print)
        {
            base.BlueprintComplete(rootCard, involvedCards, print);

            // Define which raw ingredient to reduce.
            string IngredientToReduceId = "";
            if (CardId == "blueprint_stacktorio_material_plate_iron")
            {
                IngredientToReduceId = "card_stacktorio_ore_resource_iron";
            }
            else
            {
                if (CardId == "blueprint_stacktorio_material_plate_copper")
                {
                    IngredientToReduceId = "card_stacktorio_ore_resource_copper";
                }
            }



            if (IngredientToReduceId != "")
            {
                if (rootCard.CardData.Id == "card_stacktorio_furnace_stone")
                {
                    Debug.Log("BlueprintPlates - furnace stone");
                    FurnaceStone furnaceStone = (FurnaceStone) rootCard.CardData;
                    furnaceStone.UpdateFuelAndStacks(CardId); // send cardId of the blueprint
                }

                if (rootCard.CardData.Id == IngredientToReduceId)
                {
                    rootCard.CardData.Value -= 1;
                }
                else
                {
                    for (int i = involvedCards.Count-1; i > 0; i--)
                    {
                        if (involvedCards[i].CardData.Id == IngredientToReduceId)
                        {                
                            involvedCards[i].CardData.Value -= 1;
                            break; // break after 1 'iron ore' was consumed
                        }
                    }
                }
            }
        }
    }
}