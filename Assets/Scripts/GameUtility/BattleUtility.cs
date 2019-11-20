﻿using System;
using System.Collections;
using System.Collections.Generic;

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
    }
}