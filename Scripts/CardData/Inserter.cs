using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace StacktorioModNS
{
    public class Inserter : Conveyor
	{   
        public int HandSize;
        private Vector2[] corners = new Vector2[4];
        private CardData pickedUpCard;

        protected override void Awake()
        {
            TotalTime = 1f;
            HandSize = 1;
            MyGameCard.CardData.Value = 0;
        }

        /*
        * Check source card: not null && can be picked-up
        * Check target card: null || same type || (forge/assembler/lab && room available for source-card)
        *
        */
        public override void UpdateCard()
        {
            // 1. If no timer running && nothing picked-up yet --> then check to pick something up.
            if (MyGameCard.TimerRunning == false && pickedUpCard == null)
            {
                pickedUpCard = GetConveyableCardFromInputCard(GetSourceCard(allowDraggingCards: false));
            }
            
            if (MyGameCard.TimerRunning == false && pickedUpCard != null)
            {
                //MyGameCard.StartTimer(TotalTime, LoadCard, SokLoc.Translate("card_stacktorio_inserters_inserter_status"), GetActionId("LoadCard"));
                
                MyGameCard.CardData.Value = pickedUpCard.Value;
                descriptionOverride = SokLoc.Translate("card_stacktorio_inserters_inserter_status") + "\n" + 
                                  MyGameCard.CardData.Value + "x " + pickedUpCard.Name;
            }

            if (pickedUpCard == null) 
            {
                MyGameCard.CancelAnyTimer();
                
                MyGameCard.CardData.Value = 0;
                descriptionOverride = SokLoc.Translate("card_stacktorio_inserters_inserter_description");
            }
            
            DrawArrows(GetSourceCard(allowDraggingCards: false), GetTargetCard(allowDraggingCards: false));
        }
        

        [TimedAction("load_card")]
        public void LoadCard()
        {
            if (pickedUpCard == null)
                return;
            
            // If target still has capacity.
            CardData targetCard = GetTargetCard(allowDraggingCards: false);
            if (targetCard != null)
            {
                if (targetCard is FurnaceStone && ((FurnaceStone)targetCard).HasCapacityLeft(pickedUpCard))
                {
                    


                }









                //if (targetCard.hasCapacity())
                //{
                    SendToTargetCard(pickedUpCard.MyGameCard, targetCard.MyGameCard);
                    pickedUpCard = null;
                //}
                //else
                
            }
            else
            {
                /*
                if (pickedUpCard.MyGameCard.BounceTarget == targetCard.MyGameCard)
                {
                    pickedUpCard.MyGameCard.BounceTarget = null;
                }
                */
                pickedUpCard.MyGameCard.SendToPosition(MyGameCard.transform.position - directionVector);
                pickedUpCard = null;
            }
            
        }


        /*
        * Pick-ups a card, currently supported:
        * - Ores from Mining Drills
        * 
        */
        private CardData GetConveyableCardFromInputCard(CardData card)
        {
            if (card == null)
                return null;
            
            GameCard child = card.MyGameCard.Child;
            while (child != null)
            {
                if (child.CardData is MiningDrillBurner { Value: > 0 } miningDrill)
                {
                    return miningDrill.RemoveResources(HandSize);
                }
                child = child.Child;
            }
            return null;
        }

        private void SendToTargetCard(GameCard card, GameCard targetCard)
        {
            Vector3 vector = targetCard.transform.position - card.transform.position;
            vector.y = 0f;
            Vector3 value = new Vector3(vector.x * 4f, 7f, vector.z * 4f);
            card.BounceTarget = targetCard.GetRootCard();
            card.Velocity = value;
        }


        private void DrawArrows(CardData sourceCard, CardData targetCard)
        {
            DrawInputArrow(sourceCard);
            DrawOutputArrow(targetCard);
        }

        private void DrawInputArrow(CardData sourceCard)
        {
            Vector3 position = MyGameCard.transform.position;
            Vector3 start = ((!(sourceCard != null)) ? (MyGameCard.transform.position + directionVector * 0.5f) : TransformToEdge(sourceCard.transform.position, position, sourceCard.MyGameCard, -1f));
            position = TransformToEdge(start, position, MyGameCard, 1f);
            DrawManager.instance.DrawShape(new ConveyorArrow
            {
                Start = start,
                End = position
            });
        }

        private void DrawOutputArrow(CardData targetCard)
        {
            Vector3 position = MyGameCard.transform.position;
            Vector3 end = ((!(targetCard != null)) ? (MyGameCard.transform.position - directionVector * 0.5f) : TransformToEdge(position, targetCard.transform.position, targetCard.MyGameCard, 1f));
            position = TransformToEdge(position, end, MyGameCard, -1f);
            DrawManager.instance.DrawShape(new ConveyorArrow
            {
                Start = position,
                End = end
            });
        }

        private Vector3 TransformToEdge(Vector3 start, Vector3 end, GameCard card, float dir)
        {
            Vector2 start2 = new Vector2(start.x, start.z);
            Vector2 end2 = new Vector2(end.x, end.z);
            Vector2 pointOnCardEdge = GetPointOnCardEdge(start2, end2, card);
            return new Vector3(pointOnCardEdge.x, 0f, pointOnCardEdge.y) + (start - end).normalized * ExtraSideDistance * dir;
        }

        private Vector2 GetPointOnCardEdge(Vector2 start, Vector2 end, GameCard card)
        {
            Bounds bounds = card.GetBounds();
            corners[0] = new Vector2(bounds.min.x, bounds.min.z);
            corners[1] = new Vector2(bounds.max.x, bounds.min.z);
            corners[2] = new Vector2(bounds.max.x, bounds.max.z);
            corners[3] = new Vector2(bounds.min.x, bounds.max.z);
            for (int i = 0; i < 4; i++)
            {
                Vector2 p = corners[i];
                Vector2 p2 = corners[(i + 1) % 4];
                if (MathHelper.LineSegmentsIntersection(start, end, p, p2, out var intersection, out var _))
                {
                    return intersection;
                }
            }
            return start;
        }

        /*
        * Always returns the root-card of a stack.
        * Currently, all GameCard types are supported.
        */
        private CardData GetSourceCard(bool allowDraggingCards)
        {
            return WorldManager.instance.GetBestCardInDirection(MyGameCard, directionVector, allowDraggingCards, (GameCard card) => true)?.GetRootCard().CardData;
        }

        /*
        * Always returns the root-card of a stack.
        * Currently, all GameCard types are supported.
        */
        private CardData GetTargetCard(bool allowDraggingCards)
        {
            return WorldManager.instance.GetBestCardInDirection(MyGameCard, -directionVector, allowDraggingCards, (GameCard card) => true)?.GetRootCard().CardData;
        }


        private Vector3 directionVector
        {
            get
            {
                if (Direction == 0)
                {
                    return Vector3.back;
                }
                if (Direction == 1)
                {
                    return Vector3.left;
                }
                if (Direction == 2)
                {
                    return Vector3.forward;
                }
                if (Direction == 3)
                {
                    return Vector3.right;
                }
                return Vector3.back;
            }
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard is Inserter)
            {
                return true;
            }
            return false;
        }
        
    }
}