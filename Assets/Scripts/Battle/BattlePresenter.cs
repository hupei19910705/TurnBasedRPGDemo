using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

public interface IBattlePresenter
{
    IEnumerator Run();
}

public enum RoundStatus
{
    None,
    HeroTurn,
    EnemyTurn
}

public enum FightStatus
{
    Fighting,
    FightResult,
    FightEnd
}

public class BattlePresenter : IBattlePresenter
{
    private IBattleView _view = null;
    private PlayerData _playerData = null;
    private Dictionary<int, EnemyData> _enemiesData = null;
    private GameData _gameData;
    private GameRecord _gameRecord;

    private RoundStatus _roundStatus;
    private FightStatus _fightStatus;

    private bool _leave = false;
    private bool _isWin = false;

    private int _roundCount = 0;

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
        _fightStatus = FightStatus.Fighting;
        _roundStatus = RoundStatus.HeroTurn;
        _roundCount = 0;

        while (!_leave)
        {
            switch (_fightStatus)
            {
                case FightStatus.Fighting:
                    yield return _FightRound();
                    break;
                case FightStatus.FightResult:
                    _BattleResultEvent();
                    _fightStatus = FightStatus.FightEnd;
                    break;
                case FightStatus.FightEnd:
                    break;
            }
            yield return null;
        }
    }

    private IEnumerator _FightRound()
    {
        _roundCount++;
        switch (_roundStatus)
        {
            case RoundStatus.HeroTurn:
                while (_roundStatus == RoundStatus.HeroTurn)
                    yield return null;
                break;
            case RoundStatus.EnemyTurn:
                yield return _EnemiesTurn();
                _BuffsEffect();
                _view.FreshInTheEndOfRound();
                _ActiveHeroTurn();
                break;
        }
        
    }

    private void _BuffsEffect()
    {
        foreach(var pair in _playerData.TeamHeroes)
        {
            if(pair.Value.IsAlive)
            {
                var models = pair.Value.BuffAndDebuffsEffect();
                _view.AddNumText(SelectTargetType.Hero, int.Parse(pair.Key), models);
            }
        }

        foreach (var pair in _enemiesData)
        {
            if (pair.Value.IsAlive)
            {
                var models = pair.Value.BuffAndDebuffsEffect();
                _view.AddNumText(SelectTargetType.Enemy, pair.Key, models);
            }
        }
    }
    #region Init
    private void _Register()
    {
        _UnRegister();
        _view.HeroEndTurn += _EndHeroTurn;
        _view.UseItem += _UseItem;
        _view.UseSkill += _UseSkill;
        _view.CheckIsFightWin += _CheckIsFightWin;
        _view.LeaveFight += _LeaveFight;
    }

    private void _UnRegister()
    {
        _view.HeroEndTurn -= _EndHeroTurn;
        _view.UseItem -= _UseItem;
        _view.UseSkill -= _UseSkill;
        _view.CheckIsFightWin -= _CheckIsFightWin;
        _view.LeaveFight -= _LeaveFight;
    }
    #endregion

    #region Hero
    private void _EndHeroTurn(int pos)
    {
        _playerData.TeamHeroes[pos.ToString()].SetEndTurnFlag(true);
        if (_CheckIsAllHeroTurnEnd())
            _roundStatus = RoundStatus.EnemyTurn;
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
            var key = i.ToString();
            if (_playerData.TeamHeroes.ContainsKey(key) && _playerData.TeamHeroes[key].IsAlive)
                return i;
        }
        _SetFightResultFlag(false);
        return -1;
    }

    private void _ActiveHeroTurn()
    {
        if (_fightStatus != FightStatus.Fighting)
            return;

        _roundStatus = RoundStatus.HeroTurn;
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
        var hero = _playerData.TeamHeroes[heroIdx.ToString()];
        var model = Skill.GeneralAtkModel(attack);
        var result = hero.ValueEffectByModel(model);
        _view.AddNumText(SelectTargetType.Hero, heroIdx, new List<ResultModel> { result });
    }

    private void _UseItem(int pos,int toIdx, SelectTargetType targetType)
    {
        var item = _playerData.BackPack[pos];
        if (item == null)
            return;

        var model = item.GetImmediatelyEffectModel();
        var buffs = item.GetBuffs();

        CharacterData target = null;
        switch(targetType)
        {
            case SelectTargetType.Hero:
                target = _playerData.TeamHeroes[toIdx.ToString()];
                break;
            case SelectTargetType.Enemy:
                target = _enemiesData[toIdx];
                break;
        }

        if (target == null)
            return;

        var result = target.ValueEffectByModel(model);
        target.AddBuffOrDebuffs(buffs);

        item.RemoveNum(1);
        _playerData.FreshBackpack(pos);

        _view.AddNumText(targetType, toIdx, new List<ResultModel> { result });
    }

    private List<ResultModel> _UseSkill(Skill skill,CharacterData fromData, CharacterData toData,UseTarget targetType)
    {
        if (skill == null)
        {
            var model = Skill.GeneralAtkModel(fromData.PAttack);
            var result = toData.ValueEffectByModel(model);
            return new List<ResultModel> { result };
        }

        List<ResultModel> results = new List<ResultModel>();
        fromData.ChangeMp(-skill.MpCost);
        var models = skill.GetImmediatelyEffectModels(fromData, targetType);
        var buffs = skill.GetBuffs(fromData, targetType);

        results = toData.ValueEffectByModels(models);
        toData.AddBuffOrDebuffs(buffs);

        return results;
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
                _HeroBeHit(pair.Value.PAttack, heroIdx);
                yield return _view.EnemyAttack(pair.Key, heroIdx);
                if (!_playerData.TeamHeroes[heroIdx.ToString()].IsAlive)
                    heroIdx = _GetAliveHeroIndex();

                if(_fightStatus == FightStatus.FightResult)
                    yield break;
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
    private void _CheckIsFightWin()
    {
        if(!_CheckIsAllEnemiesAlive())
            _SetFightResultFlag(true);
    }

    private void _SetFightResultFlag(bool win)
    {
        _fightStatus = FightStatus.FightResult;
        _roundStatus = RoundStatus.None;
        _view.SetFightEndFlag();
        _isWin = win;
    }

    private void _BattleResultEvent()
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

    private void _LeaveFight()
    {
        _leave = true;
    }
    #endregion
}
