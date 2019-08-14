using UnityEngine;
using System.Collections.Generic;
using DBDef;
using TheHoney;
using Thea2.Common;
using Thea2.General;

namespace GameScript
{
    public class MyScripts : ScriptBase
    {
        static public string UIS_GetCurentPlayerName(object obj)
        {
            return "EXAMPLE MOD";
        }

        static public string SomeOtherScript(object obj)
        {
            return "2 + 2 = "+(2+2).ToString();
        }
    }
}