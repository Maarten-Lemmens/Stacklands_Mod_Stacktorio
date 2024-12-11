using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    public class Spaceship : CardData
	{
		public float TravelTime = 1f;

        protected override bool CanHaveCard(CardData otherCard)
        {
           return true;
        }
        
        public override bool CanHaveCardsWhileHasStatus()
        {
            return true;
        }

        public override void UpdateCard()
        {
            if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
            {
                int num = ChildrenMatchingPredicateCount((CardData x) => x is BaseVillager);
                if (num > 0)
                {
                    MyGameCard.StartTimer(TravelTime, FlyOff, SokLoc.Translate("card_stacktorio_spaceship_status"), GetActionId("FlyOff"));
                }
                else
                {
                    MyGameCard.CancelTimer(GetActionId("FlyOff"));
                }
            }
        }

        [TimedAction("fly_off")]
        public void FlyOff()
        {
            if (!TransitionScreen.InTransition && !WorldManager.instance.InAnimation)
            {
                //GameCanvas.instance.ChangeLocationPrompt(GoAway, Stay, "island");
                GoAway();
            }
        }

        private void GoAway()
        {
            // Clean board + create spaceship & engineer
            WorldManager.instance.RemoveAllCardsFromBoard("main");
            
            // Spaceship
            // TODO: make it a broken space-ship
            CardData cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "card_stacktorio_spaceship", checkAddToStack: false);
            cardData.MyGameCard.SendIt();
            
            // Villager
            cardData = WorldManager.instance.CreateCard(WorldManager.instance.MiddleOfBoard(), "card_stacktorio_villager_engineer", checkAddToStack: false);
            cardData.MyGameCard.SendIt();
            
            // Iron ore field
            cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "card_stacktorio_resource_patch_iron", checkAddToStack: false);
            cardData.MyGameCard.SendIt();

            // Coal field
            cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "card_stacktorio_resource_patch_coal", checkAddToStack: false);
            cardData.MyGameCard.SendIt();

            // Stone field
            cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "card_stacktorio_resource_patch_stone", checkAddToStack: false);
            cardData.MyGameCard.SendIt();

            // Copper field
            cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "card_stacktorio_resource_patch_copper", checkAddToStack: false);
            cardData.MyGameCard.SendIt();
            
            // Research: Automation
            cardData = WorldManager.instance.CreateCard(WorldManager.instance.GetRandomSpawnPosition(), "card_stacktorio_research_automation", checkAddToStack: false);
            cardData.MyGameCard.SendIt();

            WorldManager.instance.CreateSmoke(cardData.transform.position);           
        }
    }
}