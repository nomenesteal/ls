using System;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
namespace D_Elise
{
    class Program
    {
        private const string ChampionName = "Elise";

        private static Orbwalking.Orbwalker _orbwalker;

        private static bool _human;

        private static bool _spider;

        private static Spell _humanQ, _humanW, _humanE, _r, _spiderQ, _spiderW, _spiderE;

        private static Menu _config;

        private static SpellSlot _igniteSlot;

        private static Obj_AI_Hero _player;

        private static readonly float[] HumanQcd = { 6, 6, 6, 6, 6 };

        private static readonly float[] HumanWcd = { 12, 12, 12, 12, 12 };

        private static readonly float[] HumanEcd = { 14, 13, 12, 11, 10 };

        private static readonly float[] SpiderQcd = { 6, 6, 6, 6, 6 };

        private static readonly float[] SpiderWcd = { 12, 12, 12, 12, 12 };

        private static readonly float[] SpiderEcd = { 26, 23, 20, 17, 14 };

        private static float _humQcd = 0, _humWcd = 0, _humEcd = 0;

        private static float _spidQcd = 0, _spidWcd = 0, _spidEcd = 0;

        private static float _humaQcd = 0, _humaWcd = 0, _humaEcd = 0;

        private static float _spideQcd = 0, _spideWcd = 0, _spideEcd = 0;

        private static SpellDataInst _smiteSlot;

       
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {

            _player = ObjectManager.Player;
            if (_player.BaseSkinName != ChampionName) return;

            _humanQ = new Spell(SpellSlot.Q, 625f);
            _humanW = new Spell(SpellSlot.W, 950f);
            _humanE = new Spell(SpellSlot.E, 1075f);
            _spiderQ = new Spell(SpellSlot.Q, 475f);
            _spiderW = new Spell(SpellSlot.W, 0);
            _spiderE = new Spell(SpellSlot.E, 750f);
            _r = new Spell(SpellSlot.R, 0);

            _humanW.SetSkillshot(0.25f, 100f, 1000, true, SkillshotType.SkillshotLine);
            _humanE.SetSkillshot(0.25f, 55f, 1300, true, SkillshotType.SkillshotLine);

            //DFG = new Items.Item(3128, 750f);

            _igniteSlot = _player.GetSpellSlot("SummonerDot");
            _smiteSlot = _player.SummonerSpellbook.GetSpell(_player.GetSpellSlot("summonersmite"));
            

            _config = new Menu("D-Elise", "D-Elise", true);


            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseHumanQ", "Human Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseHumanW", "Human W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseHumanE", "Human E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Auto use R")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseSpiderQ", "Spider Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseSpiderW", "Spider W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseSpiderE", "Spider E")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Human Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Human W")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            _config.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddItem(new MenuItem("HumanQFarm", "Human Q")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("HumanWFarm", "Human W")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("SpiderQFarm", "Spider Q")).SetValue(false);
            _config.SubMenu("Farm").AddItem(new MenuItem("SpiderWFarm", "Spider W")).SetValue(true);
            _config.SubMenu("Farm").AddItem(new MenuItem("Farm_R", "Auto Switch(toggle)").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("Farm").AddItem(new MenuItem("ActiveFreeze", "Freeze Lane").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm").AddItem(new MenuItem("ClearActive", "Clear Lane").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm").AddItem(new MenuItem("Lanemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //Farm
            _config.AddSubMenu(new Menu("Jungle", "Jungle"));
            _config.SubMenu("Jungle").AddItem(new MenuItem("HumanQFarmJ", "Human Q")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("HumanWFarmJ", "Human W")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("SpiderQFarmJ", "Spider Q")).SetValue(false);
            _config.SubMenu("Jungle").AddItem(new MenuItem("SpiderWFarmJ", "Spider W")).SetValue(true);
            _config.SubMenu("Jungle").AddItem(new MenuItem("ActiveJungle", "Jungle").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Jungle").AddItem(new MenuItem("Junglemana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));

            //misc
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "Use Packets")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Spidergapcloser", "SpiderE to GapCloser")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("Humangapcloser", "HumanE to GapCloser")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("UseEInt", "HumanE to Interrupt")).SetValue(true);
            _config.SubMenu("Misc").AddItem(new MenuItem("smite", "Auto Smite Minion").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("autoE", "HUmanE with VeryHigh Use").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Misc")
                                   .AddItem(new MenuItem("Echange", "E Hit").SetValue(
                                    new StringList(new[] { "Low", "Medium", "High", "Very High" })));



            //Kill Steal
            _config.AddSubMenu(new Menu("KillSteal", "Ks"));
            _config.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("HumanQKs", "Human Q")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("HumanWKs", "Human W")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("SpiderQKs", "Spider Q")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);


            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Human Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Human W")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Human E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("SpiderDrawQ", "Spider Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("SpiderDrawE", "Spider E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#881df2'>D-Elise by Diabaths</font> Loaded.");

        }



        private static void Game_OnGameUpdate(EventArgs args)
        {
            Cooldowns();
           
            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);

            CheckSpells();
           
            if (_config.Item("ActiveFreeze").GetValue<KeyBind>().Active ||
                _config.Item("ClearActive").GetValue<KeyBind>().Active)

                FarmLane();

            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active)
            {
                JungleFarm();

            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (_config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass();

            }
            if (_config.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
            if (_config.Item("autoE").GetValue<KeyBind>().Active)
            {
                AutoE();

            }
       }
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                //Game.PrintChat("Spell name: " + args.SData.Name.ToString());
                GetCDs(args);
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(_humanW.Range, SimpleTs.DamageType.Magical);
            var sReady = (_smiteSlot != null && _smiteSlot.Slot != SpellSlot.Unknown && _smiteSlot.State == SpellState.Ready);
            var qdmg = _player.GetSpellDamage(target, SpellSlot.Q);
            var wdmg = _player.GetSpellDamage(target, SpellSlot.W);
            if (target == null) return; //buffelisecocoon
            if (_human)
            {
                if (target.Distance(_player.Position) < _humanE.Range && _config.Item("UseHumanE").GetValue<bool>() && _humanE.IsReady())
                {
                    if (sReady && _config.Item("smite").GetValue<bool>() &&
                        _humanE.GetPrediction(target).CollisionObjects.Count == 1)
                    {
                        CheckingCollision(target);
                        _humanE.Cast(target, Packets());
                    }
                    else if (_humanE.GetPrediction(target).Hitchance >= Echange())
                    {
                        _humanE.Cast(target, Packets());
                    }
                }

                if (_player.Distance(target) <= _humanQ.Range && _config.Item("UseHumanQ").GetValue<bool>() && _humanQ.IsReady())
                {
                    _humanQ.Cast(target, Packets());
                }
                if (_player.Distance(target) <= _humanW.Range && _config.Item("UseHumanW").GetValue<bool>() && _humanW.IsReady())
                {
                    _humanW.Cast(target, Packets());
                }
                if (!_humanQ.IsReady() && !_humanW.IsReady() && !_humanE.IsReady() && _config.Item("UseRCombo").GetValue<bool>() && _r.IsReady())
                {
                    _r.Cast();
                }
                if (!_humanQ.IsReady() && !_humanW.IsReady()  && _player.Distance(target) <= _spiderQ.Range && _config.Item("UseRCombo").GetValue<bool>() && _r.IsReady())
                {
                    _r.Cast();
                }
            }
            if (!_spider) return;
            if (_player.Distance(target) <= _spiderQ.Range && _config.Item("UseSpiderQ").GetValue<bool>() && _spiderQ.IsReady())
            {
                _spiderQ.Cast(target, Packets());
            }
            if (_player.Distance(target) <= 200 && _config.Item("UseSpiderW").GetValue<bool>() && _spiderW.IsReady())
            {
                _spiderW.Cast();
            }
            if (_player.Distance(target) <= _spiderE.Range && _player.Distance(target) > _spiderQ.Range && _config.Item("UseSpiderE").GetValue<bool>() && _spiderE.IsReady() && !_spiderQ.IsReady())
            {
                _spiderE.Cast(target, Packets());
            }
            if (_player.Distance(target) > _spiderQ.Range && !_spiderE.IsReady() && _r.IsReady() && !_spiderQ.IsReady() && _config.Item("UseRCombo").GetValue<bool>())
            {
                _r.Cast();
            }
            if (_humanQ.IsReady() && _humanW.IsReady() && _r.IsReady() && _config.Item("UseRCombo").GetValue<bool>())
            {
                _r.Cast();
            }
            if (_humanQ.IsReady() && _humanW.IsReady() && _r.IsReady() && _config.Item("UseRCombo").GetValue<bool>())
            {
                _r.Cast();
            }
            if ((_humanQ.IsReady() && qdmg >= target.Health  || _humanW.IsReady() && wdmg >= target.Health) && _config.Item("UseRCombo").GetValue<bool>())
            {
                _r.Cast();
            }
        }

        private static void Harass()
        {
            var target = SimpleTs.GetTarget(_humanQ.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {

                if (_human && _player.Distance(target) <= _humanQ.Range && _config.Item("UseQHarass").GetValue<bool>() && _humanQ.IsReady())
                {
                    _humanQ.Cast(target, Packets());
                }

                if (_human && _player.Distance(target) <= _humanW.Range && _config.Item("UseWHarass").GetValue<bool>() && _humanW.IsReady())
                {
                    _humanW.Cast(target, Packets());
                }
            }
        }

        private static void JungleFarm()
        {
            var jungleQ = (_config.Item("HumanQFarmJ").GetValue<bool>() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Junglemana").GetValue<Slider>().Value);
            var jungleW = (_config.Item("HumanWFarmJ").GetValue<bool>() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Junglemana").GetValue<Slider>().Value);
            var spiderjungleQ = _config.Item("SpiderQFarmJ").GetValue<bool>();
            var spiderjungleW = _config.Item("SpiderWFarmJ").GetValue<bool>();
            var switchR = (100*(_player.Mana/_player.MaxMana)) < _config.Item("Junglemana").GetValue<Slider>().Value;
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _humanQ.Range,
            MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                foreach (var minion in mobs)
                    if (_human)
                    {
                        if (jungleQ && _humanQ.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _humanQ.Range)
                        {
                            _humanQ.Cast(minion, Packets());
                        }
                        if (jungleW && _humanW.IsReady() && !_humanQ.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _humanW.Range)
                        {
                            _humanW.Cast(minion, Packets());
                        }
                        if ((!_humanQ.IsReady() && !_humanW.IsReady()) || switchR)
                        {
                            _r.Cast();
                        }
                    }
                foreach (var minion in mobs)
                {
                    if (_spider)
                    {
                        if (spiderjungleQ && _spiderQ.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _spiderQ.Range)
                        {
                            _spiderQ.Cast(minion, Packets());
                        }
                        if (spiderjungleW && _spiderW.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= 150)
                        {
                            _orbwalker.SetAttack(true);
                            _spiderW.Cast();
                        }
                        if (_r.IsReady() && _humanQ.IsReady() && !_spiderQ.IsReady() && !_spiderW.IsReady() && _spider)
                        {
                            _r.Cast();
                        }
                    }
                }
            }
        }

        private static void FarmLane()
        {
            var ManaUse = (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value;
            var useR = _config.Item("Farm_R").GetValue<KeyBind>().Active;
            var useHumQ = (_config.Item("HumanQFarm").GetValue<bool>() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value);
            var useHumW = (_config.Item("HumanWFarm").GetValue<bool>() && (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Lanemana").GetValue<Slider>().Value);
            var useSpiQFarm = (_spiderQ.IsReady() && _config.Item("SpiderQFarm").GetValue<bool>());
            var useSpiWFarm = (_spiderW.IsReady() && _config.Item("SpiderWFarm").GetValue<bool>());
            var allminions = MinionManager.GetMinions(_player.ServerPosition, _humanQ.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            {
                if (_config.Item("ClearActive").GetValue<KeyBind>().Active)
                {
                    foreach (var minion in allminions)
                        if (_human)
                        {
                            if (useHumQ && _humanQ.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _humanQ.Range)
                            {
                                _humanQ.Cast(minion);
                            }
                            if (useHumW && _humanW.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _humanW.Range)
                            {
                                _humanW.Cast(minion);
                            }
                            if (useR && _r.IsReady())
                            {
                                _r.Cast();
                            }
                        }
                    foreach (var minion in allminions)
                        if (_spider)
                        {
                            if (useSpiQFarm && _spiderQ.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _spiderQ.Range)
                            {
                                _spiderQ.Cast(minion);
                            }
                            if (useSpiWFarm && _spiderW.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= 125)
                            {
                                _spiderW.Cast();
                            }
                        }
                }
                if (_config.Item("ActiveFreeze").GetValue<KeyBind>().Active)
                {
                    foreach (var minion in allminions)
                        if (_human)
                        {
                            if (useHumQ && _player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health && _humanQ.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _humanQ.Range)
                            {
                                _humanQ.Cast(minion);
                            }
                            if (useHumW && _player.GetSpellDamage(minion, SpellSlot.W) > minion.Health && _humanW.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _humanW.Range)
                            {
                                _humanW.Cast(minion);
                            }
                            if (useR && _r.IsReady())
                            {
                                _r.Cast();
                            }
                        }
                    foreach (var minion in allminions)
                        if (_spider)
                        {
                            if (useSpiQFarm && _spiderQ.IsReady() && _player.GetSpellDamage(minion, SpellSlot.Q) > minion.Health && _spiderQ.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= _spiderQ.Range)
                            {
                                _spiderQ.Cast(minion);
                            }
                            if (useSpiQFarm && _spiderW.IsReady() && minion.IsValidTarget() && _player.Distance(minion) <= 125)
                            {
                                _spiderW.Cast();
                            }
                        }
                }
            }
        }
        private static void AutoE()
        {
            var target = SimpleTs.GetTarget(_humanE.Range, SimpleTs.DamageType.Magical);
            
            if (_human && _player.Distance(target) < _humanE.Range && _humanE.IsReady() && _humanE.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
            {
                _humanE.Cast(target, Packets());
            }
        }
        private static bool Packets()
        {
            return _config.Item("usePackets").GetValue<bool>();
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base target, InterruptableSpell spell)
        {
            if (!_config.Item("UseEInt").GetValue<bool>()) return;
            if (_player.Distance(target) < _humanE.Range && target != null && _humanE.GetPrediction(target).Hitchance >= HitChance.Low)
            {
                _humanE.Cast(target, Packets());
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_spiderE.IsReady() && _spider && gapcloser.Sender.IsValidTarget(_spiderE.Range) && _config.Item("Spidergapcloser").GetValue<bool>())
            {
                _spiderE.Cast(gapcloser.Sender, Packets());
            }
            if (_humanE.IsReady() && _human && gapcloser.Sender.IsValidTarget(_humanE.Range) && _config.Item("Humangapcloser").GetValue<bool>())
            {
                _humanE.Cast(gapcloser.Sender, Packets());
            }
        }

        private static float CalculateCd(float time)
        {
            return time + (time * _player.PercentCooldownMod);
        }

        private static void Cooldowns()
        {
            _humaQcd = ((_humQcd - Game.Time) > 0) ? (_humQcd - Game.Time) : 0;
            _humaWcd = ((_humWcd - Game.Time) > 0) ? (_humWcd - Game.Time) : 0;
            _humaEcd = ((_humEcd - Game.Time) > 0) ? (_humEcd - Game.Time) : 0;
            _spideQcd = ((_spidQcd - Game.Time) > 0) ? (_spidQcd - Game.Time) : 0;
            _spideWcd = ((_spidWcd - Game.Time) > 0) ? (_spidWcd - Game.Time) : 0;
            _spideEcd = ((_spidEcd - Game.Time) > 0) ? (_spidEcd - Game.Time) : 0;
        }

        private static void GetCDs(GameObjectProcessSpellCastEventArgs spell)
        {
            if (_human)
            {
                if (spell.SData.Name == "EliseHumanQ")
                    _humQcd = Game.Time + CalculateCd(HumanQcd[_humanQ.Level]);
                if (spell.SData.Name == "EliseHumanW")
                    _humWcd = Game.Time + CalculateCd(HumanWcd[_humanW.Level]);
                if (spell.SData.Name == "EliseHumanE")
                    _humEcd = Game.Time + CalculateCd(HumanEcd[_humanE.Level]);
            }
            else
            {
                if (spell.SData.Name == "EliseSpiderQCast")
                    _spidQcd = Game.Time + CalculateCd(SpiderQcd[_spiderQ.Level]);
                if (spell.SData.Name == "EliseSpiderW")
                    _spidWcd = Game.Time + CalculateCd(SpiderWcd[_spiderW.Level]);
                if (spell.SData.Name == "EliseSpiderEInitial")
                    _spidEcd = Game.Time + CalculateCd(SpiderEcd[_spiderE.Level]);
            }
        }

        private static HitChance Echange()
        {
            switch (_config.Item("Echange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }
        // Credits to Brain0305
        private static bool CheckingCollision(Obj_AI_Hero target)
        {
            foreach (var col in MinionManager.GetMinions(_player.Position, 1500, MinionTypes.All, MinionTeam.NotAlly))
            {
                var segment = Geometry.ProjectOn(col.ServerPosition.To2D(), _player.ServerPosition.To2D(),
                    col.Position.To2D());
                if (segment.IsOnSegment &&
                    target.ServerPosition.To2D().Distance(segment.SegmentPoint) <= GetHitBox(col) + 40)
                {
                    if (col.IsValidTarget(_smiteSlot.SData.CastRange[0]) &&
                        col.Health < _player.GetSummonerSpellDamage(col, Damage.SummonerSpell.Smite))
                    {
                        _player.SummonerSpellbook.CastSpell(_smiteSlot.Slot, col);
                        return true;
                    }
                }
            }
            return false;
        }
        // Credits to Brain0305
        static float GetHitBox(Obj_AI_Base minion)
        {
            var nameMinion = minion.Name.ToLower();
            if (nameMinion.Contains("mech")) return 65;
            if (nameMinion.Contains("wizard") || nameMinion.Contains("basic")) return 48;
            if (nameMinion.Contains("wolf") || nameMinion.Contains("wraith")) return 50;
            if (nameMinion.Contains("golem") || nameMinion.Contains("lizard")) return 80;
            if (nameMinion.Contains("dragon") || nameMinion.Contains("worm")) return 100;
            return 50;
        }

        private static void KillSteal()
        {
            var target = SimpleTs.GetTarget(_humanQ.Range, SimpleTs.DamageType.Magical);
            var igniteDmg = _player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var qhDmg = _player.GetSpellDamage(target, SpellSlot.Q);
            var wDmg = _player.GetSpellDamage(target, SpellSlot.W);

            if (target != null && _config.Item("UseIgnite").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
            _player.SummonerSpellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health)
                {
                    _player.SummonerSpellbook.CastSpell(_igniteSlot, target);
                }
            }
            if (_human)
            {
                if (_humanQ.IsReady() && _player.Distance(target) <= _humanQ.Range && target != null && _config.Item("HumanQKs").GetValue<bool>())
                {
                    if (target.Health <= qhDmg)
                    {
                        _humanQ.Cast(target);
                    }
                }
                if (_humanW.IsReady() && _player.Distance(target) <= _humanW.Range && target != null && _config.Item("HumanWKs").GetValue<bool>())
                {
                    if (target.Health <= wDmg)
                    {
                        _humanW.Cast(target);
                    }
                }
            }
            if (_spider && _spiderQ.IsReady() && _player.Distance(target) <= _spiderQ.Range && target != null && _config.Item("SpiderQKs").GetValue<bool>())
            {
                if (target.Health <= qhDmg)
                {
                    _spiderQ.Cast(target);
                }
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var elise = Drawing.WorldToScreen(_player.Position);

            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_human && _config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _humanQ.Range, System.Drawing.Color.LightGray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_human && _config.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _humanW.Range, System.Drawing.Color.LightGray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_human && _config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _humanE.Range, System.Drawing.Color.LightGray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_spider && _config.Item("SpiderDrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _spiderQ.Range, System.Drawing.Color.LightGray,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (_spider && _config.Item("SpiderDrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _spiderE.Range, System.Drawing.Color.LightGray,
                   _config.Item("CircleThickness").GetValue<Slider>().Value,
                   _config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (_human && _config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _humanQ.Range, System.Drawing.Color.LightGray);
                }
                if (_human && _config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _humanW.Range, System.Drawing.Color.LightGray);
                }
                if (_human && _config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _humanE.Range, System.Drawing.Color.LightGray);
                }
                if (_spider && _config.Item("SpiderDrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _spiderQ.Range, System.Drawing.Color.LightGray);
                }
                if (_spider && _config.Item("SpiderDrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _spiderE.Range, System.Drawing.Color.LightGray);
                }
            }
            if (!_spider)
            {
                if (_spideQcd == 0)
                    Drawing.DrawText(elise[0] - 60, elise[1], Color.White, "SQ Rdy");
                else
                    Drawing.DrawText(elise[0] - 60, elise[1], Color.Orange, "SQ: " + _spideQcd.ToString("0.0"));
                if (_spideWcd == 0)
                    Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.White, "SW Rdy");
                else
                    Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.Orange, "SW: " + _spideWcd.ToString("0.0"));
                if (_spideEcd == 0)
                    Drawing.DrawText(elise[0], elise[1], Color.White, "SE Rdy");
                else
                    Drawing.DrawText(elise[0], elise[1], Color.Orange, "SE: " + _spideEcd.ToString("0.0"));
            }
            else
            {
                if (_humaQcd == 0)
                    Drawing.DrawText(elise[0] - 60, elise[1], Color.White, "HQ Rdy");
                else
                    Drawing.DrawText(elise[0] - 60, elise[1], Color.Orange, "HQ: " + _humaQcd.ToString("0.0"));
                if (_humaWcd == 0)
                    Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.White, "HW Rdy");
                else
                    Drawing.DrawText(elise[0] - 20, elise[1] + 30, Color.Orange, "HW: " + _humaWcd.ToString("0.0"));
                if (_humaEcd == 0)
                    Drawing.DrawText(elise[0], elise[1], Color.White, "HE Rdy");
                else
                    Drawing.DrawText(elise[0], elise[1], Color.Orange, "HE: " + _humaEcd.ToString("0.0"));
            }
        }

        private static void CheckSpells()
        {
            if (_player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ" ||
                _player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseHumanW" ||
                _player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseHumanE")
            {
                _human = true;
                _spider = false;
            }

            if (_player.Spellbook.GetSpell(SpellSlot.Q).Name == "EliseSpiderQCast" ||
                _player.Spellbook.GetSpell(SpellSlot.W).Name == "EliseSpiderW" ||
                _player.Spellbook.GetSpell(SpellSlot.E).Name == "EliseSpiderEInitial")
            {
                _human = false;
                _spider = true;
            }
        }
    }
}

