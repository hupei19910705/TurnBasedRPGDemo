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
    public string ImageKey { get; private set; }

    private const string IMAGE_PATH_PREFIX = "Texture/Icons/Skill/";

    public Skill(SkillType type,string id,string name,string imageKey = "")
    {
        Type = type;
        ID = id;
        Name = name;
        if (!string.IsNullOrEmpty(imageKey))
            ImageKey = IMAGE_PATH_PREFIX + imageKey;
    }
}
