#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using DBDef;
using System.Collections.Generic;
using Thea2.Common;
using Thea2.Server;
using TheHoney;
using UnityEngine;
using System.Linq;

namespace GameScript
{
    public class CardGameScripts : ScriptBase
	{
        // AI 2.0

        #region Core planning
        static public CardAIPlanData ProducePlan(CardAI ai, CardAIPlanData data, bool allowTwoStages)
        {
            if (ai.intensity < 1)
            {
                Debug.LogError("[ERROR]0 level for AI will not produce any plans!, Increase calculation intensity to minimum 1!");
                return new CardAIPlanData();
            }

            NetBattlefield bf = data.bf;
            int apNextTurn = bf.APNextTurn(ai.playerID);
            int leftAP = bf.LeftAPThisTurn(ai.playerID) + apNextTurn;
            float value = bf.GetValueByCloneSimulation(ai.iterations, ai.r);

            List<NetCard> cards = CardsWithinAPRange(ai, bf, ai.totalAP);
            if (cards == null) return new CardAIPlanData();

            List<NetSkill> supportSkills2 = new List<NetSkill>();
            List<NetSkill> skills = SelectSkillsToCast(cards);

            if (skills == null || skills.Count == 0)
            {
                Debug.LogWarning("[!! AI] Avaliable skills to cast = 0");
                return new CardAIPlanData();
            }
            List<NetSkill> skills2 = null;
            List<int> importantPos = null;

            #region detect potential two-cast suggestions
            if (allowTwoStages)
            {
                List<NetCard> cards2 = CardsWithinAPRange(ai, bf, ai.totalAP - 1);
                if (cards2 != null)
                {
                    skills2 = SelectSkillsToCast(cards2);
                    //two cast simulation would be considered only in case of at least two skills
                    //being of value for that activity
                    if (skills2 != null && skills2.Count > 1)
                    {
                        supportSkills2 = WillDoTwoCastSimulation(ai, data, skills2);
                        if (supportSkills2.Count > 0)
                        {
                            importantPos = FindImportantPositions(ai, data, supportSkills2);
                        }

                        //expensive casts which exhausts available AP will be casted as single-casts
                        //support skills will be casted as single-casts
                        //cheap enough non-supports will be BASE for multi-casts
                        //then supports skills will be casted on top
                        skills2 = skills2.Except(supportSkills2).ToList();
                        skills = skills.Except(skills2).ToList();
                    }
                }
            }
            #endregion

            #region produce single-cast results            

            List<CardAIPlanData> plans = ProduceCastPlans(ai, data, skills, 1);
            if (plans != null && plans.Count > 0)
            {
                plans.Sort(PlanSorter);
                plans = ProduceRefinePlans(ai, data, plans);
            }

            #endregion
            #region produce two-cast results. 
            //TODO start by doing first layer of casts, with preferences if possible            

            if (skills2 != null && skills2.Count > 0)
            {                
                List<CardAIPlanData> plans2 = ProduceCastPlans(ai, data, skills2, 1);
                if (plans2 != null && plans2.Count > 0)
                {
                    plans2.Sort(PlanSorter);
                    plans2 = ProduceRefinePlans(ai, data, plans2);
                    plans.AddRange(plans2);
                }

                //if supports are possible validate do another single turn pass for remaining skills.
                if (supportSkills2 != null && supportSkills2.Count > 0)
                {
                    //register important positions if any to ensure they are preferred during position planning.
                    ai.preferredStrategicPlaces = importantPos;

                    //First Cast
                    plans2 = ProduceCastPlans(ai, data, skills2, 1);
                    if (plans2 != null && plans2.Count > 0)
                    {
                        plans2.Sort(PlanSorter);
                        plans2 = ProduceRefinePlans(ai, data, plans2);

                        //Second Cast (aka support cast)
                        //Use first plan and select boosts which seems to be most efficient among available options.
                        //this does one base cast and then one boost for each which account for: AxB 
                        List<CardAIPlanData> supportPlans = new List<CardAIPlanData>();
                        if (plans2 != null && plans2.Count > 0)
                        {
                            for (int i=0; i<ai.supportCasts && i<plans2.Count; i++)
                            {
                                CardAIPlanData plan = plans2[i];
                                supportPlans.AddRange(ProduceCastPlans(ai, plan, supportSkills2, 1));
                            }
                        }
                        
                        // select the best result of AxB to find the best theoretical result after two casts
                        //do deeper planning for the base positioning and then cast support skills on the chosen plan to
                        //acquire actual final value 
                        supportPlans.Sort(PlanSorter);
                        
                        if (supportPlans.Count>0)
                        {
                            CardAIPlanData cpd = supportPlans[0];
                            CardAIPlanData planBase = plans2.Find(o => o.firstPlay.skill == cpd.firstPlay.skill);                            

                            CardAIPlanData result = ProduceSingleRefination(ai, data, planBase);
                            if (result.valid)
                            {
                                List<CardAIPlanData> results =  ProduceCastPlans(ai, result, supportSkills2, 1);
                                if (results != null && results.Count > 0)
                                {
                                    results.Sort(PlanSorter);
                                    results = ProduceRefinePlans(ai, data, results);

                                    plans.AddRange(results);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region compare all results and select the best
            // all results are compared based on how much AP they use.
            // positive gain produced by all results are based on final result and final AP cost
            // GainedAdvantage / (2 + AP used) which results in:
            // 1/2 multiplier for 1 AP actions
            // 1/3 multiplier for 2 AP actions
            // 1/4 multiplier for 3+ AP actions
            // Single-casts would be considered as if they would use-up AvaliableAP amount
            // to ensure we do not consider them better even though they cannot provide any better results
            //if two-cast actions are available to some actions
            // aka: there is no point in having cheaper cost if AP cannot be later used.
            
            if (plans.Count == 0)
            {
                Debug.LogWarning("[!! AI] Available plans = 0");
                return new CardAIPlanData();
            }


            CardAIPlanData selected = plans[0];
            float selectedV = float.MinValue;
            
            for (int i = 0; i < plans.Count; i++)
            {
                CardAIPlanData cpd = plans[i];
                int newLeftAP = cpd.bf.LeftAPThisTurn(ai.playerID) + apNextTurn;
                int apUsed = leftAP - newLeftAP;
                float v = cpd.value - value;
                v = v / (1f + apUsed);
                if (selectedV < v)
                {
                    selected = plans[i];
                    selectedV = v;
                }
            }
            #endregion

            return selected;
        }
        #endregion
        #region Important position - Tactic planning
        //this section will try to find positions which may endup important with the secondary cast
        //eg splashing support skills.
        //TODO: ??
        //    It would also find indescribable positions
        //    eg current opponent attack splash positions.
        //NOTE: if no important positions are found only single casts would be considered to safe 
        //computation power
        static List<int> FindImportantPositions(CardAI ai, 
                                            CardAIPlanData data, 
                                            List<NetSkill> consideredSkills)
        {
            List<int> importantPositions = null;
            //if there is no skills which can be used there is no activity to consider
            if (consideredSkills == null || consideredSkills.Count <1)
            {                
                return importantPositions;
            }

            //if there is no allied cards on BF then simply skip this stage
            if (data.bf.GetOwnLivingCardsBFPositions(ai.playerID).Count < 1)
            {
                return importantPositions;
            }

            //consider all friend targeting skills, 
            //if they splash get their possible current targets
            //and use them to produce splash positions
            foreach (var v in consideredSkills)
            {
                if (v.IsCastingSpell() && v.IsSplashng())
                {
                    Script s = v.GetSubSkill().targets.script2;
                    List<int> pos = v.FindValidTargets(data.bf, -1);
                    if(pos != null)
                    {
                        if (pos.FindIndex(o => data.bf.IsBattleSlot(o) && 
                                          !data.bf.IsSlotFree(o) && 
                                          data.bf.IsSameSideSlot(ai.playerID, o)) == -1)
                        {
                            //this skill does not have allied targets
                            continue;
                        }

                        List<int> sec = v.FindSecondaryTargets(data.bf, -1, pos);
                        if (sec == null) continue;

                        if (importantPositions == null) importantPositions = new List<int>();
                        importantPositions.Union(sec);
                    }
                }
            }
            return importantPositions;
        }

        static List<NetSkill> WillDoTwoCastSimulation(CardAI ai,
                                            CardAIPlanData data,
                                            List<NetSkill> consideredSkills )
        {
            List<NetSkill> supportSkills = new List<NetSkill>();

            //if there is no skills which can be used there is no activity to consider
            if (consideredSkills == null || consideredSkills.Count < 1)
            {
                return supportSkills;
            }

            NetBattlefield bf = null;            
            foreach(var v in consideredSkills)
            {
                if (v.IsCastingSpell())
                {
                    if (bf == null) bf = ProduceFakeBFIfNeeded(ai.playerID, data.bf);

                    List<int> pos = v.FindValidTargets(bf, -1);
                    if (pos != null)
                    {
                        if (pos.FindIndex(o => bf.IsBattleSlot(o) &&
                                          !bf.IsSlotFree(o) &&
                                          bf.IsSameSideSlot(ai.playerID, o)) == -1)
                        {
                            //this skill does not have allied targets
                            continue;
                        }

                        supportSkills.Add(v);
                    }
                }
            }

            return supportSkills;
        }
        static NetBattlefield ProduceFakeBFIfNeeded(int playerID, NetBattlefield bf)
        {
            //fake battlefield will contain all friendly cards currently not on battlefield
            //positioned on free positions so that AI can see if it can produce
            //any targets. there is no simulation involved so fake state should not harm "thinking"

            //we will not need fake BF IF all owned cards are already on BF.
            HashSet<NetCard> ownCastedCards = bf.GetCardsOnBattelfield(playerID);
            NetCardPlayerData ncd = bf.GetPlayerByPlayerID(playerID);
            if (ncd == null || NetType.IsNullOrEmpty(ncd.HandCards)) return bf;

            List<int> ownCardsAtHand = ncd.HandCards.value;
            List<NetCard> cardsToCast = new List<NetCard>();
            //this is different set than just cards which were used, because card which casts summons or
            //does support will not be present on battlefield and still cost more than base AP
            foreach(var v in ownCardsAtHand)
            {
                NetCard nc = bf.GetCardByID(v);
                if(!ownCastedCards.Contains(nc))
                {
                    cardsToCast.Add(nc);
                }
            }

            if (cardsToCast.Count == 0) return bf;

            //there are cards which should be represented on battle positions
            //which requires new BF            
            //we would do simple but fake casts
            NetBattlefield bf2 = bf.CloneAndRemember();
            int halfSize = bf2.BattlefieldSize / 2;
            int start = playerID == 0 ? 0 : halfSize;

            int cardListIndex = 0;

            for(int i = 0; i < halfSize; i++ )
            {
                if (cardListIndex == cardsToCast.Count) break;
                int index = i + start;
                if (bf2.BattlefieldPositions.value[index] == 0)
                {
                    bf2.BattlefieldPositions.value[index] = cardsToCast[cardListIndex].CardID;
                }
            }

            return bf2;
        }
        #endregion

        #region Single-Cast Planning
        static public List<CardAIPlanData> ProduceCastPlans(CardAI ai, CardAIPlanData data, List<NetSkill> skills, int count)
        {
            List<CardAIPlanData> plans = new List<CardAIPlanData>(skills.Count);
            NetBattlefield bf = data.bf;
            
            for (int i = 0; i < skills.Count; i++)
            {
                NetSkill ns = skills[i];
                IEnumerator<List<int>> targetEnumerator = null;

                for (int k = 0; k < count; k++)
                {
                    CardAIPlanData d = CastSkillOnece(ai, ns, ns.GetOwner(bf), data, bf.CloneAndRemember(), targetEnumerator);
                    if (d.valid)
                    {
                        if (targetEnumerator == null) targetEnumerator = d.GetTopCardPlay().targetEnumerator;
                        plans.Add(d);
                    }
                }
            }

            return plans;
        }

        static int PlanSorter(CardAIPlanData a, CardAIPlanData b)
        {
            return -a.value.CompareTo(b.value);            
        }

        static List<CardAIPlanData> ProduceRefinePlans(CardAI ai, CardAIPlanData entryState, List<CardAIPlanData> plans)
        {
            NetBattlefield bf = entryState.bf;

            //get top plans and try to refine them
            int min = Mathf.Min(plans.Count, ai.refiningSize);            
            
            for (int i = 0; i < min; i++)
            {                
                for (int k = 0; k < ai.intensity; k++)
                {                                                            
                    CardAIPlanData d = ProduceSingleRefination(ai, entryState, plans[i]);
                    if (d.valid && plans[i].value < d.value )
                    {
                        d.SetEnumerator(plans[i].GetTopCardPlay().targetEnumerator);
                        plans[i] = d;
                    }
                }                
            }
            
            //select best plan per skill            
            return plans.GetRange(0, min);
        }

        static CardAIPlanData ProduceSingleRefination(CardAI ai, CardAIPlanData entryState, CardAIPlanData plan)
        {
            NetBattlefield bf = entryState.bf;
            NetSkill ns = bf.GetSkillByID(plan.GetTopCardPlay().skill);

            //cast form the source plan data so that it is not casted "after" action we want to reiterate to different targets
            CardAIPlanData d = CastSkillOnece(ai, ns, ns.GetOwner(bf), entryState, bf.CloneAndRemember(), plan.GetTopCardPlay().targetEnumerator);
            if (d.valid && plan.value < d.value)
            {
                d.SetEnumerator(plan.GetTopCardPlay().targetEnumerator);                
            }
            return d;
        }
        #endregion
        #region Multi-Cast Planning
        static List<CardAIPlanData> RefineMultiCastPlan(CardAI ai, CardAIPlanData entryState, List<CardAIPlanData> plans)
        {
            NetBattlefield bf = entryState.bf;

            //get top plans and try to refine them
            int min = Mathf.Min(plans.Count, ai.refiningSize);

            for (int i = 0; i < min; i++)
            {
                for (int k = 0; k < ai.intensity; k++)
                {
                    //prepare source for cast copying target enumerator so that it can be reused by the further casts
                    CardAIPlanData source = entryState;                    
                    NetSkill ns = bf.GetSkillByID(plans[i].GetTopCardPlay().skill);

                    //cast form the source plan data so that it is not casted "after" action we want to reiterate to different targets
                    CardAIPlanData d = CastSkillOnece(ai, ns, ns.GetOwner(bf), source, bf.CloneAndRemember(), plans[i].GetTopCardPlay().targetEnumerator);
                    if (d.valid && plans[i].value < d.value)
                    {
                        d.SetEnumerator(plans[i].GetTopCardPlay().targetEnumerator);
                        plans[i] = d;
                    }
                }
            }

            //select best plan per skill            
            return plans.GetRange(0, min);
        }
             

        #endregion
        // 
        //         static public CardAIPlanData _ProducePlan(int playerID, CardAIPlanData data, int avaliableAP, MHRandom r, int calculationIntensity = 1)
        //         {
        //             if (calculationIntensity < 1 )
        //             {
        //                 Debug.LogError("[ERROR]0 level for AI will nto produce any plans!, Increase calculation intensity to minimum 1!");
        //                 return new CardAIPlanData();
        //             }
        // 
        //             NetBattlefield bf = data.bf;
        // 
        //             List<NetCard> cards = CardsWithinAPRange(playerID, bf, avaliableAP);
        //             if (cards == null) return new CardAIPlanData();
        // 
        //             List<NetSkill> skills = SelectSkillsToCast(cards);
        //             if (skills == null || skills.Count == 0) return new CardAIPlanData();
        // 
        //             bool friendlyTurn = playerID > 0;
        // 
        //             //Do single cast of all skills to build some expectations
        //             CardAIPlanData[] plans = new CardAIPlanData[skills.Count];
        // 
        //             for (int i=0; i<skills.Count; i++)
        //             {
        //                 NetSkill ns = skills[i];
        //                 CardAIPlanData d = CastSkillOnece(friendlyTurn, ns, ns.GetOwner(bf), data, bf.CloneAndRemember(), r);
        //                 if (d.valid)
        //                 {
        //                     plans[i] = d;
        //                 }
        //             }
        // 
        //             //sort result based on their value
        //             List<CardAIPlanData> plansL = new List<CardAIPlanData>(plans);
        //             plansL.Sort(delegate (CardAIPlanData a, CardAIPlanData b)
        //             {
        //                 return a.value.CompareTo(b.value);
        //             });
        // 
        //             //get top plans and try to refine them before selecting the best
        //             int min = Mathf.Min(plansL.Count, calculationIntensity);
        //             plansL = plansL.GetRange(0, min);
        // 
        //             int refiningLevel = 8;
        //             plans = new CardAIPlanData[plansL.Count * refiningLevel];            
        //             for(int i=0; i < plansL.Count; i++)
        //             {
        //                 for(int k=0; k< refiningLevel; k++)
        //                 {
        //                     //plansL[i].validTargets
        //                 }
        //             }
        // 
        //             // Start secondary plans from the selected plans
        //             return new CardAIPlanData();
        //         }
        static public List<NetCard> CardsWithinAPRange(CardAI ai, NetBattlefield bf, int testedAPRange)
        {
            NetListInt ni = bf.GetPlayerByPlayerID(ai.playerID).HandCards;
            if (NetType.IsNullOrEmpty(ni))
            {
                return null;
            }
            List<NetCard> ncs = new List<NetCard>();
            foreach(var v in ni.value)
            {
                NetCard nc = bf.GetCardByID(v);
                if (nc.GetCastingCost() <= testedAPRange)
                {
                    ncs.Add(nc);
                }
            }
            return ncs;
        }
        static List<NetSkill> SelectSkillsToCast(List<NetCard> cards)
        {
            List<NetSkill> skills = new List<NetSkill>();

            foreach (NetCard c in cards)
            {
                skills.AddRange(c.GetBothRangesSkills());
                skills.AddRange(c.GetSpells());
            }

            return skills;
        }
        static CardAIPlanData CastSkillOnece(CardAI ai, NetSkill ns, NetCard nc, CardAIPlanData data, NetBattlefield bf, IEnumerator<List<int>> targetEnumerator)
        {
            //structure makes a new copy
            CardAIPlanData result = data;
            //using previous bf allows to utilize already cached data, as well as share caching with following casts.
            //later we will use new bf to ensure we do not override the same bf.
            NetBattlefield prevBf = data.bf;

            IList<int> targets = GetPlayCardTargets(ai.FriendlyTurn, ns, nc, prevBf);            
            if(targets == null || targets.Count < 1)
            {
                return new CardAIPlanData();
            }
                        
            if (targetEnumerator == null)
            {
                targetEnumerator = GetTargetEnumerator(ns, nc, targets, ai);
                if (targetEnumerator == null) return new CardAIPlanData();
            }
            
            targetEnumerator.MoveNext();
            List<int> selectedTargets = targetEnumerator.Current;
            if (selectedTargets== null || selectedTargets.Count == 0)
            {
                return new CardAIPlanData();
            }

            List<int> secTargets = GetSecondaryPlayTargets(ns, nc, prevBf, selectedTargets, ai.r);
            FInt skillDelay = ns.GetSkillDelay(prevBf);
            int cost = prevBf.GetCardCastingCostFast(ns.GetOwner(prevBf).CardID);
            //add cost if something need to be casted next turn by AI
            var ncp = bf.GetPlayerByPlayerID(ai.playerID);
            if (cost > ncp.ApLeft) skillDelay += 2;

            //operate from now on its own bf
            result.bf = bf;
            NetQueueItem q;
            if (ns.IsCastingSpell())
            {
                q = new NetQueueItem(bf, ns, new NetListInt(selectedTargets), new NetListInt(secTargets), skillDelay, -1);
            }
            else
            {
                q = new NetQueueItem(bf, ns, null, null, skillDelay, selectedTargets[0]);
            }
            

            
            //if ap cost were larger than current ap then we will result in negative ap.
            //because its just simulation we do not care for that now, as the sum of this and next turn ap 
            //for estimating cost would be identical
            ncp.ApLeft -= nc.GetCastingCost();

            bf.PlayCard(q, ai.r);
            float value = bf.GetValueByCloneSimulation(ai.iterations, ai.r);
            result.value = value;
            result.AddCardPlay(nc.CardID, ns.SkillInBattleID, q);
            result.SetEnumerator(targetEnumerator);
            result.valid = true;

            return result;
        }
        static IList<int> GetPlayCardTargets(bool friendlyTurn, NetSkill ns, NetCard nc, NetBattlefield bf)
        {
            Subskill ss = ns.GetSubSkill();
            if (ss.trigger.triggerGroup == ETriggerGroupType.DoAttack ||
                ss.trigger.triggerGroup == ETriggerGroupType.DoAlternateAttack)
            {
                if (ss.trigger.requiredToBeInFrontline)
                {
                    if (friendlyTurn)
                    {
                        return bf.GetFreeFriendlyMeleeSlots(false);
                    }
                    else
                    {
                        return bf.GetFreeEnemyMeleeSlots(false);
                    }
                }
                else if (ss.trigger.requiredToBeInBackline)
                {
                    if (friendlyTurn)
                    {
                        return bf.GetFreeFriendlyRangedSlots(false);
                    }
                    else
                    {
                        return bf.GetFreeEnemyRangedSlots(false);
                    }
                }
                else
                {
                    List<int> both;
                    if (friendlyTurn)
                    {
                        IList<int> a = bf.GetFreeFriendlyMeleeSlots(false);
                        IList<int> b = bf.GetFreeFriendlyRangedSlots(false);
                        both = new List<int>(a.Count + b.Count);
                        both.AddRange(a);
                        both.AddRange(b);
                        return both;
                    }
                    else
                    {
                        IList<int> a = bf.GetFreeEnemyMeleeSlots(false);
                        IList<int> b = bf.GetFreeEnemyRangedSlots(false);
                        both = new List<int>(a.Count + b.Count);
                        both.AddRange(a);
                        both.AddRange(b);
                        return both;
                    }
                }
            }
            else
            {
                return ns.FindValidTargets(bf, -1);
            }
        }

        static IEnumerator<List<int>> GetTargetEnumerator(NetSkill ns, NetCard nc, IList<int> potentialTargets, CardAI ai)
        {
            if (potentialTargets.Count < 1) return null;

            Subskill ss = ns.GetSubSkill();
            int count = 0;
            if (ss.trigger.triggerGroup == ETriggerGroupType.DoAttack ||
                ss.trigger.triggerGroup == ETriggerGroupType.DoAlternateAttack)
            {
                count = 1;
            }
            else
            {
                if (potentialTargets.Count < ss.targets.targetCountRange.minimumCount )
                {
                    return null;
                }
                if (potentialTargets.Count <= ss.targets.targetCountRange.maximumCount)
                {
                    count = potentialTargets.Count;
                }
                else
                {
                    count = ss.targets.targetCountRange.maximumCount;
                }
            }

            List<int> pt = new List<int>(potentialTargets);
            pt.Sort(delegate (int a, int b)
            {
                if(ai.preferredStrategicPlaces != null)
                {
                    bool stratA = ai.preferredStrategicPlaces.Contains(a);
                    bool stratB = ai.preferredStrategicPlaces.Contains(b);
                    
                    if(stratA != stratB)
                    {
                        return stratA ? -1 : 1;
                    }
                    //if they are the same use regular random
                }

                return ai.r.GetInt(0, 2) - 1;
            });

            var lu = new ListUtils(pt, count);

            return lu.GetEnumerator();
        }
        static List<int> GetSecondaryPlayTargets(NetSkill ns, NetCard nc, NetBattlefield bf, List<int> primaryTargets, MHRandom r)
        {
            if (primaryTargets == null ||
                 primaryTargets.Count < 1) return null;

            Subskill ss = ns.GetSubSkill();
            if (ss.trigger.triggerGroup == ETriggerGroupType.DoAttack ||
                ss.trigger.triggerGroup == ETriggerGroupType.DoAlternateAttack)
            {
                //there is no secondary targets of playing card to the battelfield unless its a spell
                return null;
            }

            if (ss.targets.script2 == null ||
                string.IsNullOrEmpty( ss.targets.script2.scriptName))
            {
                return null;
            }

            return ns.FindSecondaryTargets(bf, -1, primaryTargets);
        }

        // AI 1.4 
        /*
        static public void ProducePlan(AICardPlannerData data)
        {
            NetBattlefield bf = data.parentNode.GetBFForNewPlan();
            
            bf.EnsureCardsAreCached();

            #region Check if there are AP available to do any planning at this cost
            bool enemyTurn = bf.IsEnemyTurn();

            if (enemyTurn)
            {
                if (bf.GetPlayerByOrder(0).ApLeft < data.castingCost)
                {
                    data.NotAtWork();
                    return;
                }
            }
            else
            {
                bool possible = false;
                for (int i = 1; i < bf.Players.value.Count; i++)
                {
                    if (bf.GetPlayerByOrder(i).ApLeft >= data.castingCost)
                    {
                        possible = true;
                    }
                }

                if (!possible)
                {
                    data.NotAtWork();
                    return;
                }
            }
            #endregion

            #region Select Interesting Cards
            List<NetCard> interestingCards = new List<NetCard>();
            if (enemyTurn)
            {
                List<NetCard> hand = new List<NetCard>(bf.GetPlayerByOrder(0).HandCards.value.Count);
                int ap = bf.GetPlayerByOrder(0).ApLeft;

                foreach (int k in bf.GetPlayerByOrder(0).HandCards.value)
                {
                    NetCard nc = bf.GetCardByID(k);
                    if (nc.GetCastingCost() <= ap)
                    {
                        hand.Add(nc);
                    }
                }
                
                interestingCards = AIPickInterestingCards(hand, bf) ;
            }
            else
            {
                List<NetCard> hand = new List<NetCard>();
                for (int i = 1; i < bf.Players.value.Count; i++)
                {
                    int ap = bf.GetPlayerByOrder(i).ApLeft;

                    foreach (int k in bf.GetPlayerByOrder(i).HandCards.value)
                    {
                        NetCard nc = bf.GetCardByID(k);
                        if (nc.GetCastingCost() <= ap)
                        {
                            hand.Add(nc);
                        }
                    }
                }

                //static public List<NetCard> AIPickInterestingCards(IEnumerable<NetCard> cards, NetBattlefield bf)
                interestingCards = AIPickInterestingCards(hand, bf);
            }
            #endregion

            if (interestingCards == null)
            {
                data.NotAtWork();
                return;
            }

            List<NetCard> interestingCardsToProcess = new List<NetCard>();

            #region Prepare Nodes in toProcess from possible actions                
            //Pay for the skill        
            foreach (var v in interestingCards)
            {
                int cost = v.GetCastingCost();
                int apLeft = bf.GetPlayerByPlayerID(v.PlayerOwnerID).ApLeft;
                if (apLeft >= cost)
                {
                    if (data.castingCost == cost)
                    {
                        interestingCardsToProcess.Add(v);

                    }
                    else if (data.castingCost < cost)
                    {
                        data.FuturePlansPossible();
                    }
                }
            }
            #endregion

            #region Play Selected Skills From toProces Nodes
            List<NetSkill> skills = SelectSkillsToCast(interestingCardsToProcess);

            object objCastingTree = SimulateCastingResults(bf, skills, data.random);
            if (objCastingTree != null)
            {
                AIPlan plan = new AIPlan(objCastingTree as Dictionary<NetBattlefield, NetQueueItem>, data.parentNode);
                //plan value is based on a battelfield state after resolving all recorded actions
                //whole list of actions is resolved X times     (X == data.stackIterations)
                // (L)Lowest and (A)Average results are used to calculate:
                // Value = (L + A) /2
                //that way potential huge pit hole in the tactic will lower its total value but still 
                //average value of all options would be used to fish for less common and less likely advantages
                plan.CalculateBFValue(data.random, data.stackIterations, false);
                data.RecordPlan(plan.GetWholePlan());
            }
            #endregion

            data.NotAtWork();
        }

        /// <summary>
        /// Filter out all that are not interesting to be casted (eg duplicates)
        /// in the future we may decide to add reason to discard cards which are more expensive than available AP
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="bf"></param>
        /// <returns></returns>
        static public List<NetCard> AIPickInterestingCards(IEnumerable<NetCard> cards, NetBattlefield bf)
        {
            if (cards == null) return null;

            List<NetCard> allCards = new List<NetCard>(cards);

            if (allCards.Count < 1) return null;
            if (allCards.Count <= 4) return allCards;

            allCards.Sort(delegate (NetCard a, NetCard b)
            {
                FInt ca = a.GetExpectedPower() / (a.UseCount + 1);
                FInt cb = b.GetExpectedPower() / (b.UseCount + 1);

                int c = -ca.CompareTo(cb);
                return c;
            });
            
            //AI will consider at each step only subset of the cards.
            // - two with the largest Power divided by AP cost 
            // - one Costing least, with the largest Power
            //       (if there is at least one card of cost 1 it will select most powerful card costing 1)
            // - one Costing least, with the smallest Power
            //       (if there is at least one card of cost 1 it will select least powerful card costing 1)

            List<NetCard> choices = new List<NetCard>();
            
            //take top two most powerful cards
            choices.Add(allCards[0]);
            allCards.RemoveAt(0);
            choices.Add(allCards[0]);
            allCards.RemoveAt(0);

            // cheapest - most power
            NetCard selected = allCards[0];
            for (int i=1; i<allCards.Count; i++)
            {
                if(selected.apCost > allCards[i].apCost)
                {
                    selected = allCards[i];
                }
                else if (selected.apCost == allCards[i].apCost &&
                    selected.GetExpectedPower() < allCards[i].GetExpectedPower())
                {
                    selected = allCards[i];
                }
            }

            choices.Add(selected);
            allCards.Remove(selected);

            // cheapest - least power
            selected = allCards[0];
            for (int i = 1; i < allCards.Count; i++)
            {
                if (selected.apCost > allCards[i].apCost)
                {
                    selected = allCards[i];
                }
                else if (selected.apCost == allCards[i].apCost &&
                    selected.GetExpectedPower() > allCards[i].GetExpectedPower())
                {
                    selected = allCards[i];
                }
            }

            choices.Add(selected);
            allCards.Remove(selected);

            
            return choices;
        }

        

        static Dictionary<NetBattlefield, NetQueueItem> SimulateCastingResults(NetBattlefield bf, List<NetSkill> skills, MHRandom random)
        {
            Dictionary<NetBattlefield, NetQueueItem> castOptions =
                new Dictionary<NetBattlefield, NetQueueItem>();

            //consider only one slot melee and ranged 
            //among columns which are free at the moment.


            //   NetCardPlayerData ncp = null;

            foreach (var ns in skills)
            {
                NetCard nc = ns.GetOwner(bf);

                //FInt castingDelay = ns.GetSkillDelay(bf);

                //             if (ncp == null)
                //             {
                //                 ncp = bf.GetPlayerByPlayerID(nc.playerOwnerID);
                //             }            

                if (!ns.IsCastingSpell())
                {
                    for (int i = 0; i < bf.BattlefieldSize; i++)
                    {
                        int index = i % bf.BattlefieldSize;

                        if (ns.IsMeleeType())
                        {
                            int idx = bf.GetMeleeSlot(index, ns);

                            if (bf.GetCardFromBattleSlot(idx) == null)
                            {
                                NetBattlefield bfc = bf.CloneAndRemember();
                                NetCardPlayerData cardOwner = bfc.GetPlayerByPlayerID(nc.PlayerOwnerID);
                                cardOwner.ApLeft -= nc.GetCastingCost();
                                castOptions[bfc] = bfc.PlayCard(ns, bf.CurentTurn + ns.GetSkillDelay(nc), idx, random);
                            }
                        }

                        if (ns.IsRangedType())
                        {
                            int idx = bf.GetRangedSlot(index, ns);

                            if (bf.GetCardFromBattleSlot(idx) == null)
                            {
                                NetBattlefield bfc = bf.CloneAndRemember();
                                NetCardPlayerData cardOwner = bfc.GetPlayerByPlayerID(nc.PlayerOwnerID);
                                cardOwner.ApLeft -= nc.GetCastingCost();
                                castOptions[bfc] = bfc.PlayCard(ns, bf.CurentTurn + ns.GetSkillDelay(nc), idx, random);
                            }
                        }
                    }
                }
                else
                {
                    DBDef.Subskill ss = ns.GetSubSkill();
                    List<int> targets = ns.FindValidTargets(bf, -1);

                    //Select valid number of targets
                    if (targets == null ||
                        targets.Count < ss.targets.targetCountRange.minimumCount)
                    {
                        //impossible to cast
                        continue;
                    }

                    NetBattlefield bfc = bf.CloneAndRemember();
                    NetCardPlayerData cardOwner = bfc.GetPlayerByPlayerID(nc.PlayerOwnerID);
                    cardOwner.ApLeft -= nc.GetCastingCost();
                    castOptions[bfc] = bfc.PlayCard(ns, bf.CurentTurn + ns.GetSkillDelay(nc), -1, random);

                    if (ns.IsInstantSpell())
                    {
                        bfc.ResolveLocalStack(random);
                    }
                }
            }

            return castOptions;
        }

        */
    }
}
#endif