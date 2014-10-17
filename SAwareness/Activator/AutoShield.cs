﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAwareness
{
    internal class AutoShield
    {
        private readonly Shield _shield;

        public AutoShield()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Janna":
                    _shield = new Shield(new Spell(SpellSlot.E, 900, 0, 0, 0));
                    break;

                case "Morgana":
                    _shield = new Shield(new Spell(SpellSlot.E, 850, 0, 0, 0), true, false, true);
                    break;

                case "Lux":
                    _shield = new Shield(new Spell(SpellSlot.W, 1175, 0.5f, 150, 1200), false, true);
                    break;

                case "Orianna":
                    _shield = new Shield(new Spell(SpellSlot.E, 1295, 0.5f, 0, 1200), false);
                    break;

                case "Karma":
                    _shield = new Shield(new Spell(SpellSlot.E, 900, 0, 0, 0));
                    break;

                case "Lulu":
                    _shield = new Shield(new Spell(SpellSlot.E, 750, 0, 0, 0));
                    break;

                case "LeeSin":
                    _shield = new Shield(new Spell(SpellSlot.W, 800, 0, 0, 1500), false);
                    break;

                case "Thresh":
                    _shield = new Shield(new Spell(SpellSlot.W, 1050, 0, 0, 0));
                    break;

                    //Self

                case "JarvanIV":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Nautilus":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Rumble":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Sion":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Shen":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Skarner":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Urgot":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Diana":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Udyr":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true);
                    break;

                case "Riven":
                    _shield = new Shield(new Spell(SpellSlot.E, 0, 0, 0, 0), true);
                    break;

                case "Sivir":
                    _shield = new Shield(new Spell(SpellSlot.E, 0, 0, 0, 0), true, false, true, true);
                    break;

                case "Nocturne":
                    _shield = new Shield(new Spell(SpellSlot.W, 0, 0, 0, 0), true, false, true, true);
                    break;

                default:
                    return;
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        ~AutoShield()
        {
            Game.OnGameUpdate -= Game_OnGameUpdate;
        }

        public bool IsActive()
        {
            return Menu.Activator.GetActive() && Menu.AutoShield.GetActive();
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;

            var tempDamages =
                new Dictionary<Obj_AI_Hero, List<Activator.IncomingDamage>>(Activator.Damages);
            foreach (var damage in Activator.Damages)
            {
                Obj_AI_Hero hero = damage.Key;

                foreach (Activator.IncomingDamage tDamage in tempDamages[hero].ToArray())
                {
                    foreach (Database.Spell spell in Database.GetSpellList())
                    {
                        if (spell.Name.Contains(tDamage.SpellName))
                        {
                            if (_shield.OnlyMagic)
                            {
                                if (
                                    !IsDamageType((Obj_AI_Hero) tDamage.Source, tDamage.SpellName,
                                        Damage.DamageType.Magical))
                                {
                                    tempDamages[hero].Remove(tDamage);
                                    continue;
                                }
                                if (
                                    Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockCC")
                                        .GetValue<bool>() &&
                                    !ContainsCc(tDamage.SpellName))
                                {
                                    tempDamages[hero].Remove(tDamage);
                                    continue;
                                }
                            }
                            if (!CheckDamagelevel(tDamage.SpellName) && !_shield.OnlyMagic)
                            {
                                tempDamages[hero].Remove(tDamage);
                                continue;
                            }
                        }
                        if (!Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockAA")
                            .GetValue<bool>() && IsAutoAttack(tDamage.SpellName))
                        {
                            tempDamages[hero].Remove(tDamage);
                        }
                    }
                }
            }

            foreach (var damage in tempDamages)
            {
                //Vector2 d2 = Drawing.WorldToScreen(damage.Key.ServerPosition);
                //Drawing.DrawText(d2.X, d2.Y, System.Drawing.Color.Aquamarine, Activator.CalcMaxDamage(damage.Key).ToString());

                if (Activator.CalcMaxDamage(damage.Key) > 0 &&
                    (_shield.OnlySelf || damage.Key.Distance(ObjectManager.Player.ServerPosition) < _shield.Spell.Range))
                {
                    if (_shield.Skillshot)
                    {
                        PredictionOutput predOutput = _shield.Spell.GetPrediction(damage.Key);
                        if (predOutput.Hitchance > HitChance.Medium)
                            ObjectManager.Player.Spellbook.CastSpell(_shield.Spell.Slot, predOutput.CastPosition);
                        break;
                    }
                    if (_shield.OnlySelf && damage.Key.NetworkId == ObjectManager.Player.NetworkId)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_shield.Spell.Slot);
                        break;
                    }
                    if (_shield.Instant)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_shield.Spell.Slot, damage.Key);
                        break;
                    }
                }
            }
        }

        private static bool ContainsCc(String spellName)
        {
            foreach (Database.Spell spell in Database.GetSpellList())
            {
                if (spellName.Contains(spell.Name))
                {
                    if (spell.CcType != Database.Spell.CCtype.NoCc)
                        return true;
                }
            }
            return false;
        }

        private static bool CheckDamagelevel(String spellName)
        {
            foreach (Database.Spell spell in Database.GetSpellList())
            {
                if (spell.Name.Contains(spellName))
                {
                    if (
                        Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockDamageAmount")
                            .GetValue<StringList>()
                            .SelectedIndex == 0)
                    {
                        if (spell.Damagelvl == Database.Spell.DamageLevel.Medium ||
                            spell.Damagelvl == Database.Spell.DamageLevel.High ||
                            spell.Damagelvl == Database.Spell.DamageLevel.Extrem)
                            return true;
                    }
                    else if (
                        Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockDamageAmount")
                            .GetValue<StringList>()
                            .SelectedIndex == 1)
                    {
                        if (spell.Damagelvl == Database.Spell.DamageLevel.High ||
                            spell.Damagelvl == Database.Spell.DamageLevel.Extrem)
                            return true;
                    }
                    if (
                        Menu.AutoShield.GetMenuItem("SAwarenessAutoShieldBlockDamageAmount")
                            .GetValue<StringList>()
                            .SelectedIndex == 2)
                    {
                        if (spell.Damagelvl == Database.Spell.DamageLevel.Extrem)
                            return true;
                    }
                }
            }
            return false;
        }

        private static bool IsAutoAttack(String spellName)
        {
            if (spellName.ToLower().Contains("attack"))
                return true;
            return false;
        }

        private static bool IsDamageType(Obj_AI_Hero hero, String spellName, Damage.DamageType damageType)
        {
            DamageSpell damageSpell = null;
            foreach (SpellDataInst spellDataInst in hero.Spellbook.Spells)
            {
                if (string.Equals(spellDataInst.Name, spellName,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    damageSpell = Damage.Spells[hero.ChampionName].FirstOrDefault(s =>
                    {
                        if (s.Slot == spellDataInst.Slot)
                            return 0 == s.Stage;
                        return false;
                    }) ?? Damage.Spells[hero.ChampionName].FirstOrDefault(s => s.Slot == spellDataInst.Slot);
                    if (damageSpell != null)
                        break;
                }
            }
            if (damageSpell == null || damageSpell.DamageType != damageType)
                return false;
            return true;
        }

        private class Shield
        {
            public readonly bool Instant;
            public readonly bool OnlyMagic;
            public readonly bool OnlySelf;
            public readonly bool Skillshot;
            public readonly Spell Spell;

            public Shield(Spell spell, bool instant = true, bool skillshot = false, bool onlyMagic = false,
                bool onlySelf = false)
            {
                Spell = spell;
                Instant = instant;
                Skillshot = skillshot;
                OnlyMagic = onlyMagic;
                OnlySelf = onlySelf;
            }

            public Shield(Spell spell, bool onlySelf = false)
            {
                Spell = spell;
                Instant = true;
                Skillshot = false;
                OnlyMagic = false;
                OnlySelf = onlySelf;
            }

            public Shield(Spell spell)
            {
                Spell = spell;
                Instant = true;
                Skillshot = false;
                OnlyMagic = false;
                OnlySelf = false;
            }
        }

        private class Spell : LeagueSharp.Common.Spell
        {
            public Spell(SpellSlot slot, float range, float delay = 0, float width = 0, float speed = 0)
                : base(slot, range)
            {
                SetSkillshot(delay, width, speed, false, SkillshotType.SkillshotLine);
            }
        }
    }
}