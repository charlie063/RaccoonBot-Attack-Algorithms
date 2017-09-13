﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CoC_Bot.API;
using CoC_Bot.API.Buildings;

namespace AllInOnePushDeploy
{
    static class TroopsDeployment
    {
        public static IEnumerable<int> GroundAttack(int customOrder)
        {
            DeploymentMethods.clanCastle = AllInOnePushDeploy.deployElements.ExtractOne(u => u.ElementType == DeployElementType.ClanTroops);
            DeploymentMethods.eq = AllInOnePushDeploy.deployElements.Extract(u => u.Id == DeployId.Earthquake);
            DeploymentMethods.rageSpell = AllInOnePushDeploy.deployElements.Extract(u => u.Id == DeployId.Rage);
            DeploymentMethods.healSpell = AllInOnePushDeploy.deployElements.Extract(u => u.Id == DeployId.Heal);
            DeploymentMethods.freezeSpell = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Freeze);
            DeploymentMethods.jumpSpell = AllInOnePushDeploy.deployElements.Extract(u => u.Id == DeployId.Jump);
            DeploymentMethods.hasteSpell = AllInOnePushDeploy.deployElements.Extract(u => u.Id == DeployId.Haste);
            DeploymentMethods.poison = AllInOnePushDeploy.deployElements.Extract(u => u.Id == DeployId.Poison);
            //tanks
            DeploymentMethods.giant = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Giant);
            DeploymentMethods.golem = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Golem);
            //main troops
            DeploymentMethods.wallbreaker = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.WallBreaker);
            DeploymentMethods.bowler = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Bowler);
            DeploymentMethods.witch = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Witch);
            DeploymentMethods.healer = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Healer);
            DeploymentMethods.wizard = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Wizard);

            DeploymentMethods.spells = AllInOnePushDeploy.deployElements.Extract(DeployElementType.Spell);

            DeploymentMethods.heroes = AllInOnePushDeploy.deployElements.Extract(x => x.IsHero);
            DeploymentMethods.warden = DeploymentMethods.heroes.ExtractOne(u => u.ElementType == DeployElementType.HeroWarden);
            DeploymentMethods.queen = DeploymentMethods.heroes.ExtractOne(DeployId.Queen);

            // Drop earthquake 
            foreach (var t in DeploymentMethods.DropEQ())
                yield return t;

            if (customOrder == 1)
            {
                foreach (var t in DeploymentMethods.DeployInCustomOrder(AllInOnePushDeploy.CustomOrderList))
                    yield return t;
            }
            else
            {
                // Check if queen walk is active
                var QW = AllInOnePushDeploy.QWSettings == 1 && DeploymentMethods.queen?.Count > 0 && DeploymentMethods.healer?.Count >= AllInOnePushDeploy.HealerOnQWSettings ? true : false;
                if (QW)
                {
                    foreach (var s in DeploymentMethods.DeployFunnlling())
                        yield return s;
                    foreach (var s in DeploymentMethods.DeployGolems())
                        yield return s;
                }
                else
                {
                    foreach (var s in DeploymentMethods.DeployGolems())
                        yield return s;
                    foreach (var s in DeploymentMethods.DeployFunnlling())
                        yield return s;
                }
                foreach (var s in DeploymentMethods.DeployGiants())
                    yield return s;
                foreach (var s in DeploymentMethods.DeployHeroes())
                    yield return s;
                foreach (var s in DeploymentMethods.DeployWB())
                    yield return s;
                foreach (var s in DeploymentMethods.DeployNormalTroops())
                    yield return s;
            }

            // Screen Shots for spells if debug mode is on
            if (AllInOnePushDeploy.Debug)
            {
                AllInOnePushHelper.DebugEQpells();
                AllInOnePushHelper.DebugSpells();
            }

            // Deploy spells
            foreach (var s in DeploymentMethods.DeployJump())
                yield return s;

            foreach (var s in DeploymentMethods.DeploySpell(DeploymentMethods.rageSpell, AllInOnePushDeploy.FirstRagePoint))
                yield return s;

            yield return 2000;

            if (DeploymentMethods.hasteSpell.Sum(u => u.Count) > 0)
            {
                foreach (var s in DeploymentMethods.DeploySpell(DeploymentMethods.hasteSpell, AllInOnePushDeploy.FirstHealPoint))
                    yield return s;
            }else
            {
                foreach (var s in DeploymentMethods.DeploySpell(DeploymentMethods.healSpell, AllInOnePushDeploy.FirstHealPoint))
                    yield return s;
            }

            if (DeploymentMethods.poison?.Count > 0) 
                foreach (var s in DeploymentMethods.DeploySpell(DeploymentMethods.poison, AllInOnePushDeploy.FirstHealPoint))
                    yield return s;

            // Use freeze if inferno is found
            if (DeploymentMethods.freezeSpell?.Count > 0)
            {
                var infernos = InfernoTower.Find();
                // find and watch inferno towers
                if (infernos != null)
                {
                    foreach (var inferno in infernos)
                    {
                        inferno.FirstActivated += DropFreeze;
                        inferno.StartWatching();
                    }
                }
            }

            yield return 4000;
            // activate Grand Warden apility
            if (DeploymentMethods.isWarden)
            {
                DeploymentMethods.warden.Select();
                DeploymentMethods.warden.Select();
            }

            foreach (var s in DeploymentMethods.DeploySpell(DeploymentMethods.rageSpell, AllInOnePushDeploy.SecondRagePoint))
                yield return s;

            foreach (var s in DeploymentMethods.DeploySpell(DeploymentMethods.healSpell, AllInOnePushDeploy.SecondRagePoint))
                yield return s;

            yield return 1000;

            foreach (var s in DeploymentMethods.DeploySpell(DeploymentMethods.rageSpell, AllInOnePushDeploy.FirstHastePoint))
                yield return s;

            foreach (var s in DeploymentMethods.DeploySpell(DeploymentMethods.hasteSpell, AllInOnePushDeploy.FirstHastePoint))
                yield return s;



            // Start watching heroes
            if (DeploymentMethods.watchHeroes == true)
            {
                Deploy.WatchHeroes(DeploymentMethods.heroes);
            }

            if (DeploymentMethods.watchQueen == true)
            {
                Deploy.WatchHeroes(new List<DeployElement> { DeploymentMethods.queen });
            }
        }

        public static IEnumerable<int> AirAttack(int customOrder)
        {
            Log.Info($"[{AllInOnePushDeploy.AttackName}] 'Air Attack' has been activated");

            DeploymentMethods.wallbreaker = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.WallBreaker);
            DeploymentMethods.balloon = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Balloon);
            DeploymentMethods.minion = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Minion);
            DeploymentMethods.babyDragon = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.BabyDragon);
            DeploymentMethods.dragon = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.Dragon);
            DeploymentMethods.lava = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.LavaHound);

            DeploymentMethods.spells = AllInOnePushDeploy.deployElements.Extract(DeployElementType.Spell);

            DeploymentMethods.lightingSpell = DeploymentMethods.spells.ExtractOne(DeployId.Lightning);
            DeploymentMethods.eq = DeploymentMethods.spells.Extract(u => u.Id == DeployId.Earthquake);
            DeploymentMethods.hasteSpell = DeploymentMethods.spells.Extract(u => u.Id == DeployId.Haste);
            DeploymentMethods.rageSpell = DeploymentMethods.spells.Extract(u => u.Id == DeployId.Rage);
            DeploymentMethods.freezeSpell = DeploymentMethods.spells.ExtractOne(DeployId.Freeze);

            DeploymentMethods.clanCastle = AllInOnePushDeploy.deployElements.ExtractOne(DeployId.ClanCastle);

            DeploymentMethods.heroes = AllInOnePushDeploy.deployElements.Extract(x => x.IsHero);

            DeploymentMethods.warden = DeploymentMethods.heroes.ExtractOne(u => u.ElementType == DeployElementType.HeroWarden);
            DeploymentMethods.queen = DeploymentMethods.heroes.ExtractOne(DeployId.Queen);
            
            DeploymentMethods.dragonAttack = DeploymentMethods.dragon?.Count >= 5 ? true : false;
            DeploymentMethods.babyLoon = DeploymentMethods.babyDragon?.Count >= 7 ? true : false;
            DeploymentMethods.lavaloonion = DeploymentMethods.balloon?.Count >= 15 ? true : false;

            if (DeploymentMethods.lightingSpell?.Count >= 2)
            {
                foreach (var t in DeploymentMethods.ZapAirDefense())
                    yield return t;
            }

            if (DeploymentMethods.lightingSpell?.Count >= 2)
            {
                foreach (var t in DeploymentMethods.ZapAirDefense())
                    yield return t;
            }

            if (customOrder == 1)
            {
                foreach (var t in DeploymentMethods.DeployInCustomOrderAir(AllInOnePushDeploy.CustomOrderList))
                    yield return t;
            }
            else
            {
                if(DeploymentMethods.babyLoon)
                {
                    foreach (var t in DeploymentMethods.AirFunnelling())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployBabyDragons())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployBalloons())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployLava())
                        yield return t;
                }
                else if(DeploymentMethods.dragonAttack)
                {
                    foreach (var t in DeploymentMethods.AirFunnelling())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployLava())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployDragons())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployBalloons())
                        yield return t;
                }
                else if (DeploymentMethods.lavaloonion)
                {
                    foreach (var t in DeploymentMethods.DeployLava())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployBalloons())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployDragons())
                        yield return t;

                    foreach (var t in DeploymentMethods.DeployBabyDragons())
                        yield return t;    
                }  
            }

            if (DeploymentMethods.clanCastle?.Count > 0)
            {
                foreach (var t in Deploy.AtPoint(DeploymentMethods.clanCastle, AllInOnePushDeploy.Origin))
                    yield return t;
            }

            if (DeploymentMethods.warden?.Count > 0)
            {
                foreach (var t in Deploy.AtPoint(DeploymentMethods.warden, AllInOnePushDeploy.Origin))
                    yield return t;


                DeploymentMethods.isWarden = true;
            }
            else
                DeploymentMethods.isWarden = false;

            // Start with haste <IF> it's more than or equal rages count <ELSE> start with rages.
            var firstSpell = DeploymentMethods.hasteSpell?.Sum(u => u.Count) >= DeploymentMethods.rageSpell?.Sum(u => u.Count) ? DeploymentMethods.hasteSpell : DeploymentMethods.rageSpell;
            var secondSpell = firstSpell == DeploymentMethods.hasteSpell ? DeploymentMethods.rageSpell : DeploymentMethods.hasteSpell;

            var firstSpellUnit = firstSpell.FirstOrDefault()?.Count > 0 ? firstSpell.FirstOrDefault() : firstSpell.LastOrDefault();
            var secondSpellUnit = secondSpell.FirstOrDefault()?.Count > 0 ? secondSpell.FirstOrDefault() : secondSpell.LastOrDefault();

            var line = AllInOnePushDeploy.FirstHasteLine;

            // Todo: deploy rages for TH8, 2 on the first line then 1 on the second line.

            if (firstSpellUnit?.Count > 0)
            {
                var count = firstSpellUnit.Count >= 3 ? 3 : firstSpellUnit.Count;
                foreach (var t in Deploy.AlongLine(firstSpellUnit, line.Item1, line.Item2, count, count, 100))
                    yield return t;

                line = AllInOnePushDeploy.FirstRageLine;
            }

            foreach (var t in DeploymentMethods.DeployMinions())
                yield return t;

            yield return 4500;

            if (secondSpellUnit?.Count > 0)
            {
                var count = secondSpellUnit.Count >= 3 ? 3 : secondSpellUnit.Count;
                foreach (var t in Deploy.AlongLine(secondSpellUnit, line.Item1, line.Item2, count, count, 100))
                    yield return t;

                line = AllInOnePushDeploy.SecondHasteLine;
            }
            else
            {
                if (firstSpell?.Sum(u => u.Count) > 0)
                {
                    firstSpellUnit = firstSpell.FirstOrDefault().Count > 0 ? firstSpell.FirstOrDefault() : firstSpell.LastOrDefault();
                    var count = firstSpellUnit.Count >= 3 ? 3 : firstSpellUnit.Count;
                    foreach (var t in Deploy.AlongLine(firstSpellUnit, line.Item1, line.Item2, count, count, 100))
                        yield return t;

                    line = AllInOnePushDeploy.SecondHasteLine;
                }
            }

            // Use freeze if inferno is found
            if (DeploymentMethods.freezeSpell?.Count > 0)
            {
                var infernos = InfernoTower.Find();
                // Find and watch inferno towers
                if (infernos != null)
                {
                    foreach (var inferno in infernos)
                    {
                        inferno.FirstActivated += DropFreeze;
                        inferno.StartWatching();
                    }
                }
            }

            if (DeploymentMethods.isWarden)
            {
                DeploymentMethods.warden.Select();
                DeploymentMethods.warden.Select();
            }

            yield return 4000;

            if (firstSpell?.Sum(u => u.Count) > 0)
            {
                foreach (var unit in firstSpell)
                {
                    var count = unit.Count >= 3 ? 3 : unit.Count;
                    foreach (var t in Deploy.AlongLine(unit, line.Item1, line.Item2, count, count, 50))
                        yield return t;
                }

                line = AllInOnePushDeploy.SecondRageLine;
            }

            if (secondSpell?.Sum(u => u.Count) > 0)
            {
                secondSpellUnit = secondSpell.FirstOrDefault().Count > 0 ? secondSpell.FirstOrDefault() : secondSpell.LastOrDefault();
                foreach (var unit in secondSpell)
                {
                    var count = unit.Count >= 3 ? 3 : unit.Count;
                    foreach (var t in Deploy.AlongLine(unit, line.Item1, line.Item2, count, count, 50))
                        yield return t;
                }

            }

            foreach (var t in DeploymentMethods.DeployWB())
                yield return t;

            yield return 1500;

            if (DeploymentMethods.heroes.Any())
            {
                foreach (var hero in DeploymentMethods.heroes.Where(u => u.Count > 0))
                {
                    foreach (var t in Deploy.AtPoint(hero, AllInOnePushDeploy.Origin))
                        yield return t;
                }
                Deploy.WatchHeroes(DeploymentMethods.heroes);
            }


            if (DeploymentMethods.queen?.Count > 0)
            {
                foreach (var t in Deploy.AtPoint(DeploymentMethods.queen, AllInOnePushDeploy.Origin))
                    yield return t;
                Deploy.WatchHeroes(new List<DeployElement> { DeploymentMethods.queen });
            }

            foreach (var w in DeploymentMethods.DeployUnusedTroops())
                yield return w;
        }

        private static void DropFreeze(object sender, EventArgs e)
        {
            var inferno = (InfernoTower)sender;

            foreach (var t in Deploy.AtPoint(DeploymentMethods.freezeSpell, inferno.Location.GetCenter()))
                Thread.Sleep(t);

            inferno.StopWatching();
        }
    }   
}
