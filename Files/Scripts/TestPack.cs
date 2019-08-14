#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
﻿using UnityEngine;
using System.Collections.Generic;
using DBDef;
using Thea2;

namespace GameScript
{
    public class Scripts : ScriptBase
    {
        #region Secondary Stat Calculation
//         static public object AttributesProcessing(Attributes a)
//         {
//             a.finalAttributes = new Dictionary<Tag, float>();
//             foreach (KeyValuePair<Tag, float> pair in a.baseAttributes)
//             {
//                 a.finalAttributes[pair.Key] = pair.Value;
//             }
// 
//             Tag str = Globals.GetInstanceFromDB<Tag>("TAG", "TAG-STRENGTH");
//             Tag carry = Globals.GetInstanceFromDB<Tag>("TAG", "TAG-PERSONAL_CARRY_LIMIT");
// 
//             foreach (KeyValuePair<Tag, float> pair in a.baseAttributes)
//             {
//                 //Carry is 50 times character strength
//                 if (str != null && carry != null)
//                 {
//                     if (pair.Key == str)
//                     {
//                         a.finalAttributes[carry] = pair.Value * 50f + 1;
//                     }
//                 }
//             }
// 
//             Debug.Log("-.-");
//             return null;
//         }
        #endregion
    }
}
#endif