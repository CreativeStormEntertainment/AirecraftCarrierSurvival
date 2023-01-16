using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class ManeuverCalculator
{
    private static List<int> HelperList = new List<int>();
    private static HashSet<int> HelperSet = new HashSet<int>();
#if ALLOW_CHEATS
    public static bool Cheat;
#endif
    public static FightSquadronData GetSquadronsNeeded(List<PlayerManeuverData> playerManeuvers)
    {
        var dict = new Dictionary<EBonusValue, List<TargetBonusValueData>>();
        dict[EBonusValue.FlatAddAttackParameters] = new List<TargetBonusValueData>();
        dict[EBonusValue.MultiplyAttackParameters] = new List<TargetBonusValueData>();
        dict[EBonusValue.FlatAddSquadronType] = new List<TargetBonusValueData>();
        dict[EBonusValue.FlatValueSquadronType] = new List<TargetBonusValueData>();
        dict[EBonusValue.DisableModifier] = new List<TargetBonusValueData>();
        dict[EBonusValue.DisableAll] = new List<TargetBonusValueData>();

        var list = new List<AttackParametersData>();
        var squadronList = new List<FightSquadronData>();
        for (int i = 0; i < playerManeuvers.Count; i++)
        {
            AttackParametersData data;
            var maneuver = playerManeuvers[i];
            if (maneuver == null)
            {
                data = new AttackParametersData { Attack = Mathf.NegativeInfinity };
                squadronList.Add(null);
            }
            else
            {
                data = maneuver.Values;
                var maneuverSquadrons = new FightSquadronData();
                SetSquadrons(ref maneuverSquadrons, maneuver.NeededSquadrons.Type, (EManeuverSquadronType)maneuver.NeededSquadrons.Type, maneuver.NeededSquadrons.Count);
                squadronList.Add(maneuverSquadrons);
                AddBonusValues(dict, i, maneuver.Modifiers, playerManeuvers, maneuver.name);
            }
            list.Add(data);
        }

        foreach (var data in dict[EBonusValue.FlatAddSquadronType])
        {
            var squadronData = squadronList[data.Order];
            if (squadronData != null)
            {
                AddSquadrons(ref squadronData, playerManeuvers[data.Order].NeededSquadrons.Type, data.ValueData.SquadronType, (int)data.ValueData.Value1, (int)data.ValueData.Value2);
                squadronList[data.Order] = squadronData;
            }
        }

        foreach (var data in dict[EBonusValue.FlatValueSquadronType])
        {
            var squadronData = squadronList[data.Order];
            if (squadronData != null)
            {
                SetSquadrons(ref squadronData, playerManeuvers[data.Order].NeededSquadrons.Type, data.ValueData.SquadronType, (int)data.ValueData.Value1);
                squadronList[data.Order] = squadronData;
            }
        }

        var result = new FightSquadronData();
        foreach (var data in squadronList)
        {
            if (data != null)
            {
                Assert.IsTrue(data.Bombers >= 0);
                Assert.IsTrue(data.Fighters >= 0);
                Assert.IsTrue(data.Torpedoes >= 0);
                Add(ref result.Bombers, data.Bombers, 0);
                Add(ref result.Fighters, data.Fighters, 0);
                Add(ref result.Torpedoes, data.Torpedoes, 0);
            }
        }
        return result;
    }

    public static void Calculate(List<PlayerManeuverData> playerManeuvers, List<EnemyManeuverData> enemies, List<int> enemyDurability, int focusedEnemy, out FightSquadronData neededSquadrons,
        out AttackParametersData playerData, out List<AttackParametersData> playerValuesList, out AttackParametersData enemyData, out CasualtiesData casualtiesData,
        ECalculateType type, int minDivisor, int maxDivisor, int divisor, int bonusDefense, int bonusAttack, bool magic, int enemyAttMod, int enemyDefMod)
    {
        CalculateEnemy(enemies, enemyDurability, out var enemyList, out enemyData, type, enemyAttMod, enemyDefMod);

        playerData = new AttackParametersData();
        neededSquadrons = new FightSquadronData();
        casualtiesData = new CasualtiesData();
        playerValuesList = new List<AttackParametersData>();

        var dict = new Dictionary<EBonusValue, List<TargetBonusValueData>>();
        dict[EBonusValue.FlatAddAttackParameters] = new List<TargetBonusValueData>();
        dict[EBonusValue.MultiplyAttackParameters] = new List<TargetBonusValueData>();
        dict[EBonusValue.FlatAddSquadronType] = new List<TargetBonusValueData>();
        dict[EBonusValue.FlatValueSquadronType] = new List<TargetBonusValueData>();
        dict[EBonusValue.DisableModifier] = new List<TargetBonusValueData>();
        dict[EBonusValue.DisableAll] = new List<TargetBonusValueData>();

        var cannotEffectSet = new HashSet<int>();

        foreach (var maneuver in enemies)
        {
            AddBonusValues(dict, -1, maneuver.Modifiers, playerManeuvers, maneuver.name);
        }

        foreach (var data in dict[EBonusValue.DisableModifier])
        {
            Assert.IsFalse(data.Order < 0);
            if (playerManeuvers.Count > data.Order && playerManeuvers[data.Order] != null)
            {
                cannotEffectSet.Add(data.Order);
            }
        }

        foreach (var data in dict[EBonusValue.DisableAll])
        {
            Assert.IsFalse(data.Order < 0);
            if (playerManeuvers.Count > data.Order && playerManeuvers[data.Order] != null)
            {
                cannotEffectSet.Add(data.Order);
                var newData = data;
                newData.ValueData.Value1 = 0f;
                newData.ValueData.Value2 = 0f;
                dict[EBonusValue.MultiplyAttackParameters].Add(newData);
            }
        }

        var list = new List<AttackParametersData>();
        var squadronList = new List<FightSquadronData>();
        for (int i = 0; i < playerManeuvers.Count; i++)
        {
            AttackParametersData data;
            var maneuver = playerManeuvers[i];
            if (maneuver == null)
            {
                data = new AttackParametersData { Attack = Mathf.NegativeInfinity };
                squadronList.Add(null);
            }
            else
            {
                data = maneuver.Values;
                data.Defense += bonusDefense;
                data.Attack += bonusAttack;
                var maneuverSquadrons = new FightSquadronData();
                SetSquadrons(ref maneuverSquadrons, maneuver.NeededSquadrons.Type, (EManeuverSquadronType)maneuver.NeededSquadrons.Type, maneuver.NeededSquadrons.Count);
                squadronList.Add(maneuverSquadrons);
                if (!cannotEffectSet.Contains(i))
                {
                    AddBonusValues(dict, i, maneuver.Modifiers, playerManeuvers, maneuver.name);
                }
            }
            list.Add(data);
        }

        foreach (var data in dict[EBonusValue.FlatAddSquadronType])
        {
            var squadronData = squadronList[data.Order];
            if (squadronData != null)
            {
                AddSquadrons(ref squadronData, playerManeuvers[data.Order].NeededSquadrons.Type, data.ValueData.SquadronType, (int)data.ValueData.Value1, (int)data.ValueData.Value2);
                squadronList[data.Order] = squadronData;
            }
        }

        foreach (var data in dict[EBonusValue.FlatValueSquadronType])
        {
            var squadronData = squadronList[data.Order];
            if (squadronData != null )
            {
                SetSquadrons(ref squadronData, playerManeuvers[data.Order].NeededSquadrons.Type, data.ValueData.SquadronType, (int)data.ValueData.Value1);
                squadronList[data.Order] = squadronData;
            }
        }

        foreach (var data in squadronList)
        {
            if (data != null)
            {
                Assert.IsTrue(data.Bombers >= 0);
                Assert.IsTrue(data.Fighters >= 0);
                Assert.IsTrue(data.Torpedoes >= 0);
                Add(ref neededSquadrons.Bombers, data.Bombers, 0);
                Add(ref neededSquadrons.Fighters, data.Fighters, 0);
                Add(ref neededSquadrons.Torpedoes, data.Torpedoes, 0);
            }
        }

        foreach (var data in dict[EBonusValue.FlatAddAttackParameters])
        {
            Assert.IsFalse(data.Order < 0);
            if (data.Order < list.Count)
            {
                var paramsData = list[data.Order];
                if (paramsData.Attack != Mathf.NegativeInfinity)
                {
                    paramsData.Attack += data.ValueData.Value1;
                    paramsData.Defense += data.ValueData.Value2;
                    list[data.Order] = paramsData;
                }
            }
        }

        foreach (var data in dict[EBonusValue.MultiplyAttackParameters])
        {
            Assert.IsFalse(data.Order < 0);
            if (data.Order < list.Count)
            {
                var paramsData = list[data.Order];
                if (paramsData.Attack != Mathf.NegativeInfinity)
                {
                    paramsData.Attack *= data.ValueData.Value1;
                    paramsData.Defense *= data.ValueData.Value2;
                    list[data.Order] = paramsData;
                }
            }
        }
        for (int i = 0; i < list.Count; i++)
        {
            var data = list[i];
            data.Attack = Mathf.Ceil(data.Attack);
            data.Defense = Mathf.Ceil(data.Defense);

            playerValuesList.Add(data);
            if (data.Attack != Mathf.NegativeInfinity)
            {
                playerData.Attack += data.Attack;
                playerData.Defense += data.Defense;
            }
        }

        HelperList.Clear();
        HelperSet.Clear();
        for (int i = 0; i < enemyList.Count; i++)
        {
            HelperSet.Add(i);
        }
        if (focusedEnemy >= 0 && focusedEnemy < enemyList.Count)
        {
            if (magic)
            {
                var data = playerValuesList[0];
                data.Attack = playerData.Attack = enemyList[focusedEnemy].Defense;
                playerValuesList[0] = data;
            }
            for (int i = 0; i < enemyDurability[focusedEnemy]; i++)
            {
                HelperList.Add(focusedEnemy);
            }
            HelperSet.Remove(focusedEnemy);
        }
#if ALLOW_CHEATS
        if (Cheat)
        {
            playerData.Attack += 10000;
            playerData.Defense += 10000;
        }
#endif
        float playerAttack = playerData.Attack;
        while (HelperSet.Count > 0)
        {
            int index;
            switch (type)
            {
                case ECalculateType.TestMin:
                    float min = float.PositiveInfinity;
                    index = 1000;
                    foreach (var i in HelperSet)
                    {
                        if (enemyList[i].Defense < min)
                        {
                            min = enemyList[i].Defense;
                            index = i;
                        }
                    }
                    Assert.IsFalse(index == 1000);
                    break;
                case ECalculateType.TestMax:
                    float max = float.NegativeInfinity;
                    index = 1000;
                    foreach (var i in HelperSet)
                    {
                        if (enemyList[i].Defense > max)
                        {
                            max = enemyList[i].Defense;
                            index = i;
                        }
                    }
                    Assert.IsFalse(index == 1000);
                    break;
                case ECalculateType.Real:
                default:
                    index = RandomUtils.GetRandom(HelperSet);
                    break;
            }
            HelperList.Add(index);
            HelperSet.Remove(index);
        }
        for (int i = 0; i < enemyDurability.Count; i++)
        {
            if (i == focusedEnemy)
            {
                continue;
            }
            for (int j = 1; j < enemyDurability[i]; j++)
            {
                HelperList.Add(i);
            }
        }

        foreach (var index in HelperList)
        {
            playerAttack -= enemyList[index].Defense;
            if (playerAttack >= 0f)
            {
                int dur = enemyDurability[index];
                dur--;
                enemyDurability[index] = dur;
                if (dur < 1)
                {
                    casualtiesData.EnemyDestroyedIndices.Add(index);
                }
            }
            else
            {
                break;
            }
        }
        HelperList.Clear();
        if (enemyData.Attack > playerData.Defense)
        {
            var squadrons = new Dictionary<EPlaneType, int>();
            squadrons[EPlaneType.Bomber] = neededSquadrons.Bombers;
            squadrons[EPlaneType.Fighter] = neededSquadrons.Fighters;
            squadrons[EPlaneType.TorpedoBomber] = neededSquadrons.Torpedoes;
            int total = 0;
            foreach (int value in squadrons.Values)
            {
                total += value;
            }
            float diff = enemyData.Attack - playerData.Defense;
            int min = (int)(diff / minDivisor);
            int max = Mathf.CeilToInt(diff / maxDivisor);
            int damaged = Mathf.Max(Mathf.CeilToInt((diff / enemyData.Attack) * (100f / divisor)), min);
            damaged = Mathf.Min(damaged, max, total);
            for (int i = 0; i < damaged; i++)
            {
                int rand = Random.Range(0, total--);
                foreach (var squadronType in squadrons.Keys)
                {
                    if (rand < squadrons[squadronType])
                    {
                        squadrons[squadronType]--;
                        Assert.IsFalse(squadrons[squadronType] < 0);
                        ((type == ECalculateType.Real && Random.value < Parameters.Instance.DifficultyParams.SquadronBreakChance) ? casualtiesData.SquadronsBroken : casualtiesData.SquadronsDestroyed)[squadronType]++;
                        break;
                    }
                    else
                    {
                        Assert.IsFalse(squadrons[squadronType] < 0);
                        rand -= squadrons[squadronType];
                    }
                }
            }
        }
    }

    public static void CalculateEnemy(IEnumerable<EnemyManeuverData> enemies, IEnumerable<int> enemyDurability, out List<AttackParametersData> enemyList, out AttackParametersData enemyData, ECalculateType type, int enemyAttMod, int enemyDefMod)
    {
        enemyList = new List<AttackParametersData>();
        enemyData = new AttackParametersData();

        foreach (var maneuver in enemies)
        {
            AttackParametersData data;
            switch (type)
            {
                case ECalculateType.TestMin:
                    data = maneuver.MinValues;
                    break;
                case ECalculateType.TestMax:
                    data = maneuver.MaxValues;
                    break;
                case ECalculateType.Real:
                default:
                    data = new AttackParametersData(Random.Range(maneuver.MinValues.Attack, maneuver.MaxValues.Attack),
                        Random.Range(maneuver.MinValues.Defense, maneuver.MaxValues.Defense));
                    break;
            }
            data.Durability = maneuver.Durability;
            if (maneuver.MisidentifiedType != EMisidentifiedType.Unique)
            {
                if (maneuver.MinValues.Attack > enemyAttMod)
                {
                    data.Attack -= enemyAttMod;
                }
                if (maneuver.MinValues.Defense > enemyDefMod)
                {
                    data.Defense -= enemyDefMod;
                }
            }

            enemyList.Add(data);
        }

        using (var enumer = enemyDurability.GetEnumerator())
        {
            for (int i = 0; i < enemyList.Count; i++)
            {
                bool next = enumer.MoveNext();
                Assert.IsTrue(next);
                enemyData.Attack += enemyList[i].Attack;
                for (int j = 0; j < enumer.Current; j++)
                {
                    enemyData.Defense += enemyList[i].Defense;
                }
            }
        }
    }

    private static void AddBonusValues(Dictionary<EBonusValue, List<TargetBonusValueData>> dict, int order, List<BonusData> modifiers, List<PlayerManeuverData> playerManeuvers, string dataName)
    {
        foreach (var modifier in modifiers)
        {
            if (modifier.Effect == EBonusValue.MultiplyAttackParameters)
            {
                if (modifier.EffectData.Value1 < 0f)
                {
                    Debug.LogError("Maneuver " + dataName + " has negative attack multiplier!");
                }
                if (modifier.EffectData.Value2 < 0f)
                {
                    Debug.LogError("Maneuver " + dataName + " has negative defense multiplier!");
                }
            }

            var list = dict[modifier.Effect];
            int listIndex = list.Count;
            int ignoreSlot = (modifier.Benefitor == EBonusBenefitor.BaseWithoutThis ? order : -1);
            switch (modifier.ModifierType)
            {
                case EBonusType.Always:
                    Assert.IsFalse((int)modifier.Benefitor == -10);
                    ApplyBonusEveryone(list, modifier.EffectData, playerManeuvers.Count, ignoreSlot);
                    break;
                case EBonusType.Next:
                    int n = order + (modifier.Data.Negate ? -1 : 1);
                    if (n != -1)
                    {
                        list.Add(new TargetBonusValueData(n, modifier.EffectData));
                    }
                    break;
                case EBonusType.SpecificPiece:
                    list.Add(new TargetBonusValueData(modifier.Data.Slot, modifier.EffectData));
                    if (modifier.Data.Various > -1)
                    {
                        list.Add(new TargetBonusValueData(modifier.Data.Various, modifier.EffectData));
                    }
                    break;
                case EBonusType.NumberOfNext:
                    Assert.IsFalse((int)modifier.Benefitor == -10);
                    int start = (modifier.ThisSlot ? order : modifier.Data.Slot);
                    if (modifier.Data.Negate)
                    {
                        for (int i = start - 1; i >= 0; i--)
                        {
                            if (i != ignoreSlot)
                            {
                                list.Add(new TargetBonusValueData(i, modifier.EffectData));
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < modifier.Data.Various; i++)
                        {
                            int slot2 = i + start + 1;
                            if (slot2 != ignoreSlot)
                            {
                                list.Add(new TargetBonusValueData(slot2, modifier.EffectData));
                            }
                        }
                    }
                    if (!modifier.ThisSlot || modifier.Benefitor != EBonusBenefitor.BaseWithoutThis)
                    {
                        list.Add(new TargetBonusValueData(start, modifier.EffectData));
                    }
                    break;
                case EBonusType.AllOfManeuverType:
                    Assert.IsFalse((int)modifier.Benefitor == -10);
                    for (int i = 0; i < playerManeuvers.Count; i++)
                    {
                        var manev = playerManeuvers[i];
                        if (i != ignoreSlot && manev != null && (modifier.Data.Negate != (manev.ManeuverType == modifier.Data.ManeuverType)))
                        {
                            list.Add(new TargetBonusValueData(i, modifier.EffectData));
                        }
                    }
                    break;
                case EBonusType.AllOfSquadronType:
                    Assert.IsFalse((int)modifier.Benefitor == -10);
                    if (modifier.Benefitor <= EBonusBenefitor.BaseWithoutThis)
                    {
                        for (int i = 0; i < playerManeuvers.Count; i++)
                        {
                            var manev = playerManeuvers[i];
                            if (i != ignoreSlot && manev != null &&
                                (modifier.Data.Negate != (manev.NeededSquadrons.Type == modifier.SquadronType && manev.NeededSquadrons.Count > 0)))
                            {
                                list.Add(new TargetBonusValueData(i, modifier.EffectData));
                            }
                        }
                    }
                    else
                    {
                        int i = 0;
                        int max = order;
                        if (modifier.Benefitor == EBonusBenefitor.BaseAllNext)
                        {
                            i = order + 1;
                            max = playerManeuvers.Count;
                        }
                        for (; i < max; i++)
                        {
                            var manev = playerManeuvers[i];
                            if (manev != null && (modifier.Data.Negate != (manev.NeededSquadrons.Type == modifier.SquadronType && manev.NeededSquadrons.Count > 0)))
                            {
                                list.Add(new TargetBonusValueData(i, modifier.EffectData));
                            }
                        }
                    }
                    break;
                case EBonusType.SpecificPlacement:
                case EBonusType.AfterSpecificPlacement:
                    break;
                case EBonusType.CustomRequirements:
                    int slot = modifier.RequirementData.Slot;
                    Assert.IsFalse((int)modifier.Benefitor == -10);
                    switch (modifier.Requirement)
                    {
                        case EBonusRequirement.BetweenSquadrons:
                            if (order > 0 && playerManeuvers.Count > (order + 1))
                            {
                                var manev1 = playerManeuvers[order - 1];
                                var manev2 = playerManeuvers[order + 1];
                                if (manev1 != null && manev2 != null && manev1.NeededSquadrons.Type == modifier.RequirementData.SquadronType &&
                                playerManeuvers[order + 1].NeededSquadrons.Type == modifier.RequirementData.SquadronType)
                                {
                                    ApplyBonusBetween(list, modifier, order, playerManeuvers.Count);
                                }
                            }
                            break;
                        case EBonusRequirement.BetweenManeuvers:
                            if (order > 0 && playerManeuvers.Count > (order + 1))
                            {
                                var manev1 = playerManeuvers[order - 1];
                                var manev2 = playerManeuvers[order + 1];
                                if (manev1 != null && manev2 != null && manev1.ManeuverType == modifier.RequirementData.ManeuverType &&
                                    manev2.ManeuverType == modifier.RequirementData.ManeuverType)
                                {
                                    ApplyBonusBetween(list, modifier, order, playerManeuvers.Count);
                                }
                            }
                            break;
                        case EBonusRequirement.OnlyManeuver:
                            int count = 0;
                            int max = modifier.RequirementData.Negate ? 0 : 1;
                            bool ok = true;
                            var baseIDs = new List<int>();
                            for (int i = 0; i < playerManeuvers.Count; i++)
                            {
                                var manev = playerManeuvers[i];
                                if (manev != null)
                                {
                                    bool isManeuver = manev.ManeuverType == modifier.RequirementData.ManeuverType;
                                    if (isManeuver != modifier.RequirementData.Negate)
                                    {
                                        baseIDs.Add(i);
                                    }
                                    if (isManeuver && ++count > max)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            if (ok)
                            {
                                switch (modifier.Benefitor)
                                {
                                    case EBonusBenefitor.BaseWithoutThis:
                                        foreach (var index in baseIDs)
                                        {
                                            list.Add(new TargetBonusValueData(index, modifier.EffectData));
                                        }
                                        break;
                                    case EBonusBenefitor.BaseAndThis:
                                        foreach (var index in baseIDs)
                                        {
                                            list.Add(new TargetBonusValueData(index, modifier.EffectData));
                                        }
                                        ApplyBonus(list, modifier, order, playerManeuvers.Count);
                                        break;
                                    case EBonusBenefitor.JustThis:
                                    case EBonusBenefitor.Everyone:
                                        ApplyBonus(list, modifier, order, playerManeuvers.Count);
                                        break;
                                }
                            }
                            break;
                        case EBonusRequirement.SpecificOfManeuver:
                            if (playerManeuvers.Count > slot)
                            {
                                var manev = playerManeuvers[slot];
                                var maneuverType = modifier.RequirementData.ManeuverType;
                                if (manev != null && ((manev.ManeuverType == maneuverType) != modifier.RequirementData.Negate))
                                {
                                    int slot2 = modifier.RequirementData.Various;
                                    bool ok2 = slot2 < 0;
                                    if (!ok2 && playerManeuvers.Count > slot2)
                                    {
                                        manev = playerManeuvers[slot2];
                                        ok2 = manev != null && ((manev.ManeuverType == maneuverType) != modifier.RequirementData.Negate);
                                    }
                                    if (ok2)
                                    {
                                        ApplyBonusSpecific(list, modifier, slot, slot2, order, playerManeuvers.Count);
                                    }
                                }
                            }
                            break;
                        case EBonusRequirement.SpecificOfSquadron:
                            if (playerManeuvers.Count > slot)
                            {
                                var manev = playerManeuvers[slot];
                                var squadronType = modifier.RequirementData.SquadronType;
                                if (manev != null && ((manev.NeededSquadrons.Type == squadronType) != modifier.RequirementData.Negate))
                                {
                                    int slot2 = modifier.RequirementData.Various;
                                    bool ok2 = slot2 < 0;
                                    if (!ok2 && playerManeuvers.Count > slot2)
                                    {
                                        manev = playerManeuvers[slot2];
                                        ok2 = manev != null && ((manev.NeededSquadrons.Type == squadronType) != modifier.RequirementData.Negate);
                                    }
                                    if (ok2)
                                    {
                                        ApplyBonusSpecific(list, modifier, slot, slot2, order, playerManeuvers.Count);
                                    }
                                }
                            }
                            break;
                        case EBonusRequirement.GroupCountOfManeuver:
                            int count2 = 0;
                            int index2 = -1;
                            int newCount = 0;
                            int newIndex = -1;
                            var maneuverType2 = modifier.RequirementData.ManeuverType;
                            for (int i = 0; i < playerManeuvers.Count; i++)
                            {
                                var manev = playerManeuvers[i];
                                if (manev == null || manev.ManeuverType != maneuverType2)
                                {
                                    if (newCount > count2)
                                    {
                                        count2 = newCount;
                                        index2 = newIndex;
                                    }
                                    if (manev == null)
                                    {
                                        newCount = 0;
                                    }
                                    else
                                    {
                                        if (modifier.RequirementData.ManeuverType == EManeuverType.Any)
                                        {
                                            maneuverType2 = manev.ManeuverType;
                                        }
                                        newCount = 1;
                                        newIndex = i;
                                    }
                                }
                                else
                                {
                                    newCount++;
                                }
                            }
                            if (newCount > count2)
                            {
                                count2 = newCount;
                                index2 = newIndex;
                            }
                            if (modifier.RequirementData.Negate ? count2 < 2 : count2 >= modifier.RequirementData.Various)
                            {
                                ApplyBonusGroup(list, modifier, order, index2, count2, playerManeuvers.Count);
                            }
                            break;
                        case EBonusRequirement.AllManeuverGroups:
                            var manevType = (EManeuverType)1000;
                            int startIndex = 0;
                            int manevCount;
                            for (int i = 0; i < playerManeuvers.Count; i++)
                            {
                                var manev = playerManeuvers[i];
                                bool isNull = manev == null;
                                if (isNull || manev.ManeuverType != manevType)
                                {
                                    manevCount = i - startIndex;
                                    if (manevCount > 1)
                                    {
                                        ApplyBonusGroup(list, modifier, order, startIndex, manevCount, playerManeuvers.Count);
                                    }
                                    startIndex = i;
                                    manevType = isNull ? (EManeuverType)1000 : manev.ManeuverType;
                                }
                            }
                            manevCount = playerManeuvers.Count - startIndex;
                            if (manevCount > 1)
                            {
                                ApplyBonusGroup(list, modifier, order, startIndex, manevCount, playerManeuvers.Count);
                            }
                            break;
                        case EBonusRequirement.AllManeuversOfType:
                            var maneuverType3 = modifier.RequirementData.ManeuverType;
                            bool ok3 = true;
                            foreach (var manev in playerManeuvers)
                            {
                                if (manev.ManeuverType != maneuverType3)
                                {
                                    ok3 = false;
                                    break;
                                }
                            }
                            if (ok3)
                            {
                                switch (modifier.Benefitor)
                                {
                                    case EBonusBenefitor.BaseWithoutThis:
                                        ApplyBonusEveryone(list, modifier.EffectData, playerManeuvers.Count, order);
                                        break;
                                    case EBonusBenefitor.JustThis:
                                        list.Add(new TargetBonusValueData(order, modifier.EffectData));
                                        break;
                                    case EBonusBenefitor.BaseAndThis:
                                    case EBonusBenefitor.Everyone:
                                        ApplyBonusEveryone(list, modifier.EffectData, playerManeuvers.Count);
                                        break;
                                }
                            }
                            break;
                        case EBonusRequirement.NextManeuver:
                            var maneuverType4 = modifier.RequirementData.ManeuverType;

                            int index3 = order + (modifier.RequirementData.Negate ? -1 : 1);
                            if ((modifier.RequirementData.Negate ? order > 0 : playerManeuvers.Count > index3) && playerManeuvers[index3] != null &&
                                (maneuverType4 == EManeuverType.Any || playerManeuvers[index3].ManeuverType == maneuverType4))
                            {
                                switch (modifier.Benefitor)
                                {
                                    case EBonusBenefitor.BaseWithoutThis:
                                        list.Add(new TargetBonusValueData(index3, modifier.EffectData));
                                        break;
                                    case EBonusBenefitor.JustThis:
                                        list.Add(new TargetBonusValueData(order, modifier.EffectData));
                                        break;
                                    case EBonusBenefitor.BaseAndThis:
                                        list.Add(new TargetBonusValueData(order, modifier.EffectData));
                                        list.Add(new TargetBonusValueData(index3, modifier.EffectData));
                                        break;
                                    case EBonusBenefitor.Everyone:
                                        ApplyBonusEveryone(list, modifier.EffectData, playerManeuvers.Count);
                                        break;
                                }
                            }
                            break;
                        case EBonusRequirement.NextSquadron:
                            var squadronType2 = modifier.RequirementData.SquadronType;

                            int index4 = order + (modifier.RequirementData.Negate ? -1 : 1);
                            if ((modifier.RequirementData.Negate ? order > 0 : playerManeuvers.Count > index4) && playerManeuvers[index4] != null &&
                                playerManeuvers[index4].NeededSquadrons.Type == squadronType2)
                            {
                                switch (modifier.Benefitor)
                                {
                                    case EBonusBenefitor.BaseWithoutThis:
                                        list.Add(new TargetBonusValueData(index4, modifier.EffectData));
                                        break;
                                    case EBonusBenefitor.JustThis:
                                        list.Add(new TargetBonusValueData(order, modifier.EffectData));
                                        break;
                                    case EBonusBenefitor.BaseAndThis:
                                        list.Add(new TargetBonusValueData(order, modifier.EffectData));
                                        list.Add(new TargetBonusValueData(index4, modifier.EffectData));
                                        break;
                                    case EBonusBenefitor.Everyone:
                                        ApplyBonusEveryone(list, modifier.EffectData, playerManeuvers.Count);
                                        break;
                                }
                            }
                            break;
                    }
                    break;
            }
            for (int i = listIndex; i < list.Count; i++)
            {
                if (list[i].Order < 0)
                {
                    Assert.IsTrue(false, "Order error in " + dataName + ", " + order);
                }
            }
        }
    }

    private static void AddSquadrons(ref FightSquadronData data, EPlaneType type, EManeuverSquadronType squadronType, int count, int minValue)
    {
        switch (squadronType)
        {
            case EManeuverSquadronType.Bomber:
                if (type == EPlaneType.Bomber)
                {
                    Add(ref data.Bombers, count, minValue);
                }
                break;
            case EManeuverSquadronType.Fighter:
                if (type == EPlaneType.Fighter)
                {
                    Add(ref data.Fighters, count, minValue);
                }
                break;
            case EManeuverSquadronType.Torpedo:
                if (type == EPlaneType.TorpedoBomber)
                {
                    Add(ref data.Torpedoes, count, minValue);
                }
                break;
            case EManeuverSquadronType.Any:
                AddSquadrons(ref data, type, EManeuverSquadronType.Bomber, count, minValue);
                AddSquadrons(ref data, type, EManeuverSquadronType.Fighter, count, minValue);
                AddSquadrons(ref data, type, EManeuverSquadronType.Torpedo, count, minValue);
                break;
        }
    }

    private static void SetSquadrons(ref FightSquadronData data, EPlaneType type, EManeuverSquadronType squadronType, int count)
    {
        switch (squadronType)
        {
            case EManeuverSquadronType.Bomber:
                if (type == EPlaneType.Bomber)
                {
                    data.Bombers = count;
                }
                break;
            case EManeuverSquadronType.Fighter:
                if (type == EPlaneType.Fighter)
                {
                    data.Fighters = count;
                }
                break;
            case EManeuverSquadronType.Torpedo:
                if (type == EPlaneType.TorpedoBomber)
                {
                    data.Torpedoes = count;
                }
                break;
            case EManeuverSquadronType.Any:
                SetSquadrons(ref data, type, EManeuverSquadronType.Bomber, count);
                SetSquadrons(ref data, type, EManeuverSquadronType.Fighter, count);
                SetSquadrons(ref data, type, EManeuverSquadronType.Torpedo, count);
                break;
        }
    }

    private static void Add(ref int value, int delta, int minValue)
    {
        value = Mathf.Max(value + delta, minValue);
    }

    private static void ApplyBonus(List<TargetBonusValueData> list, BonusData data, int order, int maneuversCount)
    {
        if (data.Benefitor == EBonusBenefitor.Everyone)
        {
            ApplyBonusEveryone(list, data.EffectData, maneuversCount);
        }
        else
        {
            list.Add(new TargetBonusValueData(order, data.EffectData));
        }
    }

    private static void ApplyBonusEveryone(List<TargetBonusValueData> list, BonusValueData data, int maneuversCount, int ignoreSlot = -1)
    {
        for (int i = 0; i < maneuversCount; i++)
        {
            if (i != ignoreSlot)
            {
                list.Add(new TargetBonusValueData(i, data));
            }
        }
    }

    private static void ApplyBonusBetween(List<TargetBonusValueData> list, BonusData modifier, int index, int count)
    {
        switch (modifier.Benefitor)
        {
            case EBonusBenefitor.BaseWithoutThis:
                list.Add(new TargetBonusValueData(index - 1, modifier.EffectData));
                list.Add(new TargetBonusValueData(index + 1, modifier.EffectData));
                break;
            case EBonusBenefitor.BaseAndThis:
                list.Add(new TargetBonusValueData(index - 1, modifier.EffectData));
                list.Add(new TargetBonusValueData(index + 1, modifier.EffectData));
                ApplyBonus(list, modifier, index, count);
                break;
            case EBonusBenefitor.JustThis:
            case EBonusBenefitor.Everyone:
                ApplyBonus(list, modifier, index, count);
                break;
        }
    }

    private static void ApplyBonusSpecific(List<TargetBonusValueData> list, BonusData modifier, int slot, int slot2, int index, int count)
    {
        switch (modifier.Benefitor)
        {
            case EBonusBenefitor.BaseWithoutThis:
                list.Add(new TargetBonusValueData(slot, modifier.EffectData));
                if (slot2 > -1)
                {
                    list.Add(new TargetBonusValueData(modifier.RequirementData.Various, modifier.EffectData));
                }
                break;
            case EBonusBenefitor.BaseAndThis:
                list.Add(new TargetBonusValueData(slot, modifier.EffectData));
                if (slot2 > -1)
                {
                    list.Add(new TargetBonusValueData(modifier.RequirementData.Various, modifier.EffectData));
                }
                ApplyBonus(list, modifier, index, count);
                break;
            case EBonusBenefitor.JustThis:
            case EBonusBenefitor.Everyone:
                ApplyBonus(list, modifier, index, count);
                break;
            case EBonusBenefitor.AllNext:
            case EBonusBenefitor.AllPrev:
                ApplyBonusPrevNext(list, modifier, index, modifier.Benefitor, count);
                break;
            case EBonusBenefitor.BaseAllNext:
            case EBonusBenefitor.BaseAllPrev:
                ApplyBonusPrevNext(list, modifier, slot, modifier.Benefitor, count);
                break;
        }
    }

    private static void ApplyBonusGroup(List<TargetBonusValueData> list, BonusData modifier, int order, int startIndex, int count, int maxCount)
    {
        switch (modifier.Benefitor)
        {
            case EBonusBenefitor.BaseWithoutThis:
                for (int i = 0; i < count; i++)
                {
                    int index = startIndex + i;
                    if (index != order)
                    {
                        list.Add(new TargetBonusValueData(index, modifier.EffectData));
                    }
                }
                break;
            case EBonusBenefitor.BaseAndThis:
                for (int i = 0; i < count; i++)
                {
                    int index = startIndex + i;
                    list.Add(new TargetBonusValueData(index, modifier.EffectData));
                }
                break;
            case EBonusBenefitor.JustThis:
            case EBonusBenefitor.Everyone:
                ApplyBonus(list, modifier, order, maxCount);
                break;
        }
    }

    private static void ApplyBonusPrevNext(List<TargetBonusValueData> list, BonusData data, int index, EBonusBenefitor benefitor, int maneuversCount)
    {
        int next = (benefitor == EBonusBenefitor.AllPrev || benefitor == EBonusBenefitor.BaseAllPrev) ? -1 : 1;
        index += next;
        while (index >= 0 && index < maneuversCount)
        {
            list.Add(new TargetBonusValueData(index, data.EffectData));
            index += next;
        }
    }
}
