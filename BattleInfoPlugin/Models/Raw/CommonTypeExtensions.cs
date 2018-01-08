using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleInfoPlugin.Models.Raw
{
    public static class CommonTypeExtensions
    {
        private static readonly FleetDamages defaultValue = new FleetDamages();

        #region 支援

        public static FleetDamages GetEnemyDamages(this Api_Support_Info support)
            => support?.api_support_airatack?.api_stage3?.api_edam?.GetDamages()
               ?? support?.api_support_hourai?.api_damage?.GetDamages()
               ?? defaultValue;

        #endregion

        #region 砲撃

        public static FleetDamages GetFriendDamages(this Hougeki hougeki)
            => hougeki?.api_damage?.GetFriendDamages(hougeki.api_df_list, hougeki.api_at_eflag)
               ?? defaultValue;

        public static FleetDamages GetEnemyDamages(this Hougeki hougeki)
            => hougeki?.api_damage?.GetEnemyDamages(hougeki.api_df_list, hougeki.api_at_eflag)
               ?? defaultValue;

        #endregion

        #region 夜戦

        public static FleetDamages GetFriendDamages(this Midnight_Hougeki hougeki)
            => hougeki?.api_damage?.GetFriendDamages(hougeki.api_df_list, hougeki.api_at_eflag)
               ?? defaultValue;

        public static FleetDamages GetEnemyDamages(this Midnight_Hougeki hougeki)
            => hougeki?.api_damage?.GetEnemyDamages(hougeki.api_df_list, hougeki.api_at_eflag)
               ?? defaultValue;

        #endregion

        #region 航空戦

        public static FleetDamages GetFirstFleetDamages(this Api_Kouku kouku)
            => kouku?.api_stage3?.api_fdam.GetDamages()
               ?? defaultValue;

        public static FleetDamages GetSecondFleetDamages(this Api_Kouku kouku)
            => kouku?.api_stage3_combined?.api_fdam?.GetDamages()
               ?? defaultValue;

        public static FleetDamages GetEnemyDamages(this Api_Kouku kouku)
            => kouku?.api_stage3?.api_edam?.GetDamages()
               ?? defaultValue;

        public static FleetDamages GetEnemyDamages(this Api_Air_Base_Attack[] attacks)
            => attacks?.Select(x => x?.api_stage3?.api_edam?.GetDamages() ?? defaultValue)
            ?.Aggregate((a, b) => a.Add(b)) ?? defaultValue;

        public static AirSupremacy GetAirSupremacy(this Api_Kouku kouku)
            => (AirSupremacy)(kouku?.api_stage1?.api_disp_seiku ?? (int)AirSupremacy.航空戦なし);

        public static AirCombatResult[] ToResult(this Api_Kouku kouku, string prefixName = "")
        {
            return kouku != null
                ? new []
                {
                    kouku.api_stage1.ToResult($"{prefixName}空対空"),
                    kouku.api_stage2.ToResult($"{prefixName}空対艦")
                }
                : new AirCombatResult[0];
        }

        public static AirCombatResult ToResult(this Api_Stage1 stage1, string name)
            => stage1 == null ? new AirCombatResult(name)
            : new AirCombatResult(name, stage1.api_f_count, stage1.api_f_lostcount, stage1.api_e_count, stage1.api_e_lostcount);

        public static AirCombatResult ToResult(this Api_Stage2 stage2, string name)
            => stage2 == null ? new AirCombatResult(name)
            : new AirCombatResult(name, stage2.api_f_count, stage2.api_f_lostcount, stage2.api_e_count, stage2.api_e_lostcount);

        #endregion

        #region 雷撃戦

        public static FleetDamages GetFriendDamages(this Raigeki raigeki)
            => raigeki?.api_fdam?.GetDamages()
               ?? defaultValue;

        public static FleetDamages GetEnemyDamages(this Raigeki raigeki)
            => raigeki?.api_edam?.GetDamages()
               ?? defaultValue;

        #endregion

        #region ダメージ計算

        /// <summary>
        /// 12項目中先頭6項目取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="origin">ゴミ-1が付いてる場合1オリジン</param>
        /// <returns></returns>
        public static IEnumerable<T> GetFriendData<T>(this IEnumerable<T> source, int origin = 1)
            => source.Skip(origin).Take(6);

        /// <summary>
        /// 12項目中末尾6項目取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="origin">ゴミ-1が付いてる場合1オリジン</param>
        /// <returns></returns>
        public static IEnumerable<T> GetEnemyData<T>(this IEnumerable<T> source, int origin = 1)
            => source.Skip(origin + 6).Take(6);

        /// <summary>
        /// 雷撃・航空戦ダメージリスト算出
        /// </summary>
        /// <param name="damages">api_fdam/api_edam</param>
        /// <returns></returns>
        public static FleetDamages GetDamages(this double[] damages)
            => damages
                .GetFriendData(0) //敵味方共通
                .Select(Convert.ToInt32)
                .ToArray()
                .ToFleetDamages();

        #region 砲撃戦ダメージリスト算出

        /// <summary>
        /// 砲撃戦友軍ダメージリスト算出
        /// </summary>
        /// <param name="damages">api_damage</param>
        /// <param name="df_list">api_df_list</param>
        /// <returns></returns>
        public static FleetDamages GetFriendDamages(this object[] damages, object[] df_list, int[] at_eflag)
            => damages
                .ToIntArraySum()
                .ToFriendDamages(df_list.ToIntArray(), at_eflag)
                .ToFleetDamages();

        /// <summary>
        /// 砲撃戦敵軍ダメージリスト算出
        /// </summary>
        /// <param name="damages">api_damage</param>
        /// <param name="df_list">api_df_list</param>
        /// <returns></returns>
        public static FleetDamages GetEnemyDamages(this object[] damages, object[] df_list, int[] at_eflag)
            => damages
                .ToIntArraySum()
                .ToEnemyDamages(df_list.ToIntArray(), at_eflag)
                .ToFleetDamages();

        /// <summary>
        /// 砲撃戦ダメージリストint配列化
        /// 弾着観測射撃/連撃データは合計する
        /// </summary>
        /// <param name="damages">api_damage</param>
        /// <returns></returns>
        private static int[] ToIntArraySum(this object[] damages)
            => damages
                .Where(x => x is Array)
                .Select(x => ((Array) x).Cast<object>())
                .Select(x => x.Select(Convert.ToInt32).Sum())
                .ToArray();

        /// <summary>
        /// api_df_list int配列化
        /// 弾着観測射撃/連撃データは先頭要素を取得する
        /// </summary>
        /// <param name="dflist">api_df_list</param>
        /// <returns></returns>
        private static int[] ToIntArray(this object[] dflist)
            => dflist
                .Where(x => x is Array)
                .Select(x => ((Array)x).Cast<object>())
                .Select(x => x.Select(Convert.ToInt32).ToArray().SafeAccess(0, 0))
                .ToArray();

        /// <summary>
        /// フラット化したapi_damageとapi_df_listを元に
        /// api_at_eflagに基づき自軍6隻または敵軍6隻に
        /// 限定した長さ6のダメージ合計配列を作成
        /// </summary>
        /// <param name="damages">api_damage</param>
        /// <param name="dfList">api_df_list</param>
        /// <param name="efList">api_at_eflag</param>
        /// <returns></returns>
        private static int[] ToFriendDamages(this int[] damages, int[] dfList, int[] efList)
        {
            return SelectDamages(damages, dfList, efList, i => i != 0);
        }
        private static int[] ToEnemyDamages(this int[] damages, int[] dfList, int[] efList)
        {
            return SelectDamages(damages, dfList, efList, i => i == 0);
        }
        private static int[] SelectDamages(int[] damages, int[] dfList, int[] efList, Func<int, bool> op)
        {
            var zip1 = dfList.Zip(efList, (df, ef) => new { df, ef });
            var zip2 = damages.Zip(zip1, (da, a) => new { a.df, a.ef, da });
            var ret = new int[6];
            foreach (var d in zip2.Where(d => op(d.ef)))
            {
                ret[d.df] += d.da;
            }
            return ret;
        }

        #endregion

        public static T SafeAccess<T>(this T[] array, int index, T defaultValue)
        {
            if (index >= 0 && array?.Length > index)
            {
                return array[index];
            } else
            {
                return defaultValue;
            }
        }
        #endregion
    }
}
