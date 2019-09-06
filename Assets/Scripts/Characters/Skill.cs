using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    GeneralAttack,
    Physical,
    Magic
}

public class Skill
{
    public SkillType Type { get; private set; }
    public string ID { get; private set; }
    public string Name { get; private set; }
}
