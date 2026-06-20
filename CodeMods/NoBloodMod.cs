using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace NoBloodMod
{
    /// <summary>
    /// 无血Mod - 在小猫房间与小猫交互时不扣血，在女神房间选择能力时也不扣血
    /// </summary>
    public class NoBloodModMain : SimpleModBehaviour
    {
        private static Harmony _harmony;
        private static bool _done = false;

        /// <summary>
        /// Mod 加载时调用
        /// </summary>
        public override void OnModLoaded()
        {
            base.OnModLoaded();
            Debug.Log("[小猫互动不扣血] Mod 已加载");
        }

        /// <summary>
        /// Mod 卸载时调用
        /// </summary>
        public override void OnModUnloaded()
        {
            base.OnModUnloaded();
            // 取消 Harmony 补丁
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
                Debug.Log("[小猫互动不扣血] Harmony 补丁已移除");
            }
            _done = false;
            Debug.Log("[小猫互动不扣血] Mod 已卸载");
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Update()
        {
            // 首帧初始化 Harmony
            if (!_done)
            {
                _done = true;
                try
                {
                    // 创建 Harmony 实例
                    _harmony = new Harmony("com.tokgok.nobloodmod");

                    // Patch 猫伤害 - 拦截 UnitObjectPlayer.GetDamage
                    PatchCatDamage();

                    // Patch 女神房间扣血 - 拦截 UnitObjectOther.HandlePurchase
                    PatchDevilRoomPurchase();

                    Debug.Log("[小猫互动不扣血] Harmony 补丁应用成功");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[小猫互动不扣血] Harmony 补丁应用失败: {ex.Message}");
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// 补丁猫伤害 - 当攻击者是猫 (unitType=1114) 时跳过伤害
        /// </summary>
        private static void PatchCatDamage()
        {
            // 获取 UnitObjectPlayer 类型
            Type unitObjectPlayerType = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                unitObjectPlayerType = assembly.GetType("UnitObjectPlayer");
                if (unitObjectPlayerType != null) break;
            }

            if (unitObjectPlayerType == null)
            {
                Debug.LogError("[小猫互动不扣血] 未找到 UnitObjectPlayer 类型");
                return;
            }

            // 获取 GetDamage 方法
            // 方法签名: public bool GetDamage(int damage, UnitObject atkUnit, ...)
            MethodInfo getDamageMethod = null;
            foreach (MethodInfo method in unitObjectPlayerType.GetMethods())
            {
                if (method.Name == "GetDamage" && method.GetParameters().Length >= 2)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    // 查找第二个参数是 UnitObject 类型的方法
                    if (parameters.Length >= 2 && parameters[1].ParameterType.Name == "UnitObject")
                    {
                        getDamageMethod = method;
                        break;
                    }
                }
            }

            if (getDamageMethod == null)
            {
                Debug.LogError("[小猫互动不扣血] 未找到 UnitObjectPlayer.GetDamage 方法");
                return;
            }

            // 应用 Prefix 补丁
            MethodInfo catDamagePrefix = typeof(NoBloodModMain).GetMethod("CatDamagePrefix");
            _harmony.Patch(getDamageMethod, new HarmonyMethod(catDamagePrefix));

            Debug.Log("[小猫互动不扣血] 猫伤害补丁已应用");
        }

        /// <summary>
        /// 猫伤害前置补丁 - 当攻击者是猫时返回 false 跳过伤害
        /// </summary>
        public static bool CatDamagePrefix(object __instance, object atkUnit)
        {
            try
            {
                if (atkUnit == null) return true; // 继续执行原始方法

                // 获取 atkUnit 的 unitType
                FieldInfo unitTypeField = atkUnit.GetType().GetField("unitType");
                if (unitTypeField == null) return true;

                int unitType = (int)unitTypeField.GetValue(atkUnit);

                // unitType == 1114 是猫 NPC
                if (unitType == 1114)
                {
                    Debug.Log("[小猫互动不扣血] 猫攻击被拦截，不造成伤害");
                    return false; // 跳过原始方法，不造成伤害
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[小猫互动不扣血] 猫伤害补丁异常: {ex.Message}");
            }

            return true; // 继续执行原始方法
        }

        /// <summary>
        /// 补丁女神房间扣血 - 拦截 UnitObjectOther.HandlePurchase
        /// </summary>
        private static void PatchDevilRoomPurchase()
        {
            // 获取 UnitObjectOther 类型
            Type unitObjectOtherType = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                unitObjectOtherType = assembly.GetType("UnitObjectOther");
                if (unitObjectOtherType != null) break;
            }

            if (unitObjectOtherType == null)
            {
                Debug.LogError("[小猫互动不扣血] 未找到 UnitObjectOther 类型");
                return;
            }

            // 获取 HandlePurchase 方法
            MethodInfo handlePurchaseMethod = null;
            foreach (MethodInfo method in unitObjectOtherType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name == "HandlePurchase")
                {
                    handlePurchaseMethod = method;
                    break;
                }
            }

            if (handlePurchaseMethod == null)
            {
                Debug.LogError("[小猫互动不扣血] 未找到 UnitObjectOther.HandlePurchase 方法");
                return;
            }

            // 应用 Prefix 补丁
            MethodInfo purchasePrefix = typeof(NoBloodModMain).GetMethod("HandlePurchasePrefix");
            _harmony.Patch(handlePurchaseMethod, new HarmonyMethod(purchasePrefix));

            Debug.Log("[小猫互动不扣血] 女神房间扣血补丁已应用");
        }

        /// <summary>
        /// HandlePurchase 前置补丁 - 将 hpPrice 设为 0
        /// </summary>
        public static void HandlePurchasePrefix(object __instance)
        {
            try
            {
                // 获取 hpPrice 字段并设置为 0
                FieldInfo hpPriceField = __instance.GetType().GetField("hpPrice", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (hpPriceField != null)
                {
                    hpPriceField.SetValue(__instance, 0);
                    Debug.Log("[小猫互动不扣血] HP价格已设为0");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[小猫互动不扣血] HandlePurchase补丁异常: {ex.Message}");
            }
        }
    }
}
