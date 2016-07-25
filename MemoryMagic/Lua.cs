using System;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace MemoryMagic
{
    public class Lua
    {
        private readonly Hook _wowHook;

        public Lua(Hook wowHook)
        {
            _wowHook = wowHook;
        }

        public void DoString(string command)
        {
            if (!_wowHook.Installed) return;

            // Allocate memory
            var doStringArgCodecave = _wowHook.Memory.AllocateMemory(Encoding.UTF8.GetBytes(command).Length + 1);
            // Write value:
            _wowHook.Memory.WriteBytes(doStringArgCodecave, Encoding.UTF8.GetBytes(command));

            // Write the asm stuff for Lua_DoString
            var asm = new[]
            {
                "mov eax, " + doStringArgCodecave,
                "push 0",
                "push eax",
                "push eax",
                "mov eax, " + ((uint) Offsets.Framescript_ExecuteBuffer + _wowHook.Process.BaseOffset()),
                "call eax",
                "add esp, 0xC",
                "retn"
            };
            // Inject
            _wowHook.InjectAndExecute(asm);
            // Free memory allocated 
            _wowHook.Memory.FreeMemory(doStringArgCodecave);
        }

        public string GetLocalizedText(string localVar)
        {
            if (!_wowHook.Installed) return "WoW Hook not installed";

            var Lua_GetLocalizedText_Space = _wowHook.Memory.AllocateMemory(Encoding.UTF8.GetBytes(localVar).Length + 1);

            _wowHook.Memory.Write(Lua_GetLocalizedText_Space, Encoding.UTF8.GetBytes(localVar));

            var asm = new[]
            {
                "call " + ((uint) Offsets.ClntObjMgrGetActivePlayerObj + + _wowHook.Process.BaseOffset()),
                "mov ecx, eax",
                "push -1",
                "mov edx, " + Lua_GetLocalizedText_Space + "",
                "push edx",
                "call " + ((uint) Offsets.FrameScript__GetLocalizedText + _wowHook.Process.BaseOffset()),
                "retn"
            };

            var sResult = Encoding.UTF8.GetString(_wowHook.InjectAndExecute(asm));

            // Free memory allocated 
            _wowHook.Memory.FreeMemory(Lua_GetLocalizedText_Space);
            return sResult;
        }

        public void SendTextMessage(string message)
        {
            DoString("RunMacroText('/me " + message + "')");
        }

        public void CastSpellByName(string spell)
        {
            DoString($"CastSpellByName('{spell}')");
        }

        public double DebuffRemainingTime(string debuffName)
        {
            var luaStr = $"name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitAura('target','{debuffName}',nil,'HARMFUL')";
            DoString(luaStr);
            var result = GetLocalizedText("expirationTime");

            if (result == "")
                return 0;

            DoString("time = GetTime()");
            var currentTime = GetLocalizedText("time");

            var timeInSeconds = double.Parse(result) - double.Parse(currentTime);

            return timeInSeconds < 0 ? 0 : timeInSeconds;
        }

        public bool HasTarget
        {
            get
            {
                DoString(@"guid = UnitGUID(""target"")");
                string result = GetLocalizedText("guid");

                return true;
            }
        }

        public bool PlayerIsCasting
        {
            get
            {
                DoString(@"spell, rank, displayName, icon, startTime, endTime, isTradeSkill, castID, interrupt = UnitCastingInfo(""player"")");
                string result = GetLocalizedText("castID");

                return true;
            }
        }

        public bool TargetIsCasting
        {
            get
            {
                DoString(@"spell, rank, displayName, icon, startTime, endTime, isTradeSkill, castID, interrupt = UnitCastingInfo(""target"")");
                string result = GetLocalizedText("castID");

                return true;
            }
        }

        public bool TargetIsVisible
        {
            get
            {
                DoString(@"local vis = UnitIsVisible(""target"")");
                string result = GetLocalizedText("vis");

                return true;
            }
        }

        public bool TargetIsFriend
        {
            get
            {
                DoString(@"isFriend = UnitIsFriend(""player"",""target"");");
                string result = GetLocalizedText("isFriend");

                return true;
            }
        }

        public int MaxRunes
        {
            get
            {
                DoString(@"local CurrentMaxRunes = UnitPower(""player"", SPELL_POWER_RUNES);");
                string result = GetLocalizedText("CurrentMaxRunes");
                return int.Parse(result);
            }
        }

        public bool RuneReady(int runeId) // runeId = 1..6
        {
            DoString(@"local RuneReady = select(3, GetRuneCooldown(i))");
            string result = GetLocalizedText("RuneReady");
            return true;
        }

        public int CurrentRunes
        {
            get
            {
                int count = 0;
                for (int i = 1; i <= 6; i++)
                {
                    if (RuneReady(i))
                        count++;
                }
                return count;
            }
        }

        public int CurrentComboPoints
        {
            get
            {
                DoString(@"local power = UnitPower(""player"", 4)");
                string result = GetLocalizedText("power");
                return int.Parse(result);
            }
        }

        public int CurrentSoulShards
        {
            get
            {
                DoString(@"local power = UnitPower(""player"", 7)");
                string result = GetLocalizedText("power");
                return int.Parse(result);
            }
        }

        public int CurrentHolyPower
        {
            get
            {
                DoString(@"local power = UnitPower(""player"", 9)");
                string result = GetLocalizedText("power");
                return int.Parse(result);
            }
        }

        public bool TargetIsEnemy => !TargetIsFriend;

        public int CurrentHealth
        {
            get
            {
                DoString(@"local health = UnitHealth(""player"")");
                string result = GetLocalizedText("health");
                return int.Parse(result);
            }
        }

        public int MaxHealth
        {
            get
            {
                DoString(@"local maxHealth = UnitHealthMax(""player"")");
                string result = GetLocalizedText("maxHealth");
                return int.Parse(result);
            }
        }

        public int HealthPercent
        {
            get
            {
                double result = CurrentHealth / MaxHealth * 100;
                return int.Parse(Math.Round(result, 0).ToString());
            }
        }

        public int TargetCurrentHealth
        {
            get
            {
                DoString(@"local health = UnitHealth(""target"")");
                string result = GetLocalizedText("health");
                return int.Parse(result);
            }
        }

        public int TargetMaxHealth
        {
            get
            {
                DoString(@"local maxHealth = UnitHealthMax(""target"")");
                string result = GetLocalizedText("maxHealth");
                return int.Parse(result);
            }
        }

        public int TargetHealthPercent
        {
            get
            {
                double result = TargetCurrentHealth / TargetMaxHealth * 100;
                return int.Parse(Math.Round(result, 0).ToString());
            }
        }

        public int CurrentPower
        {
            get
            {
                DoString(@"local power = UnitPower(""player"")");
                string result = GetLocalizedText("power");
                return int.Parse(result);
            }
        }

        public int MaximumPower
        {
            get
            {
                DoString(@"local maxPower = UnitPowerMax(""player"")");
                string result = GetLocalizedText("maxPower");
                return int.Parse(result);
            }
        }

        public int Focus => CurrentPower;
        public int Mana => CurrentPower;
        public int Energy => CurrentPower;
        public int Rage => CurrentPower;
        public int Fury => CurrentPower;
        public int RunicPower => CurrentPower;

        public bool IsSpellOnCooldown(int spellId)
        {
            DoString($@"local getTime = GetTime(); local start, duration, enabled = GetSpellCooldown({spellId}); local remainingCD = start + duration - getTime");
            string result = GetLocalizedText("remainingCD");
            return true;
        }

        public bool IsSpellOnCooldown(string spellName)
        {
            DoString($@"local getTime = GetTime(); local start, duration, enabled = GetSpellCooldown(""{spellName}""); local remainingCD = start + duration - getTime");
            string result = GetLocalizedText("remainingCD");
            return true;
        }
        
        public bool IsSpellInRange(int spellId)
        {
            DoString($@"local name, rank, icon, castTime, minRange, maxRange = GetSpellInfo({spellId}); local inRange = IsSpellInRange(name, ""target"")");
            string result = GetLocalizedText("inRange");
            return true;
        }

        public bool IsSpellInRange(string spellName)
        {
            DoString($@"local inRange = IsSpellInRange(""{spellName}"", ""target"")");
            string result = GetLocalizedText("inRange");
            return true;
        }

        public int GetSpellCharges(int spellId)
        {
            DoString($@"local charges, maxCharges, start, duration = GetSpellCharges({spellId})");
            string result = GetLocalizedText("charges");
            return int.Parse(result);
        }

        public int GetSpellCharges(string spellName)
        {
            DoString($@"local charges, maxCharges, start, duration = GetSpellCharges(""{spellName}"")");
            string result = GetLocalizedText("charges");
            return int.Parse(result);
        }

        public bool CanCast(int spellId,
                            bool checkIfPlayerIsCasting = true,
                            bool checkIfSpellIsOnCooldown = true,
                            bool checkIfSpellIsInRange = true,
                            bool checkSpellCharges = true,
                            bool checkIfTargetIsVisible = true)
        {
            if (checkIfPlayerIsCasting)
                if (PlayerIsCasting)
                    return false;

            if (checkIfSpellIsOnCooldown)
                if (IsSpellOnCooldown(spellId))
                    return false;

            if (checkIfSpellIsInRange)
                if (IsSpellInRange(spellId) == false)
                    return false;

            if (checkSpellCharges)
                if (GetSpellCharges(spellId) <= 0)
                    return false;

            if (checkIfTargetIsVisible)
                if (TargetIsVisible == false)
                    return false;

            return true;
        }

        public bool CanCast(string spellName,
                            bool checkIfPlayerIsCasting = true,
                            bool checkIfSpellIsOnCooldown = true,
                            bool checkIfSpellIsInRange = true,
                            bool checkSpellCharges = true,
                            bool checkIfTargetIsVisible = true)
        {
            if (checkIfPlayerIsCasting)
                if (PlayerIsCasting)
                    return false;

            if (checkIfSpellIsOnCooldown)
                if (IsSpellOnCooldown(spellName))
                    return false;

            if (checkIfSpellIsInRange)
                if (IsSpellInRange(spellName) == false)
                    return false;

            if (checkSpellCharges)
                if (GetSpellCharges(spellName) <= 0)
                    return false;

            if (checkIfTargetIsVisible)
                if (TargetIsVisible == false)
                    return false;

            return true;
        }

        public void SendMacro(string macro)
        {
            DoString("RunMacroText('/me " + macro + "')");
        }

        public bool HasBuff(int buffId)
        {
            DoString($@"local buffName = GetSpellInfo({buffId}); local name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitBuff(""player"", buffName)");
            string result = GetLocalizedText("count");
            return true;
        }

        public bool HasBuff(string buffName)
        {
            DoString($@"local name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitBuff(""player"", ""{buffName}"")");
            string result = GetLocalizedText("count");
            return true;
        }

        public bool HasDebuff(int debuffId)
        {
            DoString($@"local debuffName = GetSpellInfo({debuffId}); local name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitDebuff(""player"", debuffName)");
            string result = GetLocalizedText("count");
            return true;
        }

        public bool HasDebuff(string debuffName)
        {
            DoString($@"local name, rank, icon, count, debuffType, duration, expirationTime, unitCaster, isStealable, shouldConsolidate, spellId = UnitDebuff(""player"", ""{debuffName}"")");
            string result = GetLocalizedText("count");
            return true;
        }
    }
}