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
    private GameData _gameData;
    private GameRecord _gameRecord;

    private bool _battleEnd = false;
    private bool _leave = false;
    private bool _isEnemyTurn = false;
    private bool _isWin = false;

    //private int _selectTargetHeroIdx = -1;

    private SerialCoroutines _serialCor = new SerialCoroutines();

    public BattlePresenter(
        IBattleView view,
        PlayerData playerData,
        Dictionary<int, EnemyData> enemiesData,
        GameData gameData)
    {
        _view = view;
        _playerData = playerData;
        _enemiesData = enemiesData;
        _gameData = gameData;
        _gameRecord = GameUtility.Instance.GetCurGameRecord();
    }

    public IEnumerator Run()
    {
        _Register();
        _view.Enter(_playerData, _enemiesData);

        while (!_leave)
        {
            if(!_battleEnd && _isEnemyTurn)
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
        _view.HeroEndTurn += _EndHeroTurn;
        _view.UseItem += _UseItem;
        _view.UseSkill += _UseSkill;
        _view.OnSettlementBattle += _BattleEndEvent;
        _view.LeaveBattle += _LeaveBattle;
    }

    private void _UnRegister()
    {
        _view.HeroEndTurn -= _EndHeroTurn;
        _view.UseItem -= _UseItem;
        _view.UseSkill -= _UseSkill;
        _view.OnSettlementBattle -= _BattleEndEvent;
        _view.LeaveBattle -= _LeaveBattle;
    }
    #endregion

    #region Hero
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
        _SetBattleEndFlag(false);
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

    private void _HeroBeHit(int attack,int heroIdx)
    {
        var hero = _playerData.TeamHeroes[heroIdx];
        hero.BeHit(attack);
    }

    private void _UseItem(Item item,int toIdx)
    {
        switch (item.Type)
        {
            case ItemType.RedPotion:
                _playerData.TeamHeroes[toIdx].ChangeHp(item.EffectValue);
                break;
            case ItemType.BluePotion:
                _playerData.TeamHeroes[toIdx].ChangeMp(item.EffectValue);
                break;
            default:
                return;
        }
        item.RemoveNum(1);
        _playerData.FreshBackpack(item.Pos);
    }

    private void _UseSkill(Skill skill,int fromIdx,int toIdx)
    {
        var from = _playerData.TeamHeroes[fromIdx];

        if(skill == null)
            _enemiesData[toIdx].BeHit(_playerData.TeamHeroes[fromIdx].Attack);
        else
        {
            from.ChangeMp(-skill.MpCost);

            var changeValue = skill.EffectValue;
            if (!skill.IsConstant)
                changeValue = Mathf.FloorToInt(skill.Multiple * from.Attack);

            CharacterData target = null;
            switch(skill.EffectiveResult)
            {
                case EffectiveResult.Restore:
                    target = _playerData.TeamHeroes[toIdx];
                    if (skill.EffectType == EffectType.Mp)
                        target.ChangeMp(changeValue);
                    else
                        target.ChangeHp(changeValue);
                    break;
                case EffectiveResult.Reduce:
                    target = _enemiesData[toIdx];
                    if (skill.EffectType == EffectType.Mp)
                        target.ChangeMp(-changeValue);
                    else
                        target.BeHit(changeValue, skill.EffectType == EffectType.Real);
                    break;
            }
        }

        if (!_CheckIsAllEnemiesAlive())
            _SetBattleEndFlag(true);
    }
    #endregion

    #region Enemy
    private IEnumerator _EnemiesTurn()
    {
        var heroIdx = _GetAliveHeroIndex();
        foreach(var pair in _enemiesData)
        {
            if (pair.Value.IsAlive && heroIdx != -1)
            {
                _HeroBeHit(pair.Value.Attack, heroIdx);
                yield return _view.EnemyAttack(pair.Key, heroIdx);
                if (!_playerData.TeamHeroes[heroIdx].IsAlive)
                    heroIdx = _GetAliveHeroIndex();
                if (_battleEnd)
                {
                    _BattleEndEvent();
                    break;
                }
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

    #region Settlement
    private void _SetBattleEndFlag(bool win)
    {
        _battleEnd = true;
        _view.SetBattleEndFlag();
        _isWin = win;
    }

    private void _BattleEndEvent()
    {
        int totalDropExp = 0;
        List<Item> totalDropItems = new List<Item>();

        if(_isWin)
        {
            foreach (var pair in _enemiesData)
            {
                totalDropExp += pair.Value.DropExp;
                var dropList = pair.Value.DropItems;
                for (int i = 0; i < dropList.Count; i++)
                {
                    var item = GameUtility.Instance.CaculateDropItems(dropList[i]);
                    if (item != null)
                        totalDropItems.Add(item);
                }
            }
        }
        
        var levelExpDatas = _AddExpToTeamHeroes(totalDropExp);
        _playerData.AddItems(totalDropItems);
        GameUtility.Instance.Save();
        _view.PopupResultPage(_isWin, levelExpDatas, totalDropItems);
    }

    private Dictionary<string, HeroLevelExpData> _AddExpToTeamHeroes(int exp)
    {
        Dictionary<string, HeroLevelExpData> levelExpDatas = new Dictionary<string, HeroLevelExpData>();
        var levelExpTable = _gameData.LevelExpTable;
        foreach (var hero in _playerData.Heroes.Values)
        {
            HeroLevelExpData levelExpData = _playerData.AddExp(hero.UID, exp, levelExpTable);
            levelExpDatas.Add(hero.UID, levelExpData);
        }
        return levelExpDatas;
    }

    private void _LeaveBattle()
    {
        _leave = true;
    }
    #endregion
}
