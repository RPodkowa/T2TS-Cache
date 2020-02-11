#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using DBDef;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Thea2.Client;
using Thea2.Common;
using TheHoney;
using UnityEngine;
using UnityEngine.UI;
using Vectrosity;

namespace GameScript
{
    public class ResearchScreenScripts : ScriptBase
    {
        
        static public void InitializeResearchPanel(
                            DBClass typeBypass, 
                            GameObject source,
                            Dictionary<DBClass, GameObject> unlockToGO,
                            Advancement screen,
                            string addFilter)
        {
            try
            {
                Type t = typeBypass.GetType();
                if (t == typeof(Resource))
                {
                    var r = Globals.GetTypeFromDBAsDBClass<Resource>();
                    ResearchPanelConstruction(source, unlockToGO, r, screen, addFilter, 1f, 1f);
                }
                else if (t == typeof(ItemTech))
                {
                    var r = Globals.GetTypeFromDBAsDBClass<ItemTech>();
                    ResearchPanelConstruction(source, unlockToGO, r, screen, addFilter, 2f, 1.1f);
                }
                else if (t == typeof(BuildingTech))
                {
                    var r = Globals.GetTypeFromDBAsDBClass<BuildingTech>();
                    ResearchPanelConstruction(source, unlockToGO, r, screen, addFilter, 1f, .8f);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[ERROR]\n"+e);
            }
        }
        static void ResearchPanelConstruction(GameObject source, 
                            Dictionary<DBClass, GameObject> unlockToGO,
                            List<DBClass> sources,
                            Advancement screen,
                            string addFilter,
                            float radiusMultiplier,
                            float horizontalSpreadMultiplier)
        {
            int circleRadius = 160;
            float spreadAngleDistance = 360 / 20f;

            var r = sources;
             
            
            if (addFilter == "food items")
            {
                r = r.FindAll(o => FilterCooking(o));
            }
            else if (addFilter == "nonfood items")
            {
                r = r.FindAll(o => FilterNonCooking(o));
            }

            List<List<DBClass>> structure = new List<List<DBClass>>();
            HashSet<DBClass> listed = new HashSet<DBClass>();

            Dictionary<DBClass, Vector3> resourcePositions = new Dictionary<DBClass, Vector3>();

            float layoutRadius = circleRadius * radiusMultiplier;
            float spreadDistance = spreadAngleDistance / horizontalSpreadMultiplier;
            int k = 0;
            while (listed.Count < r.Count)
            {
                k++;
                if (k > 10) break;
                List<DBClass> layer = new List<DBClass>();
                structure.Add(layer);
                if (!SortingSources(r, listed, layer))
                {
                    break;
                }
            }

            for (var l = 0; l < structure.Count; l++)
            {
                if (l == 0)
                {
                    float count = structure[l].Count;
                    for (int i = 0; i < count; i++)
                    {
                        Quaternion q = Quaternion.Euler(0, 0, 360 * i / count);

                        Vector3 dir = q * (new Vector3(0, 1, 0) * layoutRadius);

                        DBClass res = structure[l][i];
                        GameObject s = GameObjectUtils.Instantiate(source, source.transform.parent);
                        GameObject go = screen.PopulateNode(s, res);
                        go.transform.localPosition = dir;
                        resourcePositions[res] = dir;
                        unlockToGO[res] = go;

                        VectorLine vl = screen.DrawLink(go, go.transform.parent.gameObject);

                        screen.RegisterLine(res, null, vl);

                        DBClass resRef = Globals.GetInstanceFromDB(go.name);
                        go.GetComponent<Button>().onClick.AddListener(delegate ()
                        {
                            if (resRef is Resource) screen.Click(resRef as Resource);
                            else if (resRef is ItemTech) screen.Click(resRef as ItemTech);
                            else if (resRef is BuildingTech) screen.Click(resRef as BuildingTech);
                        });

                        Image icon = GameObjectUtils.FindByNameGetComponent<Image>(go, "Bg");
                        icon.alphaHitTestMinimumThreshold = 0.5f;

                        var st = GameObjectUtils.GetOrAddComponent<RolloverSimpleTooltip>(icon.gameObject);
                        if (resRef is Resource)
                        {
                            var stRes = resRef as Resource;
                            st.title = stRes.descriptionInfo.name;
                            st.imageName = stRes.descriptionInfo.iconName;
                            st.description = stRes.descriptionInfo.description;
                        }
                        else if (resRef is ItemTech)
                        {
                            var stIT = resRef as ItemTech;
                            st.title = stIT.descriptionInfo.name;
                            st.imageName = stIT.descriptionInfo.iconName;
                            st.description = stIT.descriptionInfo.description;
                        }
                        else if (resRef is BuildingTech)
                        {
                            var stBT = resRef as BuildingTech;
                            st.title = stBT.descriptionInfo.name;
                            st.imageName = stBT.descriptionInfo.iconName;
                            st.description = stBT.descriptionInfo.description;
                        }
                    }
                }
                else
                {
                    List<DBClass> layer = structure[l];
                    while (layer.Count > 0)
                    {
                        DBClass res = layer[0];
                        if (GetParents(res) == null) break;

                        List<DBClass> group = layer.FindAll(o => GetParents(o) != null &&
                                                        (o == res ||
                                                            (
                                                                GetParents(o).All(GetParents(res).Contains) &&
                                                                GetParents(res).All(GetParents(o).Contains)
                                                            )
                                                        )
                                                     );
                        layer = layer.FindAll(o => !group.Contains(o));

                        Vector3 cumulativeOffset = Vector3.zero;
                        int count = 0;
                        foreach (var v in GetParents(res))
                        {
                            cumulativeOffset += resourcePositions[v];
                            count++;
                        }

                        Vector3 groupDir = (cumulativeOffset / count).normalized;

                        for (int i = 0; i < group.Count; i++)
                        {
                            res = group[i];

                            float steps = i - (group.Count - 1) * 0.5f;
                            Quaternion q = Quaternion.Euler(0, 0, steps * spreadDistance);
                            Vector3 dir = q * groupDir;

                            //First radius is multiplied based on the extra settings to ensure this distance is set on screen basis
                            //Second and further are spread by default distance
                            dir = dir.normalized * layoutRadius +
                                  dir.normalized * circleRadius * l;

                            GameObject s = GameObjectUtils.Instantiate(source, source.transform.parent);
                            GameObject go = screen.PopulateNode(s, res);
                            go.transform.localPosition = dir;
                            resourcePositions[res] = dir;
                            unlockToGO[res] = go;

                            foreach (var v in GetParents(res))
                            {
                                VectorLine vl = screen.DrawLink(unlockToGO[res], unlockToGO[v]);
                                screen.RegisterLine(v, res, vl);
                            }

                            DBClass resRef = Globals.GetInstanceFromDB(go.name);
                            go.GetComponent<Button>().onClick.AddListener(delegate ()
                            {
                                if (resRef is Resource) screen.Click(resRef as Resource);
                                else if (resRef is ItemTech) screen.Click(resRef as ItemTech);
                                else if (resRef is BuildingTech) screen.Click(resRef as BuildingTech);
                            });

                            Image icon = GameObjectUtils.FindByNameGetComponent<Image>(go, "Bg");
                            icon.alphaHitTestMinimumThreshold = 0.5f;

                            var st = GameObjectUtils.GetOrAddComponent<RolloverSimpleTooltip>(icon.gameObject);
                            if (resRef is Resource)
                            {
                                var stRes = resRef as Resource;
                                st.title = stRes.descriptionInfo.name;
                                st.imageName = stRes.descriptionInfo.iconName;
                                st.description = stRes.descriptionInfo.description;
                            }
                            else if (resRef is ItemTech)
                            {
                                var stIT = resRef as ItemTech;
                                st.title = stIT.descriptionInfo.name;
                                st.imageName = stIT.descriptionInfo.iconName;
                                st.description = stIT.descriptionInfo.description;
                            }
                            else if (resRef is BuildingTech)
                            {
                                var stBT = resRef as BuildingTech;
                                st.title = stBT.descriptionInfo.name;
                                st.imageName = stBT.descriptionInfo.iconName;
                                st.description = stBT.descriptionInfo.description;
                            }
                        }
                    }
                }
            }

            GameObject.Destroy(source);
        }
        static bool SortingSources(IEnumerable listOfType, HashSet<DBClass> listed, List<DBClass> layer)
        {
            foreach (var v in listOfType)
            {
                DBClass instance = v as DBClass;
                //do not add resources which are not to be displayed
                if (GetCost(instance) < 0)
                {
                    listed.Add(instance);
                    continue;
                }

                if (!listed.Contains(instance))
                {
                    bool add = true;
                    if (GetParents(instance) != null)
                    {
                        foreach (var p in GetParents(instance))
                        {
                            if (!listed.Contains(p) ||
                                layer.Contains(p))
                            {
                                add = false;
                                break;
                            }
                        }
                    }

                    if (add)
                    {
                        layer.Add(instance);
                        listed.Add(instance);
                    }
                }
            }
            if (layer.Count == 0)
            {
                foreach (var v in listOfType)
                {
                    DBClass instance = v as DBClass;
                    if (!listed.Contains(instance))
                    {
                        Debug.LogError("[ERROR]Resource " + instance.dbName + " should be able to be researched, but its parent(-s) is not!");
                    }
                }
                return false;
            }
            return true;
        }
        static bool FilterCooking(DBClass item)
        {
            if (item is ItemTech) return (item as ItemTech).dbName.StartsWith("ITEM_TECH-COOKING");
            return false;
        }
        static bool FilterNonCooking(DBClass item)
        {
            if (item is ItemTech) return !(item as ItemTech).dbName.StartsWith("ITEM_TECH-COOKING");
            return false;
        }
        static int GetCost(DBClass source)
        {
            if (source is Resource)
            {
                return (source as Resource).researchCost;
            }

            if (source is ItemTech)
            {
                return (source as ItemTech).upgradeCost;
            }

            if (source is BuildingTech)
            {
                return (source as BuildingTech).upgradeCost;
            }

            return 99;
        }
        static DBClass[] GetParents(DBClass source)
        {
            if (source is Resource)
            {
                return (source as Resource).parents;
            }

            if (source is ItemTech)
            {
                return (source as ItemTech).parentsItemTech;
            }

            if (source is BuildingTech)
            {
                return (source as BuildingTech).parentsBuildingTech;
            }

            return null;
        }
        static void RegisterLine(DBClass r, DBClass r2, VectorLine v, object registratorDictionary)
        {
            var connectingLines = registratorDictionary as Dictionary<VectorLine, Multitype<DBClass, DBClass>>;
            connectingLines[v] = new Multitype<DBClass, DBClass>(r, r2);
        }
        static void Click(DBClass r, NOUnlocks unlocksData)
        {
            DBClass[] parents = GetParents(r);
            int researchCost = GetCost(r);
            string unlockName = r.dbName;
            string unlockInfo = "";

            bool allowUnlock = true;
            if (unlocksData.unlocks == null || unlocksData.unlocks.Contains(unlockName))
            {
                allowUnlock = false;
                unlockInfo = "UI_ALREADY_UNLOCKED";
            }
            else if (parents != null)
            {
                allowUnlock = false;

                foreach (var v in parents)
                {
                    if (unlocksData.unlocks != null &&
                        unlocksData.unlocks.Contains(v.dbName))
                    {
                        allowUnlock = true;
                        break;
                    }
                }
                if (!allowUnlock)
                {
                    unlockInfo = "UI_PARENT_UNLOCK_REQUIRED";
                }
            }

            if (researchCost > unlocksData.unlockPoints && allowUnlock)
            {
                unlockInfo = "UI_NOT_ENOUGH_RP";
                allowUnlock = false;
            }

            if (r is Resource)
            {
                PopupUnlockResource.Open(r as Resource, unlockInfo, allowUnlock);
            }
            else if (r is ItemTech)
            {
                if (r.dbName.StartsWith("ITEM_TECH-COOKING"))
                {
                    PopupUnlockFood.Open(r as ItemTech, unlockInfo, allowUnlock);
                }
                else
                {
                    PopupUnlockItem.Open(r as ItemTech, unlockInfo, allowUnlock);
                }

            }
            else if (r is BuildingTech)
            {
                if (Array.Find((r as BuildingTech).unlockRecipes, o => o.buildingRecipe != null) != null)
                {
                    PopupUnlockBuilding.Open(r as BuildingTech, unlockInfo, allowUnlock);
                }
                else
                {
                    PopupUnlockRitual.Open(r as BuildingTech, unlockInfo, allowUnlock);
                }
            }
        }       
    }
}
#endif