﻿using System;
using System.Linq;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Master
{
    class LeeSin : Program
    {
        private const String Version = "1.1.0";
        private Obj_AI_Base allyObj = null;
        private SpellDataInst QData, WData, EData, RData;
        private Boolean TiamatReady = false, HydraReady = false, BladeReady = false, BilgeReady = false, RandReady = false;
        private InventorySlot Ward = null;
        private Int32 lastTimeWard = 0, lastTimeJump = 0;

        public LeeSin()
        {
            SkillQ = new Spell(QData.Slot, QData.SData.CastRange[0]);//1300
            SkillW = new Spell(WData.Slot, WData.SData.CastRange[0]);
            SkillE = new Spell(EData.Slot, EData.SData.CastRange[0]);//575
            SkillR = new Spell(RData.Slot, RData.SData.CastRange[0]);
            SkillQ.SetSkillshot(-QData.SData.SpellCastTime, QData.SData.LineWidth, QData.SData.MissileSpeed, true, SkillshotType.SkillshotLine);

            Config.AddSubMenu(new Menu("Key Bindings", "KeyBindings"));
            Config.SubMenu("KeyBindings").AddItem(new MenuItem(Name + "starActive", "Star Combo").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("KeyBindings").AddItem(new MenuItem(Name + "insecMake", "Insec").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("KeyBindings").AddItem(new MenuItem(Name + "wardJump", "Ward Jump").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("KeyBindings").AddItem(new MenuItem(Name + "ksbrdr", "Kill Steal Baron/Dragon").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Combo Settings", "csettings"));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "pusage", "Use Passive").SetValue(false));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "qusage", "Use Q").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "wusage", "Use W").SetValue(false));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "autowusage", "Use W If Hp Under").SetValue(new Slider(20, 1)));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "eusage", "Use E").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "rusage", "Use R To Finish").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "ignite", "Auto Ignite If Killable").SetValue(true));
            Config.SubMenu("csettings").AddItem(new MenuItem(Name + "iusage", "Use Item").SetValue(true));

            Config.AddSubMenu(new Menu("Insec Settings", "insettings"));
            //string[] allylist = ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsAlly && !i.IsMe).Select(i => i.ChampionName).ToArray();
            //Config.SubMenu("insettings").AddItem(new MenuItem(Name+ "insecally", "To:").SetValue(new StringList(allylist))).DontSave();
            Config.SubMenu("insettings").AddItem(new MenuItem(Name + "insectower", "To Tower If No Ally In").SetValue(new Slider(1100, 500, 1500)));
            Config.SubMenu("insettings").AddItem(new MenuItem(Name + "wflash", "Flash If Ward Jump Not Ready").SetValue(true));
            Config.SubMenu("insettings").AddItem(new MenuItem(Name + "pflash", "Prioritize Flash To Insec").SetValue(false));

            Config.AddSubMenu(new Menu("Harass Settings", "hsettings"));
            Config.SubMenu("hsettings").AddItem(new MenuItem(Name + "harMode", "Use Harass If Hp Above").SetValue(new Slider(20, 1)));
            Config.SubMenu("hsettings").AddItem(new MenuItem(Name + "useHarrE", "Use E").SetValue(true));

            Config.AddSubMenu(new Menu("Misc Settings", "miscs"));
            Config.SubMenu("miscs").AddItem(new MenuItem(Name + "smite", "Auto Smite Collision Minion").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem(Name + "skin", "Use Custom Skin").SetValue(true));
            Config.SubMenu("miscs").AddItem(new MenuItem(Name + "skin1", "Skin Changer").SetValue(new Slider(4, 1, 7)));
            Config.SubMenu("miscs").AddItem(new MenuItem(Name + "packetCast", "Use Packet To Cast").SetValue(true));

            Config.AddSubMenu(new Menu("Ultimate Settings", "useUlt"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy))
            {
                Config.SubMenu("useUlt").AddItem(new MenuItem(Name + "ult" + enemy.ChampionName, "Use Ultimate On " + enemy.ChampionName).SetValue(true));
            }

            Config.AddSubMenu(new Menu("Lane/Jungle Clear Settings", "LaneJungClear"));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem(Name + "useClearQ", "Use Q").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem(Name + "useClearW", "Use W").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem(Name + "useClearE", "Use E").SetValue(true));
            Config.SubMenu("LaneJungClear").AddItem(new MenuItem(Name + "useClearI", "Use Tiamat/Hydra Item").SetValue(true));

            Config.AddSubMenu(new Menu("Draw Settings", "DrawSettings"));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem(Name + "drawInsec", "Insec Line").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem(Name + "drawInsecTower", "Insec To Tower Range").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem(Name + "drawKillable", "Killable Text").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem(Name + "DrawQ", "Q Range").SetValue(false));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem(Name + "DrawW", "W Range").SetValue(true));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem(Name + "DrawE", "E Range").SetValue(false));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem(Name + "DrawR", "R Range").SetValue(false));

            if (Config.Item(Name + "skin").GetValue<bool>())
            {
                Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(Player.NetworkId, Config.Item(Name + "skin1").GetValue<Slider>().Value, Name)).Process();
                lastSkinId = Config.Item(Name + "skin1").GetValue<Slider>().Value;
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("<font color = \"#33CCCC\">Master of {0}</font> <font color = \"#fff8e7\">Brian v{1}</font>", Name, Version);
        }

        private void Orbwalk(Obj_AI_Base target = null)
        {
            Orbwalking.Orbwalk((target == null) ? SimpleTs.GetTarget(-1, SimpleTs.DamageType.Physical) : target, Game.CursorPos, Config.Item(Name + "ExtraWindup").GetValue<Slider>().Value, Config.Item(Name + "HoldPosRadius").GetValue<Slider>().Value);
        }

        private InventorySlot GetWardSlot()
        {
            Int32[] wardIds = { 3340, 3361, 3205, 3207, 3154, 3160, 2049, 2045, 2050, 2044 };
            InventorySlot warditem = null;
            foreach (var wardId in wardIds)
            {
                warditem = Player.InventoryItems.FirstOrDefault(i => i.Id == (ItemId)wardId);
                if (warditem != null && Player.Spellbook.Spells.First(i => (Int32)i.Slot == warditem.Slot + 4).State == SpellState.Ready) return warditem;
            }
            return warditem;
        }

        private Obj_AI_Base GetInsecAlly()
        {
            //Obj_AI_Base allyInsecObj = null, towerInsecObj = null;
            //foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget(Config.Item(Name + "insectower").GetValue<Slider>().Value, false) && !i.IsMe))
            //{
            //    if (allyInsecObj == null || Player.Distance(ally) < Player.Distance(allyInsecObj)) allyInsecObj = ally;
            //    if (ally.ChampionName == Config.Item(Name + "insecally").GetValue<StringList>().SList[Config.Item(Name + "insecally").GetValue<StringList>().SelectedIndex])
            //    {
            //        if (Player.Distance(ally) < Player.Distance(allyInsecObj)) allyInsecObj = ally;
            //    }
            //}
            //if (allyInsecObj == null)
            //{
            //    foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(i => i != null && i.IsValid && i.IsAlly && !i.IsDead))
            //    {
            //        if (towerInsecObj == null || Player.Distance(turret) < Player.Distance(towerInsecObj)) towerInsecObj = turret;
            //    }
            //}
            //return (allyInsecObj != null) ? allyInsecObj : towerInsecObj;

            Obj_AI_Base nearObj = null;
            if (ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(i => i.IsAlly && !i.IsDead && !i.IsMe && i.Distance(Player) < Config.Item(Name + "insectower").GetValue<Slider>().Value) != null)
            {
                nearObj = ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsAlly && !i.IsDead && !i.IsMe).OrderBy(i => i.Distance(Player)).First();
            }
            else
            {
                nearObj = ObjectManager.Get<Obj_AI_Turret>().Where(i => i.IsAlly && !i.IsDead).OrderBy(i => i.Distance(Player)).First();
            }
            return nearObj;
        }

        private void OnGameUpdate(EventArgs args)
        {
            FReady = (FData != null && FData.Slot != SpellSlot.Unknown && FData.State == SpellState.Ready);
            SReady = (SData != null && SData.Slot != SpellSlot.Unknown && SData.State == SpellState.Ready);
            IReady = (IData != null && IData.Slot != SpellSlot.Unknown && IData.State == SpellState.Ready);
            TiamatReady = Items.CanUseItem(Tiamat);
            HydraReady = Items.CanUseItem(Hydra);
            BladeReady = Items.CanUseItem(Blade);
            BilgeReady = Items.CanUseItem(Bilge);
            RandReady = Items.CanUseItem(Rand);
            if (Player.IsDead) return;
            var target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed && targetObj != null)
            {
                targetObj = null;
            }
            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                targetObj = target;
            }
            else if ((target.IsValidTarget() && targetObj == null) || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                targetObj = target;
            }
            PacketCast = Config.Item(Name + "packetCast").GetValue<bool>();
            Ward = GetWardSlot();
            allyObj = GetInsecAlly();
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    NormalCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneJungClear();
                    break;
            }
            if (Config.Item(Name + "insecMake").GetValue<KeyBind>().Active) InsecCombo();
            if (Config.Item(Name + "starActive").GetValue<KeyBind>().Active) StarCombo();
            if (Config.Item(Name + "wardJump").GetValue<KeyBind>().Active) WardJump(Game.CursorPos);
            if (Config.Item(Name + "ksbrdr").GetValue<KeyBind>().Active) KillStealBrDr();
            if (Config.Item(Name + "skin").GetValue<bool>() && Config.Item(Name + "skin1").GetValue<Slider>().Value != lastSkinId)
            {
                Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(Player.NetworkId, Config.Item(Name + "skin1").GetValue<Slider>().Value, Name)).Process();
                lastSkinId = Config.Item(Name + "skin1").GetValue<Slider>().Value;
            }
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Config.Item(Name + "DrawQ").GetValue<bool>() && SkillQ.Level > 0) Utility.DrawCircle(Player.Position, (QData.Name == "BlindMonkQOne") ? SkillQ.Range : 1300, SkillQ.IsReady() ? Color.Green : Color.Red);
            if (Config.Item(Name + "DrawW").GetValue<bool>() && SkillW.Level > 0) Utility.DrawCircle(Player.Position, SkillW.Range, SkillW.IsReady() ? Color.Green : Color.Red);
            if (Config.Item(Name + "DrawE").GetValue<bool>() && SkillE.Level > 0) Utility.DrawCircle(Player.Position, (EData.Name == "BlindMonkEOne") ? SkillE.Range : 575, SkillE.IsReady() ? Color.Green : Color.Red);
            if (Config.Item(Name + "DrawR").GetValue<bool>() && SkillR.Level > 0) Utility.DrawCircle(Player.Position, SkillR.Range, SkillR.IsReady() ? Color.Green : Color.Red);
            if (Config.Item(Name + "drawInsec").GetValue<bool>() && SkillR.IsReady())
            {
                Byte validTargets = 0;
                if (targetObj != null)
                {
                    Utility.DrawCircle(targetObj.Position, 70, Color.FromArgb(0, 204, 0));
                    validTargets += 1;
                }
                if (allyObj != null)
                {
                    Utility.DrawCircle(allyObj.Position, 70, Color.FromArgb(0, 204, 0));
                    validTargets += 1;
                }
                if (validTargets == 2)
                {
                    var posDraw = targetObj.Position + Vector3.Normalize(allyObj.Position - targetObj.Position) * 600;
                    Drawing.DrawLine(Drawing.WorldToScreen(targetObj.Position), Drawing.WorldToScreen(posDraw), 2, Color.White);
                }
            }
            if (Config.Item(Name + "drawInsecTower").GetValue<bool>() && SkillR.IsReady()) Utility.DrawCircle(Player.Position, Config.Item(Name + "insectower").GetValue<Slider>().Value, Color.Purple);
            if (Config.Item(Name + "drawKillable").GetValue<bool>())
            {
                foreach (var killableObj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsValidTarget()))
                {
                    var dmgTotal = Player.GetAutoAttackDamage(killableObj);
                    if (SkillQ.IsReady() && QData.Name == "BlindMonkQOne") dmgTotal += SkillQ.GetDamage(killableObj);
                    if (SkillR.IsReady() && Config.Item(Name + "ult" + killableObj.ChampionName).GetValue<bool>()) dmgTotal += SkillR.GetDamage(killableObj);
                    if (SkillE.IsReady() && EData.Name == "BlindMonkEOne") dmgTotal += SkillE.GetDamage(killableObj);
                    if (SkillQ.IsReady() && (killableObj.HasBuff("BlindMonkQOne", true) || killableObj.HasBuff("blindmonkqonechaos", true))) dmgTotal += GetQ2Dmg(killableObj, dmgTotal);
                    if (killableObj.Health < dmgTotal)
                    {
                        var posText = Drawing.WorldToScreen(killableObj.Position);
                        Drawing.DrawText(posText.X - 30, posText.Y - 5, Color.White, "Killable");
                    }
                }
            }
        }

        private bool CheckingCollision(Obj_AI_Hero target)
        {
            foreach (var col in MinionManager.GetMinions(Player.Position, 1500, MinionTypes.All, MinionTeam.NotAlly))
            {
                var Segment = Geometry.ProjectOn(col.Position.To2D(), Player.Position.To2D(), target.Position.To2D());
                if (Segment.IsOnSegment && col.Distance(Segment.SegmentPoint) <= col.BoundingRadius + SkillQ.Width)
                {
                    if (col.IsValidTarget(SData.SData.CastRange[0]) && col.Health < Player.GetSummonerSpellDamage(col, Damage.SummonerSpell.Smite))
                    {
                        Player.SummonerSpellbook.CastSpell(SData.Slot, col);
                        return true;
                    }
                }
            }
            return false;
        }

        private void KillStealBrDr()
        {
            var minionObj = MinionManager.GetMinions(Player.Position, 1500, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault(i => i.Name == "Worm12.1.1" || i.Name == "Dragon6.1.1");
            Orbwalk(minionObj);
            if (minionObj == null) return;
            if (SkillQ.IsReady() && !SReady && minionObj.Health - SkillQ.GetDamage(minionObj) < GetQ2Dmg(minionObj, SkillQ.GetDamage(minionObj)))
            {
                if (QData.Name == "BlindMonkQOne")
                {
                    SkillQ.Cast(minionObj, PacketCast);
                }
                else if ((minionObj.HasBuff("BlindMonkQOne", true) || minionObj.HasBuff("blindmonkqonechaos", true)) && minionObj.IsValidTarget(1300)) SkillQ.Cast();
            }
            if (SkillQ.IsReady() && SReady && minionObj.Health - (SkillQ.GetDamage(minionObj) + Player.GetSummonerSpellDamage(minionObj, Damage.SummonerSpell.Smite)) < GetQ2Dmg(minionObj, SkillQ.GetDamage(minionObj) + Player.GetSummonerSpellDamage(minionObj, Damage.SummonerSpell.Smite)))
            {
                if (QData.Name == "BlindMonkQOne")
                {
                    SkillQ.Cast(minionObj, PacketCast);
                }
                else if ((minionObj.HasBuff("BlindMonkQOne", true) || minionObj.HasBuff("blindmonkqonechaos", true)) && minionObj.IsValidTarget(1300))
                {
                    SkillQ.Cast();
                    Player.SummonerSpellbook.CastSpell(SData.Slot, minionObj);
                }
            }
            if (SReady && minionObj.IsValidTarget(SData.SData.CastRange[0]) && minionObj.Health < Player.GetSummonerSpellDamage(minionObj, Damage.SummonerSpell.Smite)) Player.SummonerSpellbook.CastSpell(SData.Slot, minionObj);
        }

        private void Harass()
        {
            if (targetObj == null) return;
            var jumpObj = ObjectManager.Get<Obj_AI_Base>().Where(i => !i.IsMe && !(i is Obj_AI_Turret) && i.Distance(Player.ServerPosition) <= SkillW.Range).OrderBy(i => i.Distance(ObjectManager.Get<Obj_AI_Turret>().Where(a => a.IsAlly).OrderBy(a => a.Distance(Player.ServerPosition)).First().ServerPosition)).FirstOrDefault();
            if (SkillQ.IsReady())
            {
                if (QData.Name == "BlindMonkQOne")
                {
                    if (Config.Item(Name + "smite").GetValue<bool>() && SReady && SkillQ.GetPrediction(targetObj).Hitchance == HitChance.Collision) CheckingCollision(targetObj);
                    SkillQ.Cast(targetObj, PacketCast);
                }
                else if ((targetObj.HasBuff("BlindMonkQOne", true) || targetObj.HasBuff("blindmonkqonechaos", true)) && targetObj.IsValidTarget(1300) && SkillW.IsReady() && WData.Name == "BlindMonkWOne" && Player.Mana >= (Config.Item(Name + "useHarrE").GetValue<bool>() ? 130 : 80) && (Player.Health * 100 / Player.MaxHealth) >= Config.Item(Name + "harMode").GetValue<Slider>().Value)
                {
                    if (jumpObj != null) SkillQ.Cast();
                }
            }
            if (!SkillQ.IsReady() && Config.Item(Name + "useHarrE").GetValue<bool>() && SkillE.IsReady() && EData.Name == "BlindMonkEOne" && targetObj.IsValidTarget(SkillE.Range)) SkillE.Cast();
            if (!SkillQ.IsReady() && SkillW.IsReady() && WData.Name == "BlindMonkWOne" && ((SkillE.Level == 0 && Utility.CountEnemysInRange(200) >= 1) || (Config.Item(Name + "useHarrE").GetValue<bool>() && targetObj.HasBuff("BlindMonkEOne", true)) || (!Config.Item(Name + "useHarrE").GetValue<bool>() && Utility.CountEnemysInRange(200) >= 1)))
            {
                if ((Environment.TickCount - lastTimeJump) > 200)
                {
                    SkillW.Cast(jumpObj, PacketCast);
                    lastTimeJump = Environment.TickCount;
                }
            }
        }

        private void WardJump(Vector3 Pos)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Pos);
            if ((SkillW.IsReady() && WData.Name != "BlindMonkWOne") || !SkillW.IsReady()) return;
            bool Jumped = false;
            if (Player.Distance(Pos) > SkillW.Range) Pos = Player.Position + Vector3.Normalize(Pos - Player.Position) * 600;
            foreach (var jumpObj in ObjectManager.Get<Obj_AI_Base>().Where(i => !i.IsMe && i.IsAlly && !(i is Obj_AI_Turret) && i.Distance(Pos) <= 200 && i.Distance(Player) <= SkillW.Range + i.BoundingRadius))
            {
                if ((Environment.TickCount - lastTimeJump) > 200)
                {
                    SkillW.Cast(jumpObj, PacketCast);
                    lastTimeJump = Environment.TickCount;
                    Jumped = true;
                }
            }
            if (!Jumped && Ward != null)
            {
                if ((Environment.TickCount - lastTimeWard) > 200)
                {
                    Ward.UseItem(Pos);
                    lastTimeWard = Environment.TickCount;
                }
            }
        }

        private void InsecCombo()
        {
            Orbwalk();
            if (targetObj == null || allyObj == null || !SkillR.IsReady()) return;
            if (!Config.Item(Name + "pflash").GetValue<bool>() && WardJumpInsec()) return;
            if (!Config.Item(Name + "pflash").GetValue<bool>() && Config.Item(Name + "wflash").GetValue<bool>() && FlashInsec()) return;
            if (Config.Item(Name + "pflash").GetValue<bool>() && FlashInsec()) return;
            if (Config.Item(Name + "pflash").GetValue<bool>() && !FReady && WardJumpInsec()) return;
            if (SkillQ.IsReady())
            {
                if (QData.Name == "BlindMonkQOne")
                {
                    if (Config.Item(Name + "smite").GetValue<bool>() && SReady && SkillQ.GetPrediction(targetObj).Hitchance == HitChance.Collision) CheckingCollision(targetObj);
                    SkillQ.Cast(targetObj, PacketCast);
                }
                else
                {
                    if ((targetObj.HasBuff("BlindMonkQOne", true) || targetObj.HasBuff("blindmonkqonechaos", true)) && targetObj.IsValidTarget(1300))
                    {
                        if (Player.Distance(targetObj) > 600 || targetObj.Health < SkillQ.GetDamage(targetObj, 1) || (Environment.TickCount - SkillQ.LastCastAttemptT) > 1800) SkillQ.Cast();
                    }
                    else
                    {
                        var enemyObj = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(i => i.IsValidTarget(1300) && i.Distance(targetObj) < 590 && (i.HasBuff("BlindMonkQOne", true) || i.HasBuff("blindmonkqonechaos", true)));
                        if (enemyObj != null && (Player.Distance(enemyObj) > 600 || (Environment.TickCount - SkillQ.LastCastAttemptT) > 1800)) SkillQ.Cast();
                    }
                }
            }
        }

        private bool WardJumpInsec()
        {
            if (targetObj.IsValidTarget(SkillR.Range))
            {
                var pos = Player.Position + Vector3.Normalize(targetObj.Position - Player.Position) * (targetObj.Distance(Player) + 500);
                var newDistance = allyObj.Distance(targetObj) - allyObj.Distance(pos);
                if (newDistance > 0 && (newDistance / 500) > 0.7)
                {
                    SkillR.Cast(targetObj, PacketCast);
                    return true;
                }
            }
            if (SkillW.IsReady() && WData.Name == "BlindMonkWOne")
            {
                var insecObj2 = Prediction.GetPrediction(targetObj, 0.25f, 2000).UnitPosition;
                var pos = allyObj.Position + Vector3.Normalize(insecObj2 - allyObj.Position) * (insecObj2.Distance(allyObj.Position) + 300);
                if (Player.Distance(pos) < 600)
                {
                    WardJump(pos);
                    return true;
                }
            }
            return false;
        }

        private bool FlashInsec()
        {
            if (targetObj.IsValidTarget(SkillR.Range))
            {
                var pos = Player.Position + Vector3.Normalize(targetObj.Position - Player.Position) * (targetObj.Distance(Player) + 500);
                var newDistance = allyObj.Distance(targetObj) - allyObj.Distance(pos);
                if (newDistance > 0 && (newDistance / 500) > 0.7)
                {
                    SkillR.Cast(targetObj, PacketCast);
                    return true;
                }
            }
            if (FReady)
            {
                var insecObj2 = Prediction.GetPrediction(targetObj, 0.25f, 2000).UnitPosition;
                var pos = allyObj.Position + Vector3.Normalize(insecObj2 - allyObj.Position) * (insecObj2.Distance(allyObj.Position) + 400);
                if (Player.Distance(pos) < 600)
                {
                    Player.SummonerSpellbook.CastSpell(FData.Slot, pos);
                    return true;
                }
            }
            return false;
        }

        private void NormalCombo()
        {
            if (targetObj == null) return;
            if (Config.Item(Name + "pusage").GetValue<bool>() && Player.HasBuff("blindmonkpassive_cosmetic", true) && Orbwalking.InAutoAttackRange(targetObj)) return;
            if (Config.Item(Name + "eusage").GetValue<bool>() && SkillE.IsReady() && EData.Name == "BlindMonkEOne" && targetObj.IsValidTarget(SkillE.Range)) SkillE.Cast();
            if (Config.Item(Name + "qusage").GetValue<bool>() && SkillQ.IsReady() && QData.Name == "BlindMonkQOne")
            {
                if (Config.Item(Name + "smite").GetValue<bool>() && SReady && SkillQ.GetPrediction(targetObj).Hitchance == HitChance.Collision) CheckingCollision(targetObj);
                SkillQ.Cast(targetObj, PacketCast);
            }
            if (Config.Item(Name + "qusage").GetValue<bool>() && SkillQ.IsReady() && targetObj.IsValidTarget(1300) && (targetObj.HasBuff("BlindMonkQOne", true) || targetObj.HasBuff("blindmonkqonechaos", true)))
            {
                if (Player.Distance(targetObj) > 500 || SkillQ.IsKillable(targetObj, 1) || (targetObj.HasBuff("BlindMonkEOne", true) && targetObj.IsValidTarget(SkillE.Range)) || (Environment.TickCount - SkillQ.LastCastAttemptT) > 1500) SkillQ.Cast();
            }
            if (Config.Item(Name + "eusage").GetValue<bool>() && SkillE.IsReady() && targetObj.IsValidTarget(575) && targetObj.HasBuff("BlindMonkEOne", true))
            {
                if (Player.Distance(targetObj) > 450 || (Environment.TickCount - SkillE.LastCastAttemptT) > 1500) SkillE.Cast();
            }
            if (Config.Item(Name + "rusage").GetValue<bool>() && Config.Item(Name + "ult" + targetObj.ChampionName).GetValue<bool>() && SkillR.IsReady() && targetObj.IsValidTarget(SkillR.Range))
            {
                if (SkillR.IsKillable(targetObj) || (targetObj.Health - SkillR.GetDamage(targetObj) < GetQ2Dmg(targetObj, SkillR.GetDamage(targetObj)) && SkillQ.IsReady() && (targetObj.HasBuff("BlindMonkQOne", true) || targetObj.HasBuff("blindmonkqonechaos", true)) && Player.Mana >= 50)) SkillR.Cast(targetObj, PacketCast);
            }
            if (Config.Item(Name + "wusage").GetValue<bool>() && SkillW.IsReady() && targetObj.IsValidTarget(SkillE.Range) && (Player.Health * 100 / Player.MaxHealth) <= Config.Item(Name + "autowusage").GetValue<Slider>().Value)
            {
                if (WData.Name == "BlindMonkWOne")
                {
                    SkillW.Cast();
                }
                else if (!Player.HasBuff("blindmonkwoneshield", true)) SkillW.Cast();
            }
            if (Config.Item(Name + "iusage").GetValue<bool>()) UseItem(targetObj);
            if (Config.Item(Name + "ignite").GetValue<bool>()) CastIgnite(targetObj);
        }

        private void StarCombo()
        {
            Orbwalk();
            if (targetObj == null) return;
            if (SkillE.IsReady() && EData.Name == "BlindMonkEOne" && targetObj.IsValidTarget(SkillE.Range)) SkillE.Cast();
            if (SkillQ.IsReady() && QData.Name == "BlindMonkQOne")
            {
                if (Config.Item(Name + "smite").GetValue<bool>() && SReady && SkillQ.GetPrediction(targetObj).Hitchance == HitChance.Collision) CheckingCollision(targetObj);
                SkillQ.Cast(targetObj, PacketCast);
            }
            if (!targetObj.IsValidTarget(SkillR.Range) && SkillR.IsReady() && SkillQ.IsReady() && (targetObj.HasBuff("BlindMonkQOne", true) || targetObj.HasBuff("blindmonkqonechaos", true)) && targetObj.IsValidTarget(SkillW.Range)) WardJump(targetObj.Position);
            UseItem(targetObj);
            if (SkillR.IsReady() && SkillQ.IsReady() && (targetObj.HasBuff("BlindMonkQOne", true) || targetObj.HasBuff("blindmonkqonechaos", true)) && targetObj.IsValidTarget(SkillR.Range) && Player.Mana >= 50) SkillR.Cast(targetObj, PacketCast);
            if (!SkillR.IsReady() && SkillQ.IsReady() && targetObj.IsValidTarget(1300) && (targetObj.HasBuff("BlindMonkQOne", true) || targetObj.HasBuff("blindmonkqonechaos", true)) && (Environment.TickCount - SkillR.LastCastAttemptT) > 500) SkillQ.Cast();
            if (!SkillR.IsReady() && SkillE.IsReady() && targetObj.IsValidTarget(575) && targetObj.HasBuff("BlindMonkEOne", true) && (Player.Distance(targetObj) > 450 || (Environment.TickCount - SkillE.LastCastAttemptT) > 1500)) SkillE.Cast();
            CastIgnite(targetObj);
        }

        private void CastIgnite(Obj_AI_Hero target)
        {
            if (IReady && target.IsValidTarget(IData.SData.CastRange[0]) && target.Health < Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite)) Player.SummonerSpellbook.CastSpell(IData.Slot, target);
        }

        private void UseItem(Obj_AI_Hero target)
        {
            if (BilgeReady && Player.Distance(target) <= 450) Items.UseItem(Bilge, target);
            if (BladeReady && Player.Distance(target) <= 450) Items.UseItem(Blade, target);
            if (TiamatReady && Utility.CountEnemysInRange(350) >= 1) Items.UseItem(Tiamat);
            if (HydraReady && (Utility.CountEnemysInRange(350) >= 2 || (Player.GetAutoAttackDamage(target) < target.Health && Utility.CountEnemysInRange(350) == 1))) Items.UseItem(Hydra);
            if (RandReady && Utility.CountEnemysInRange(450) >= 1) Items.UseItem(Rand);
        }

        private double GetQ2Dmg(Obj_AI_Base target, double dmgPlus)
        {
            Int32[] dmgQ = { 50, 80, 110, 140, 170 };
            return Player.CalcDamage(target, Damage.DamageType.Physical, dmgQ[SkillQ.Level - 1] + 0.9 * Player.FlatPhysicalDamageMod + 0.08 * (target.MaxHealth - (target.Health - dmgPlus)));
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Player.Position, SkillQ.Range, MinionTypes.All, MinionTeam.NotAlly).OrderBy(i => i.Distance(Player)).FirstOrDefault();
            if (minionObj == null) return;
            var Passive = Player.HasBuff("blindmonkpassive_cosmetic", true);
            if (Config.Item(Name + "useClearW").GetValue<bool>() && SkillW.IsReady() && Orbwalking.InAutoAttackRange(minionObj))
            {
                if (WData.Name == "BlindMonkWOne")
                {
                    if (!Passive) SkillW.Cast();
                }
                else if (!Passive || (Environment.TickCount - SkillW.LastCastAttemptT) > 2500 || !Player.HasBuff("blindmonkwoneshield", true)) SkillW.Cast();
            }
            if (Config.Item(Name + "useClearQ").GetValue<bool>() && SkillQ.IsReady())
            {
                if (QData.Name == "BlindMonkQOne")
                {
                    if (!Passive) SkillQ.Cast(minionObj, PacketCast);
                }
                else if ((minionObj.HasBuff("BlindMonkQOne", true) || minionObj.HasBuff("blindmonkqonechaos", true)) && (!Passive || SkillQ.IsKillable(minionObj, 1) || (Environment.TickCount - SkillQ.LastCastAttemptT) > 2500 || Player.Distance(minionObj) > 500)) SkillQ.Cast();
            }
            if (Config.Item(Name + "useClearE").GetValue<bool>() && SkillE.IsReady() && minionObj.IsValidTarget(SkillE.Range))
            {
                if (EData.Name == "BlindMonkEOne")
                {
                    if (!Passive) SkillE.Cast();
                }
                else if (minionObj.HasBuff("BlindMonkEOne", true) && (!Passive || (Environment.TickCount - SkillE.LastCastAttemptT) > 2500 || Player.Distance(minionObj) > 450)) SkillE.Cast();
            }
            if (Config.Item(Name + "useClearI").GetValue<bool>() && Player.Distance(minionObj) <= 350)
            {
                if (TiamatReady) Items.UseItem(Tiamat);
                if (HydraReady) Items.UseItem(Hydra);
            }
        }
    }
}