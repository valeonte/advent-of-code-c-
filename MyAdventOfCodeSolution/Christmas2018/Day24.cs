using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day24 : Day
    {
        enum AttackType
        {
            Radiation, Bludgeoning, Fire,
            Slashing, Cold
        }

        class Unit
        {
            public int HitPoints { get; }
            public AttackType AttackType { get; }
            public ICollection<AttackType> Weaknesses { get; }
            public ICollection<AttackType> Immunities { get; }
            public int AttackDamage { get; private set; }
            public int Initiative { get; }

            public Unit(int hitPoints, AttackType attackType, ICollection<AttackType> weaknesses, ICollection<AttackType> immunities, int attackDamage, int initiative)
            {
                HitPoints = hitPoints;
                AttackType = attackType;
                Weaknesses = weaknesses;
                Immunities = immunities;
                AttackDamage = attackDamage;
                Initiative = initiative;
            }

            public void Boost(int boost)
            {
                AttackDamage += boost;
            }

            public override string ToString() => $"{HitPoints} hp unit with {AttackDamage} {AttackType} damage";
        }

        enum Side
        {
            ImmuneSystem, Infection
        }

        class UnitGroup
        {
            UnitGroup(int unitCount, Unit unit, Side side, int groupId)
            {
                UnitCount = unitCount;
                Unit = unit;
                Side = side;
                GroupId = groupId;
            }

            public int UnitCount { get; private set; }
            public Unit Unit { get; }
            public Side Side { get; }
            public int GroupId { get; }

            public UnitGroup Target { get; set; }

            public int EffectivePower => UnitCount * Unit.AttackDamage;

            public int ReceiveDamageFrom(UnitGroup attacker)
            {
                var damageDealt = attacker.DamageDealtTo(Unit);
                var unitsKilled = damageDealt / Unit.HitPoints;
                UnitCount = Math.Max(0, UnitCount - unitsKilled);

                return unitsKilled;
            }

            int DamageDealtTo(Unit enemy)
            {
                if (enemy.Immunities.Contains(Unit.AttackType)) return 0;
                
                return (enemy.Weaknesses.Contains(Unit.AttackType) ? 2 : 1) * EffectivePower;
            }

            public (UnitGroup Group, int DamageDealt) PickTarget(IEnumerable<UnitGroup> groups)
            {
                return (from g in groups
                        where g.Side != Side
                        let damageDealt = DamageDealtTo(g.Unit)
                        orderby damageDealt descending,
                            g.EffectivePower descending,
                            g.Unit.Initiative descending
                        select (g, damageDealt))
                    .FirstOrDefault();
            }

            public override string ToString() => $"{Side} group {GroupId} contains {UnitCount} x {Unit}";

            static readonly Regex LineRegex1 = new Regex(@"^(?<unitCount>\d+) units each with (?<hitPoints>\d+) hit points( \((immune to (?<immunities>[\w ,]+))*(; )*(weak to (?<weaknesses>[\w ,]+))*\))* with an attack that does (?<attackDamage>\d+) (?<attackType>\w+) damage at initiative (?<initiative>\d+)$");
            static readonly Regex LineRegex2 = new Regex(@"^(?<unitCount>\d+) units each with (?<hitPoints>\d+) hit points( \((weak to (?<weaknesses>[\w ,]+))*(; )*(immune to (?<immunities>[\w ,]+))*\))* with an attack that does (?<attackDamage>\d+) (?<attackType>\w+) damage at initiative (?<initiative>\d+)$");

            static IEnumerable<AttackType> EnumerateAttackTypes(Group matchGroup)
            {
                if (!matchGroup.Success) yield break;

                foreach (var attackType in matchGroup.Value.Split(',').Select(s=>s.Trim()))
                {
                    yield return (AttackType) Enum.Parse(typeof(AttackType), attackType, true);
                }
            }

            public static UnitGroup FromLine(string line, Side side, int groupId)
            {
                var match = LineRegex1.Match(line);
                if (!match.Success) match = LineRegex2.Match(line);
                if (!match.Success) throw new InvalidOperationException();

                var unitCount = int.Parse(match.Groups["unitCount"].Value);

                var hitPoints = int.Parse(match.Groups["hitPoints"].Value);

                var immunities = EnumerateAttackTypes(match.Groups["immunities"]).ToArray();
                var weaknesses = EnumerateAttackTypes(match.Groups["weaknesses"]).ToArray();

                var attackDamage = int.Parse(match.Groups["attackDamage"].Value);
                var attackType = (AttackType) Enum.Parse(typeof(AttackType), match.Groups["attackType"].Value, true);
                var initiative = int.Parse(match.Groups["initiative"].Value);

                var unit = new Unit(hitPoints, attackType, weaknesses, immunities, attackDamage, initiative);

                return new UnitGroup(unitCount, unit, side, groupId);
            }
        }

        const string TestInput = @"Immune System:
17 units each with 5390 hit points (weak to radiation, bludgeoning) with an attack that does 4507 fire damage at initiative 2
989 units each with 1274 hit points (immune to fire; weak to bludgeoning, slashing) with an attack that does 25 slashing damage at initiative 3

Infection:
801 units each with 4706 hit points (weak to radiation) with an attack that does 116 bludgeoning damage at initiative 1
4485 units each with 2961 hit points (immune to radiation; weak to fire, cold) with an attack that does 12 slashing damage at initiative 4";

        public override dynamic Answer1()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();

            var groups = ParseGroups(input);

            FightToDeath(groups);

            return groups.Sum(g => g.UnitCount);
        }

        public override dynamic Answer2()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();

            var totalBoost = 0;
            List<UnitGroup> groups;
            bool fightHasWinner;
            do
            {
                totalBoost++;
                groups = ParseGroups(input);
                foreach (var immuneGroup in groups.Where(g => g.Side == Side.ImmuneSystem))
                {
                    immuneGroup.Unit.Boost(totalBoost);
                }

                fightHasWinner = FightToDeath(groups);

            } while (!fightHasWinner || groups[0].Side == Side.Infection);


            return groups.Sum(g => g.UnitCount);
        }

        static bool FightToDeath(List<UnitGroup> groups)
        {
            do
            {
                PickTargets(groups);
                var unitsKilled = Attack(groups);
                if (unitsKilled == 0) return false;

                groups.RemoveAll(g => g.UnitCount == 0);
            } while (groups.Select(g => g.Side).Distinct().Count() > 1);

            return true;
        }

        static List<UnitGroup> ParseGroups(string[] input)
        {
            var groups = new List<UnitGroup>();
            var addToImmune = true;
            var groupCnt = 1;
            foreach (var line in input.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                if (line.Equals("Immune System:", StringComparison.OrdinalIgnoreCase))
                {
                    addToImmune = true;
                    groupCnt = 1;
                    continue;
                }

                if (line.Equals("Infection:", StringComparison.OrdinalIgnoreCase))
                {
                    addToImmune = false;
                    groupCnt = 1;
                    continue;
                }

                groups.Add(UnitGroup.FromLine(line, addToImmune ? Side.ImmuneSystem : Side.Infection, groupCnt++));
            }

            return groups;
        }

        static int Attack(IEnumerable<UnitGroup> groups)
        {
            var totalUnitsKilled = 0;
            foreach (var attacker in groups.OrderByDescending(g => g.Unit.Initiative))
            {
                if (attacker.Target == null || attacker.UnitCount == 0) continue;

                totalUnitsKilled += attacker.Target.ReceiveDamageFrom(attacker);
            }

            return totalUnitsKilled;
        }

        static void PickTargets(IReadOnlyCollection<UnitGroup> groups)
        {
            var availableTargets = new List<UnitGroup>(groups);
            foreach (var attacker in groups.OrderByDescending(g=>g.EffectivePower).ThenByDescending(g=>g.Unit.Initiative))
            {
                var target = attacker.PickTarget(availableTargets);
                if (target.Group != null && target.DamageDealt > 0)
                {
                    attacker.Target = target.Group;

                    availableTargets.Remove(target.Group);
                }
                else
                {
                    attacker.Target = null;
                }
            }
        }
    }
}