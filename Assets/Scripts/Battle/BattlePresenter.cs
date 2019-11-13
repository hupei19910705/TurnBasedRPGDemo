﻿using System.Collections;
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
    private TeamData _teamData = null;
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
        TeamData teamData,
        Dictionary<int, EnemyData> enemiesData)
    {
        _view = view;
        _teamData = teamData;
        _enemiesData = enemiesData;
    }

    public IEnumerator Run()
    {
        _Register();
        _view.Enter(_teamData, _enemiesData);

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
        _teamData.Heroes[pos].SetEndTurnFlag(true);
        _isEnemyTurn = _CheckIsAllHeroTurnEnd();
    }

    private bool _CheckIsAllHeroTurnEnd()
    {
        foreach (var hero in _teamData.Heroes.Values.ToList())
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
            if (_teamData.Heroes.ContainsKey(i) && _teamData.Heroes[i].IsAlive)
                return i;
        }
        _leave = true;
        return -1;
    }

    private void _ActiveHeroTurn()
    {
        foreach (var hero in _teamData.Heroes.Values.ToList())
        {
            if (!hero.IsAlive)
                continue;
            hero.SetEndTurnFlag(false);
        }

        _view.ActiveHeroElement();
    }

    private void _HeroBeHit(double attack)
    {
        var hero = _teamData.Heroes[_targetHeroIdx];
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
                _teamData.Heroes[_selectTargetHeroIdx].ChangeHp(item.EffectValue);
                break;
            case ItemType.BluePotion:
                _teamData.Heroes[_selectTargetHeroIdx].ChangeMp(item.EffectValue);
                break;
            default:
                return;
        }
        item.RemoveNum(1);
        _teamData.FreshBackpack(item.Pos);
    }

    private void _UseSkill(Skill skill)
    {
        var from = _teamData.Heroes[_curHeroIdx];

        if(skill == null)
            _enemiesData[_targetEnemyIdx].BeHit(_teamData.Heroes[_curHeroIdx].Attack);
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
                    target = _teamData.Heroes[_selectTargetHeroIdx];
                    if (skill.EffectType == EffectType.Mp)
                        target.ChangeMp(changeValue);
                    else
                        target.ChangeHp(changeValue);
                    break;
                case EffectiveResult.Reduce:
                    target = _enemiesData[_targetEnemyIdx];
                    changeValue = -changeValue;
                    if (skill.EffectType == EffectType.Mp)
                        target.ChangeMp(changeValue);
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
        foreach (var enemy in _enemiesData.Values)
        {
            if (enemy.IsAlive && _targetHeroIdx != -1)
            {
                _HeroBeHit(enemy.Attack);
                yield return _view.EnemyAttack(enemy.Pos, _targetHeroIdx);
                if (!_teamData.Heroes[_targetHeroIdx].IsAlive)
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
