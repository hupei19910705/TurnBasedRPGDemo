using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroDisplayView : MonoBehaviour
{
    [SerializeField] private Image _heroImage = null;
    [SerializeField] private Text _name = null;
    [SerializeField] private Text _level = null;
    [SerializeField] private Text _exp = null;
    [SerializeField] private Slider _expSlider = null;
    [SerializeField] private Text _hp = null;
    [SerializeField] private Text _mp = null;
    [SerializeField] private Text _attack = null;
    [SerializeField] private Text _defence = null;
    [SerializeField] private Text _job = null;
    [SerializeField] private Text _pos = null;

    public void SetData(int pos,double maxExp,HeroData data)
    {
        _heroImage.sprite = Resources.Load<Sprite>(data.HeadImageKey);
        _name.text = data.Name;
        _level.text = "LV" + data.Level.ToString();
        _exp.text = string.Format("{0}/{1}", data.Exp, maxExp);
        _expSlider.value = (float)(data.Exp / maxExp);
        _hp.text = data.OriginHp.ToString();
        _mp.text = data.OriginMp.ToString();
        _attack.text = data.PAttack.ToString();
        _defence.text = data.PDefence.ToString();
        _job.text = data.Job.ToString();
        _pos.text = pos.ToString();
    }
}
