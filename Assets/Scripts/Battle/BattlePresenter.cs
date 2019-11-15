using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

public interface IBattlePresenter
{
    IEnumerator Run();
}
public class BattlePresenter : IBattlePresenter
{
    private IBattleView _view = null;
    private PlayerData _playerData = null;
    private Dictionary<int, EnemyData> _enemiesData = null;
    
    private bool _leave = false;
    private bool _isEnemyTurn = false;

    private int _curHeroIdx = -1;
    private int _targetEnemyIdx = -1;
    private int _targetHeroIdx = -1;
    private int _selectTargetHeroIdx = -1;

    private SerialCoroutines _serialCor = new SerialCoroutines();

    public BattlePresenter(
        IBattleView view,
        PlayerData playerData,
        Dictionary<int, EnemyData> enemiesData)
    {
        _view = view;
        _playerData = playerData;
        _enemiesData = enemiesData;
    }

    public IEnumerator Run()
    {
        _Register();
        _view.Enter(_playerData, _enemiesData);

        while (!_leave)
        {
            if(_isEnemyTurn)
            {
                yield return _EnemiesTurn();
                _ActiveHeroTurn();
                _isEnemyTurn = false;
            }
            yield return null;
        }
    }

    #region Init
    private void _Register()
    {
        _UnRegister();
        _view.ChangeSelectHero += _ChangeSelectHero;
        _view.ChangeSelectEnemy += _ChangeSelectEnemy;
        _view.ChangeTargetHero += _ChangeTargetHero;
        _view.HeroEndTurn += _EndHeroTurn;
        _view.UseItem += _UseItem;
        _view.UseSkill += _UseSkill;
    }

    private void _UnRegister()
    {
        _view.ChangeSelectHero -= _ChangeSelectHero;
        _view.ChangeSelectEnemy -= _ChangeSelectEnemy;
        _view.ChangeTargetHero -= _ChangeTargetHero;
        _view.HeroEndTurn -= _EndHeroTurn;
        _view.UseItem -= _UseItem;
        _view.UseSkill -= _UseSkill;
    }
    #endregion

    #region Hero
    private void _ChangeSelectHero(bool select,int pos)
    {
        _curHeroIdx = select ? pos : -1;
    }

    private void _EndHeroTurn(int pos)
    {
        _playerData.TeamHeroes[pos].SetEndTurnFlag(true);
        _isEnemyTurn = _CheckIsAllHeroTurnEnd();
    }

    private bool _CheckIsAllHeroTurnEnd()
    {
        foreach (var hero in _playerData.TeamHeroes.Values.ToList())
        {
            if (!hero.IsAlive)
                continue;
            if (!hero.IsTurnEnd)
                return false;
        }
        return true;
    }

    private int _GetAliveHeroIndex()
    {
        for (int i = 0; i < 6; i++)
        {
            if (_playerData.TeamHeroes.ContainsKey(i) && _playerData.TeamHeroes[i].IsAlive)
                return i;
        }
        _leave = true;
        return -1;
    }

    private void _ActiveHeroTurn()
    {
        foreach (var hero in _playerData.TeamHeroes.Values.ToList())
        {
            if (!hero.IsAlive)
                continue;
            hero.SetEndTurnFlag(false);
        }

        _view.ActiveHeroElement();
    }

    private void _HeroBeHit(double attack)
    {
        var hero = _playerData.TeamHeroes[_targetHeroIdx];
        hero.BeHit(attack);
    }

    private void _ChangeTargetHero(int pos)
    {
        _selectTargetHeroIdx = pos;
    }

    private void _UseItem(Item item)
    {
        switch (item.Type)
        {
            case ItemType.RedPotion:
                _playerData.TeamHeroes[_selectTargetHeroIdx].ChangeHp(item.EffectValue);
                break;
            case ItemType.BluePotion:
                _playerData.TeamHeroes[_selectTargetHeroIdx].ChangeMp(item.EffectValue);
                break;
            default:
                return;
        }
        item.RemoveNum(1);
        _playerData.FreshBackpack(item.Pos);
    }

    private void _UseSkill(Skill skill)
    {
        var from = _playerData.TeamHeroes[_curHeroIdx];

        if(skill == null)
            _enemiesData[_targetEnemyIdx].BeHit(_playerData.TeamHeroes[_curHeroIdx].Attack);
        else
        {
            from.ChangeMp(-skill.MpCost);

            var changeValue = skill.EffectValue;
            if (!skill.IsConstant)
                changeValue = Mathf.FloorToInt(skill.Multiple * (float)from.Attack);

            CharacterData target = null;
            switch(skill.EffectiveResult)
            {
                case EffectiveResult.Restore:
                    target = _playerData.TeamHeroes[_selectTargetHeroIdx];
                    if (skill.EffectType == EffectType.Mp)
                        target.ChangeMp(changeValue);
                    else
                        target.ChangeHp(changeValue);
                    break;
                case EffectiveResult.Reduce:
                    target = _enemiesData[_targetEnemyIdx];
                    if (skill.EffectType == EffectType.Mp)
                        target.ChangeMp(-changeValue);
                    else
                        target.BeHit(changeValue, skill.EffectType == EffectType.Real);
                    break;
            }
        }

        if (!_CheckIsAllEnemiesAlive())
            _leave = true;
    }
    #endregion

    #region Enemy
    private void _ChangeSelectEnemy(int pos)
    {
        _targetEnemyIdx = pos;
    }

    private IEnumerator _EnemiesTurn()
    {
        _targetHeroIdx = _GetAliveHeroIndex();
        foreach(var pair in _enemiesData)
        {
            if (pair.Value.IsAlive && _targetHeroIdx != -1)
            {
                _HeroBeHit(pair.Value.Attack);
                yield return _view.EnemyAttack(pair.Key, _targetHeroIdx);
                if (!_playerData.TeamHeroes[_targetHeroIdx].IsAlive)
                    _targetHeroIdx = _GetAliveHeroIndex();
            }
        }
    }

    private bool _CheckIsAllEnemiesAlive()
    {
        foreach (var enemy in _enemiesData.Values)
            if (enemy.IsAlive)
                return true;
        return false;
    }
    #endregion
}
