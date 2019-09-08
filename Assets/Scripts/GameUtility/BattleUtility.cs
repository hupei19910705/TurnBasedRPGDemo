using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.GameUtility
{
    public class BattleUtility
    {
        private static BattleUtility _instance;
        public static BattleUtility Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BattleUtility();
                return _instance;
            }
        }

        public double GetLevelUpExpByLevel(int level)
        {
            return Math.Floor((Math.Pow(level - 1, 3) + 60) / 5f * (Math.Pow(level - 1, 2) + 60));
        }
    }
}