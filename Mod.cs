using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    public class StacktorioMod : Mod
    {
        
        private void Awake()
        {
            EnumHelper.ExtendEnum<Location>("Stacktorio");
            
            


            //QuestGroup customQuestGroup = EnumHelper.ExtendEnum<QuestGroup>("Stacktorio");
            

        
        }
        
        
        public override void Ready()
        {
            
            //int customLocation = EnumHelper.ExtendEnum<Location>("Stacktorio");
            
            // WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "blueprint_advanced_breeding_station", 1);
            
            //StactorioBoard.CreateBoard("stacktorio", null);
            
            
            
            
            //WorldManager.instance.Boards.Add(new StactorioBoard());



            //WorldManager.instance.GameBoard._location.Add("Stacktorio");

            /*
            Quest BuildSpaceshipQuest = new Quest("build_spaceship");
            BuildSpaceshipQuest.OnActionComplete = (CardData card, string action) => (card.Id == "blueprint_spaceship" && action == "finish_blueprint") ? true : false;
            BuildSpaceshipQuest.QuestGroup = QuestGroup.Starter;
            BuildSpaceshipQuest.IsSteamAchievement = false;
            */

            //customQuest.DescriptionTermOverride = "board_stacktorio_name";
            //customQuest.RequiredCount = 1;
            //customQuest.DefaultVisible = true;

            //QuestManager.instance.AllQuests.Add(BuildSpaceshipQuest);
            //QuestManager.instance.UpdateCurrentQuests();


            /*

            string boardId = "Stacktorio";

            GameBoard existing = WorldManager.instance.GetBoardWithLocation(Location.Mainland);
            Logger.Log("test: " + existing);
            if (existing != null)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
                WorldManager.instance.Boards.Remove(existing);
            }
            GameBoard board = Instantiate(WorldManager.instance.Boards[1]);
            board.Id = boardId;

            board.transform.position = new Vector3((WorldManager.instance.Boards.Count - 1) * 100, -0.528f, 0);
            WorldManager.instance.Boards.Add(board);
            CreatePackLine packLine = board.GetComponentInChildren<CreatePackLine>();
            if (packLine != null) packLine.gameObject.SetActive(false);
            //return board;
            */


        }


    }


    


    public class ResourcePatch : CardData
	{
        public ResourceType ResourceType;
        public int ResourceDepth = 0; 

        public override void OnInitialCreate()
        {
            if (Id == "card_stacktorio_resource_patch_iron")
                ResourceType = ResourceType.iron;
            else if (Id == "card_stacktorio_resource_patch_copper")
                ResourceType = ResourceType.copper;
            else if (Id == "card_stacktorio_resource_patch_stone")
                ResourceType = ResourceType.stone;
            else if (Id == "card_stacktorio_resource_patch_coal")
                ResourceType = ResourceType.coal;

            this.ResourceDepth = 50000;
            IsOn = true;
        }
        
        protected override bool CanToggleOnOff() => true;

        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        public override bool CanBeDragged => false;

        public override bool CanBePushedBy(CardData otherCard) => false;

        public override bool CanHaveCardsWhileHasStatus() => true;

        protected override bool CanHaveCard(CardData otherCard)
		{
			if (otherCard.MyCardType is CardType.Humans || otherCard.Id == "card_stacktorio_resource_extraction_mining_drill_burner")
				return true;
			return false;
		}

        public override void UpdateCard()
		{
            Value = ResourceDepth;
            if (ResourceDepth <= 0)
			{
				IsOn = false;
			}
		}

        // Generates 1 ore-card, for a certain value, based on the type of the resource patch.
        public void MineOreManually()
        {    
            if (ResourceDepth > 0) {
                ResourceDepth -= 1;
                CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_"+ResourceType, checkAddToStack: false);
                cardData.Value = 1;
                WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
            }
        }
    }

    public class MiningDrillBurner : CardData
	{   
        public static int MaximumStackSize = 50;
        public int stackSizeIron = 0;
        public int stackSizeCoal = 0;
        public int stackSizeCopper = 0;
        public int stackSizeStone = 0;
        public int fuelRemaining = 0; // 7 items per single unit of coal is used to fuel it, consumed 1 coal to put fuelRemaining to 7.

        public override bool DetermineCanHaveCardsWhenIsRoot => true;
        
        public override bool CanHaveCardsWhileHasStatus()
		{
			return true;
		}

        public bool CanHaveCardOnTop(CardData otherCard, bool isPrefab = false)
        {
            return true;
        }

        protected override bool CanHaveCard(CardData otherCard)
		{
			if (otherCard is MiningDrillBurner || otherCard.Id == "card_stacktorio_ore_resource_coal")
            {
                return true;
            }
			return false;
		}
        
        public override void UpdateCard()
		{
            MyGameCard.CardData.Value = stackSizeIron + stackSizeCoal + stackSizeCopper + stackSizeStone;
            
            GameCard rootParent = MyGameCard.GetRootCard();
            if (rootParent.CardData is ResourcePatch)
            {
                if (rootParent.CardData.IsOn && MyGameCard.CardData.Value < MiningDrillBurner.MaximumStackSize)
                {
                    // Check Fuel
                    if (fuelRemaining > 0)
                    {
                        MyGameCard.CardData.IsOn = true;
                        MyGameCard.StartTimer(0.625f, MiningOre, SokLoc.Translate("stacktorio_resource_extraction_mining_drill_burner_status"), GetActionId("MiningOre"));
                    }
                    else
                    {
                       if (MyGameCard.GetLeafCard().CardData.Id == "card_stacktorio_ore_resource_coal") // if last card = coal
                       {
                           ((Resources)MyGameCard.GetLeafCard().CardData).Value -= 1;
                           fuelRemaining = 7;
                       }
                       else
                       {
                           MyGameCard.CancelTimer(GetActionId("MiningOre"));
                           MyGameCard.CardData.IsOn = false;
                       }
                    }
                }
                else
                {
                    MyGameCard.CancelTimer(GetActionId("MiningOre"));
                    MyGameCard.CardData.IsOn = false;
                }
            }
            else
            {
                MyGameCard.CancelTimer(GetActionId("MiningOre"));
                MyGameCard.CardData.IsOn = false;
            }
            if (! MyGameCard.CardData.IsOn)
            {
                AddStatusEffect(new StatusEffect_CardOff());
            }
            else
            {
                RemoveStatusEffect<StatusEffect_CardOff>();
            }
        }

        public override void Clicked()
        {
            if (MyGameCard.CardData.Value > 0)
            {
                if (stackSizeCoal > 0)
                {
                    CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_coal", checkAddToStack: false);
                    cardData.Value = stackSizeCoal;
                    WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                    stackSizeCoal = 0;
                }
                if (stackSizeCopper > 0)
                {
                    CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_copper", checkAddToStack: false);
                    cardData.Value = stackSizeCopper;
                    WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                    stackSizeCopper = 0;
                }
                if (stackSizeIron > 0) 
                {
                    CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_iron", checkAddToStack: false);
                    cardData.Value = stackSizeIron;
                    WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                    stackSizeIron = 0;
                }
                if (stackSizeStone > 0) 
                {
                    CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_stone", checkAddToStack: false);
                    cardData.Value = stackSizeStone;
                    WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                    stackSizeStone = 0;
                }
                
                MyGameCard.CardData.Value = 0;
            }
        }

        /*
        * Checks if an (ore)-card can be extracted from the Miner's inventory.
        * If so then return an (ore)-card of with value Math.Min(hand-size of requester _versus_ internal stack size preesnt in  of miner's inventory)
        */
        public CardData RemoveResources(int count)
        {
            if (MyGameCard.CardData.Value > 0)
            {
                if (stackSizeCopper > 0)
                {
                    CardData removedOreCard = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_copper", checkAddToStack: false);
                    int removedOre = Mathf.Min(count, stackSizeCopper);

                    //MyGameCard.CardData.Value -= removedOre;
                    stackSizeCopper -= removedOre;
                    removedOreCard.Value = removedOre;
                    
                    return removedOreCard;
                }
                else if (stackSizeIron > 0) 
                {
                    CardData removedOreCard = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_iron", checkAddToStack: false);
                    int removedOre = Mathf.Min(count, stackSizeIron);

                    //MyGameCard.CardData.Value -= removedOre;
                    stackSizeIron -= removedOre;
                    removedOreCard.Value = removedOre;
                    
                    return removedOreCard;
                }
                else if (stackSizeStone > 0) 
                {
                    CardData removedOreCard = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_stone", checkAddToStack: false);
                    int removedOre = Mathf.Min(count, stackSizeStone);

                    //MyGameCard.CardData.Value -= removedOre;
                    stackSizeStone -= removedOre;
                    removedOreCard.Value = removedOre;
                    
                    return removedOreCard;
                }
                else if (stackSizeCoal > 0)
                {
                    CardData removedOreCard = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_coal", checkAddToStack: false);
                    int removedOre = Mathf.Min(count, stackSizeCoal);

                    //MyGameCard.CardData.Value -= removedOre;
                    stackSizeCoal -= removedOre;
                    removedOreCard.Value = removedOre;
                    
                    return removedOreCard;
                }
            }
            return null;
        }

        [TimedAction("mining_ore")]
        public void MiningOre()
        {
            GameCard rootParent = MyGameCard.GetRootCard();
            if (rootParent.CardData is ResourcePatch && rootParent.CardData.IsOn && MyGameCard.CardData.IsOn && fuelRemaining > 0)
            {
                fuelRemaining -= 1; // fuel consumed
                ((ResourcePatch)rootParent.CardData).ResourceDepth -= 1; // reduce value of ore patch
                MyGameCard.CardData.Value += 1;
                
                ResourcePatch parentResourcePatch = (ResourcePatch)MyGameCard.GetRootCard().CardData;
                switch(parentResourcePatch.ResourceType)
                {
                    case ResourceType.iron:
                        stackSizeIron += 1;
                        break;
                    case ResourceType.coal:
                        stackSizeCoal += 1;
                        break;
                    case ResourceType.copper:
                        stackSizeCopper += 1;
                        break;
                    case ResourceType.stone:
                        stackSizeStone += 1;
                        break;
                    default:
                        break;
                }

                if (MyGameCard.CardData.Value >= MiningDrillBurner.MaximumStackSize) {
                    MyGameCard.CardData.IsOn = false;
                }
            }
        }
    }

    // Coal consumption: 0.0225/sec
    public class FurnaceStone : CardData
	{   
        public static int MaximumStackSize = 50;
        public int stackSizeIronPlate = 0;
        public int stackSizeCopperPlate = 0;
        public int stackSizeStoneBrick = 0;
        public int stackSizeSteelPlate = 0;

        // coal, copper, iron, iron plate, stone, 
        public string[] inputCardsAccepted = {"card_stacktorio_ore_resource_coal", "card_stacktorio_ore_resource_copper", "card_stacktorio_ore_resource_iron", "card_stacktorio_material_plate_iron", "card_stacktorio_ore_resource_stone"};
        public int[] inputStackSizes = {0, 0, 0, 0, 0};

        // TODO: CHANGE from fuel/items --> to fuel/second
        public int fuelRemaining = 0; // 4 items per single unit of coal is used to fuel it, consumed 1 coal to put fuelRemaining to 7.

        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        public override bool CanHaveCardsWhileHasStatus()
		{
			return true;
		}

        // Accepts as input: iron ore, copper ore, stone, iron plates, coal (as fuel)
        protected override bool CanHaveCard(CardData otherCard)
		{
			return inputCardsAccepted.Contains(otherCard.Id);
		}
        
        public override void UpdateCard()
		{
            if (MyGameCard.Child != null)
            {
                if (fuelRemaining <= 0)
                {
                    if (AnyChildMatchesPredicate((CardData x) => x.Id == "card_stacktorio_ore_resource_coal")) // check for coal & if existing then consume it
                    {
                        List<CardData> childCoals = ChildrenMatchingPredicate((CardData x) => x.Id == "card_stacktorio_ore_resource_coal");
                        childCoals[0].Value -= 1;
                        fuelRemaining = 4;
                    }
                    else
                    {
                        //MyGameCard.CancelTimer(GetActionId("SmeltingOre"));
                        MyGameCard.CardData.IsOn = false;
                    }
                }
                else
                {
                    if (MyGameCard.CardData.Value >= MaximumStackSize)
                    {
                        MyGameCard.CardData.IsOn = false;
                    }
                    else
                    {
                        MyGameCard.CardData.IsOn = true;
                    }
                    

                    // TODO: avoid searching the same list x times per type of resource. Obtain list + check if "contains" at least 1 card...
                    /*
                    if (AnyChildMatchesPredicate((CardData x) => x.Id =="card_stacktorio_ore_resource_Iron") || AnyChildMatchesPredicate((CardData x) => x.Id == "card_stacktorio_ore_resource_Copper"))
                    {
                        MyGameCard.StartTimer(0.3125f, SmeltingOre, SokLoc.Translate("stacktorio_furnace_stone_status"), GetActionId("SmeltingOre"));
                    }
                    else
                    {
                        MyGameCard.CancelTimer(GetActionId("SmeltingOre"));
                        MyGameCard.CardData.IsOn = false;
                    }
                    */
                }
            }
            else
            {
                //MyGameCard.CancelTimer(GetActionId("SmeltingOre"));
                MyGameCard.CardData.IsOn = false;
            }

            // Visualize on/off conditions
            if (! MyGameCard.CardData.IsOn)
            {
                AddStatusEffect(new StatusEffect_CardOff());
            }
            else
            {
                RemoveStatusEffect<StatusEffect_CardOff>();
            }

            base.UpdateCard();
        }







        public override void Clicked()
        {
            if (MyGameCard.CardData.Value > 0)
            {
                for (int i = 0; i < inputStackSizes.Count(); i++)
                {
                    if (inputStackSizes[i] > 0)
                    {
                        CardData cardData = WorldManager.instance.CreateCard(base.transform.position, inputCardsAccepted[i], checkAddToStack: false);
                        cardData.Value = inputStackSizes[i];
                        WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                        inputStackSizes[i] = 0;
                    }
                }
                
                /*
                if (stackSizeIronPlate > 0) {
                    CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_material_plate_iron", checkAddToStack: false);
                    cardData.Value = stackSizeIronPlate;
                    WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                    stackSizeIronPlate = 0;
                }
                if (stackSizeCopperPlate > 0) {
                    CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_material_plate_copper", checkAddToStack: false);
                    cardData.Value = stackSizeCopperPlate;
                    WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                    stackSizeCopperPlate = 0;
                }
                /*
                if (stackSizeIron > 0) {
                    CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_Iron", checkAddToStack: false);
                    cardData.Value = stackSizeIron;
                    WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                    stackSizeIron = 0;
                }
                if (stackSizeStone > 0) {
                    CardData cardData = WorldManager.instance.CreateCard(base.transform.position, "card_stacktorio_ore_resource_Stone", checkAddToStack: false);
                    cardData.Value = stackSizeStone;
                    WorldManager.instance.StackSendCheckTarget(MyGameCard, cardData.MyGameCard, OutputDir);
                    stackSizeStone = 0;
                }
                */
            }
            MyGameCard.CardData.Value = 0;
        }

        [TimedAction("smelting_ore")]
        public void SmeltingOre()
        {
            if (MyGameCard.CardData.IsOn && fuelRemaining > 0)
            {
                if (MyGameCard.CardData.Value >= FurnaceStone.MaximumStackSize) {
                    MyGameCard.CardData.IsOn = false;
                }
            }
        }

        // Method to update current fuel.
        public void UpdateFuelAndStacks(string blueprintCardId)
        {
            fuelRemaining -= 1;
            
            switch(blueprintCardId)
            {
                case "blueprint_stacktorio_material_plate_iron":
                    stackSizeIronPlate += 1;
                    MyGameCard.CardData.Value += 1;
                    break;
                case "blueprint_stacktorio_material_plate_copper":
                    stackSizeCopperPlate += 1;
                    MyGameCard.CardData.Value += 1;
                    break;
                default:
                    break;
            }
        }
    }

    public class Engineer : BaseVillager
    {
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
            if (MyGameCard.HasParent && MyGameCard.Parent.CardData is ResourcePatch && MyGameCard.Parent.CardData.IsOn)
            {
                MyGameCard.StartTimer(0.75f, MiningOre, SokLoc.Translate("stacktorio_resource_extraction_mining_drill_burner_status"), GetActionId("MiningOre"));
            }
            else
            {
                MyGameCard.CancelTimer(GetActionId("MiningOre"));
            }
            base.UpdateCard();
        }
    
        [TimedAction("mining_ore")]
        public void MiningOre()
        {
            if (MyGameCard.HasParent && MyGameCard.Parent.CardData is ResourcePatch)
            {
                ((ResourcePatch)MyGameCard.Parent.CardData).MineOreManually();
            }
        }
    }

    public class Resources : CardData
    {
        protected override bool CanHaveCard(CardData otherCard) => true;
        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        public override void UpdateCard()
		{
            if (MyGameCard.CardData.Value <= 0)
            {
                GameCard parent = MyGameCard.Parent;
                GameCard child = MyGameCard.Child;

                if (MyGameCard.Parent != null && MyGameCard.Child != null)
                {
                    MyGameCard.RemoveFromStack();
                    child.SetParent(parent);
                }
                MyGameCard.DestroyCard(spawnSmoke: true, playSound: false);
            }
            else
            {
                base.UpdateCard();
            }
        }
    }

    public class Science : CardData
    {
        protected override bool CanHaveCard(CardData otherCard) => true;
        public override bool DetermineCanHaveCardsWhenIsRoot => true;
        public ScienceType scienceType;

        public override void UpdateCard()
		{
            if (MyGameCard.CardData.Value <= 0)
            {
                GameCard parent = MyGameCard.Parent;
                GameCard child = MyGameCard.Child;

                if (MyGameCard.Parent != null && MyGameCard.Child != null)
                {
                    MyGameCard.RemoveFromStack();
                    child.SetParent(parent);
                }
                MyGameCard.DestroyCard(spawnSmoke: true, playSound: false);
            }
            else
            {
                base.UpdateCard();
            }
        }

        public override void OnInitialCreate()
        {
            if (Id == "card_stacktorio_science_pack_red")
                scienceType = ScienceType.red; 
        }

    }

    public class MaterialPlates : CardData
    {
        protected override bool CanHaveCard(CardData otherCard) => true;
        public override bool DetermineCanHaveCardsWhenIsRoot => true;
    }











    public class Research : CardData
    {
        /*
        * string that indicates which specific science-types are required, with 1 indicating (required) & 0 indicating (not required)
        * For the following science-types in this specific order from left to right (red, green, black, blue, purple, yellow, white)
        */
        public int[] ScienceTypesRequired = new int[7];
        public float ResearchRoundDuration;
        public bool IsResearchFullyCompleted = false;
        public string[] ResultCards;

        protected override bool CanHaveCard(CardData otherCard) => true;
        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        /*
        * Helper method to process that 1 research-round was completed.
        */
        public void ResearchRoundCompleted() {
            
            bool completed = true; // To track if research was fully completed.

            for (int i = 0; i < ScienceTypesRequired.Length; i++)
            {
                // If it was a required science-type, then reduce by 1 to indicate that a research-round was completed.
                if (ScienceTypesRequired[i] > 0)
                    ScienceTypesRequired[i] -= 1;
                
                // If there is still research required, then don't consider this to be completed
                if (ScienceTypesRequired[i] > 0)
                    completed = false;
            }

            IsResearchFullyCompleted = completed;
        }

        public virtual void ResearchFullyCompleted()
        {
            if (IsResearchFullyCompleted)
            {
                CardData cardData;

                for (int i = 0; i < ResultCards.Length; i++)
                {
                    cardData = WorldManager.instance.CreateCard(MyGameCard.transform.position, ResultCards[i], checkAddToStack: false);
                    cardData.MyGameCard.SendIt();
                }
            }
        }
    }

    /*
    * Recepi:
    * 10 seconds (per red science)
    * 1x Lab
    * 10x red science
    */
    public class ResearchAutomation : Research
    {
        public override void OnInitialCreate()
        {
            
        }

        // When this card is loaded onto the board (newly created or reloaded from open an existing game), then overwrite the description to add the recepi to research it.
        protected override void Awake()
        {
            ScienceTypesRequired = [10, 0, 0, 0, 0, 0, 0]; // Indicating 10x research-rounds are required (each time consuming only red science).
            ResearchRoundDuration = 10f; // 10 second per research-round
            ResultCards = ["blueprint_stacktorio_production_assembling_machine_1", "blueprint_stacktorio_inserters_inserter_long_handed"];
            
            UpdateCardText();
        }
        
        // Build description dynamically based on the amount of science that is still required.
        public override void UpdateCardText()
        {
            descriptionOverride = "1x " + SokLoc.Translate("card_stacktorio_production_lab_name") + "\n" +
                                  "1x " + SokLoc.Translate("card_stacktorio_research_automation_name") + "\n";
            
            for (int i = 0; i < ScienceTypesRequired.Length; i++)
            {
                // Only mention science-type in case it is actually required.
                if (ScienceTypesRequired[i] > 0)
                {
                    descriptionOverride += ScienceTypesRequired[i] + "x " + SokLoc.Translate("card_stacktorio_science_pack_" + Enum.GetName(typeof(ScienceType), i) + "_name") + "\n";
                }
            }
            descriptionOverride += "\n" + SokLoc.Translate(DescriptionTerm);
        }
    }

    /*
    * The Lab takes as input:
    * - A research-card (currently defined as CardType: Location)
    * - One or more Science-packs (red, green, etc.)
    *
    * Status:
    * - On: 
    *** - When a research-card is on-top && 
    *** - There is "the right combination of science-fuel" present inside
    * - Off: 
    *** - Otherwise the lab is switched off by default.
    *
    *
    */
    public class Lab : CardData
	{   
        public static int MaximumStackSizePerScience = 10;
        public int[] StackSizeScienceTypes = [0, 0, 0, 0, 0, 0, 0];

        public override bool DetermineCanHaveCardsWhenIsRoot => true;

        public override bool CanHaveCardsWhileHasStatus()
		{
			return true;
		}

        // Accepts as input: research-card(s) & science-cards
        protected override bool CanHaveCard(CardData otherCard)
		{
			// TODO: Check if we can change it to "Research" instead of abusing the existing "Locations" type.
            if (otherCard.MyCardType is CardType.Locations || otherCard is Science)
            {
                return true;
            }
			return false;
		}
        
        // Build description dynamically based on the amount of science that is inside the Lab.
        public override void UpdateCardText()
        {
            descriptionOverride = "Contains:\n";
            for (int i = 0; i < StackSizeScienceTypes.Length; i++)
            {
                descriptionOverride += StackSizeScienceTypes[i] + "x " + SokLoc.Translate("card_stacktorio_science_pack_" + Enum.GetName(typeof(ScienceType), i) + "_name") + "\n";
            }
            
            descriptionOverride += "\n" + SokLoc.Translate(DescriptionTerm);
        }

        public override void UpdateCard()
		{
            if (MyGameCard.Child != null)
            {
                List<CardData> ChildrenScienceCards = ChildrenMatchingPredicate((CardData x) => x is Science);
                List<CardData> ChildrenResearchCards = ChildrenMatchingPredicate((CardData x) => x is Research);
                
                /*
                * For each science-card in the stack:
                * - Check if it can be consumed + update the Lab's internal stack-size for that specific science-type.
                */
                for (int i = 0; i < ChildrenScienceCards.Count; i++)
                {
                    // If the internal stack size (of the Lab) for this specific science-type is not at its maxium yet, then process the science-card.
                    if (StackSizeScienceTypes[(int)((Science)ChildrenScienceCards[i]).scienceType] < MaximumStackSizePerScience)
                    {
                        // Process science-card, by reducing its value (up to the Lab's MaximumStackSizePerScience)
                        // Check the minimum between (maximum science stack size - current stack size for the type) versus (the value of the science-card being absorbed)
                        int absorbsion = Mathf.Min(MaximumStackSizePerScience - StackSizeScienceTypes[(int)((Science)ChildrenScienceCards[i]).scienceType], ChildrenScienceCards[i].Value);
                        StackSizeScienceTypes[(int)((Science)ChildrenScienceCards[i]).scienceType] += absorbsion; // Add the absorbsion to the Lab's internal stack size (for that science-type)
                        ChildrenScienceCards[i].Value -= absorbsion; // Reduce the absorbtion from the science-card.
                    }
                }

                /*
                * Check if there is a Research-card present in the stack (only take the fist one into).
                * If so, then check if all required science types are present within the labs (using a helper method).
                */
                if (ChildrenResearchCards.Count > 0)
                {
                    Research childResearchCard = (Research)ChildrenResearchCards[0];
                    if (IsRequiredScienceTypesAvailable(childResearchCard.ScienceTypesRequired) && ! childResearchCard.IsResearchFullyCompleted)
                    {
                        // All required science-types (for this specific research) are present within the lab.
                        MyGameCard.CardData.IsOn = true;
                        MyGameCard.StartTimer(childResearchCard.ResearchRoundDuration, Researching, SokLoc.Translate(childResearchCard.Id + "_status"), GetActionId("Researching"));
                    }
                    else
                    {
                        // Not enough science available within the Lab to start a new research round.
                        MyGameCard.CardData.IsOn = false;
                    }                    
                }
                else
                {
                    // No research-card present (anymore)
                    MyGameCard.CardData.IsOn = false;
                    MyGameCard.CancelTimer(GetActionId("Researching"));
                }
            }
            else
            {
                // No (more) card on the Lab
                MyGameCard.CardData.IsOn = false;
                MyGameCard.CancelTimer(GetActionId("Researching"));
            }

            // Visualize on/off conditions
            if (! MyGameCard.CardData.IsOn)
            {
                AddStatusEffect(new StatusEffect_CardOff());
            }
            else
            {
                RemoveStatusEffect<StatusEffect_CardOff>();
            }

            base.UpdateCard();
        }

        /*
        * Helper method to decide if the Lab can continue processing or not.
        * It can only continue if (for each required science type) there is also (at least one science-fuel present).
        */
        public bool IsRequiredScienceTypesAvailable(int[] ScienceTypesRequired)
        {
            for (int i = 0; i < ScienceTypesRequired.Length; i++)
            {
                // If (I need the specific science-type) AND (I do not have it available) then stop processing.
                if (ScienceTypesRequired[i] > 0 && StackSizeScienceTypes[i] <= 0)
                    return false;
            }
            return true;
        }

        /*
        * When the research-timer end, reduce the Lab's internal science-stacks,
        * depending on the research-types that were required for that specific Research being processed.
        */
        [TimedAction("researching")]
        public void Researching()
        {
            // IsOn = Labs still has a research-card on-top + there is still enough science-fuel available internally to complete the research-round.
            if (MyGameCard.CardData.IsOn)
            {
                List<CardData> ChildrenResearchCards = ChildrenMatchingPredicate((CardData x) => x is Research);
                if (ChildrenResearchCards.Count > 0)
                {
                    Research childResearchCard = (Research)ChildrenResearchCards[0];

                    /*
                    * The sequence of the below steps is very important. First resolve the Lab's science-fuel. Second resolve the Research-card status.
                    * STEP 1 --> Reduce the Lab's internal science-fuels (based on the Research-science-types that were required)
                    */
                    LabResearchRoundCompleted(childResearchCard.ScienceTypesRequired);
                    
                    // STEP 2 --> Complete 1 research round from the Research-card.
                    childResearchCard.ResearchRoundCompleted();

                    // STEP 3 --> Check if the Research-card was fully researched
                    if (childResearchCard.IsResearchFullyCompleted)
                    {
                        // STEP 4 --> Address the outcome of completing said Research-card.
                        childResearchCard.ResearchFullyCompleted();
                    }
                }
                else
                {
                    MyGameCard.CardData.IsOn = false;
                }
            }
        }

        /*
        * Helper method from the Lab to process that 1 research-round was completed.
        * Important: call this one _BEFORE_ you process the research-round of the Research-card.
        * Why? Because it uses the Research.ScienceTypesRequired int[] to reduce the right science-types from the Lab...
        */
        public void LabResearchRoundCompleted(int[] ResearchScienceTypesRequired) {
            
            for (int i = 0; i < ResearchScienceTypesRequired.Length; i++)
            {
                // If it was a required science-type (as defined by the Research-card), then reduce the Lab's internal science-fuel-stack by 1.
                if (ResearchScienceTypesRequired[i] > 0)
                    StackSizeScienceTypes[i] -= 1;
            }
        }
    }

    
}