using System;
using System.Collections.Generic;
using System.Linq;
using BattleInfoPlugin.Models.Raw;

namespace BattleInfoPlugin.Models
{
    /// <summary>
    /// 1艦隊分のダメージ一覧
    /// </summary>
    public class FleetDamages
    {
        public int Ship1 { get; set; }
        public int Ship2 { get; set; }
        public int Ship3 { get; set; }
        public int Ship4 { get; set; }
        public int Ship5 { get; set; }
        public int Ship6 { get; set; }

        public int[] ToArray()
        {
            return new[]
            {
                this.Ship1,
                this.Ship2,
                this.Ship3,
                this.Ship4,
                this.Ship5,
                this.Ship6,
            };
        }

        public FleetDamages Add(FleetDamages value)
        {
            return Parse(new[]
            {
                this.Ship1 + value.Ship1,
                this.Ship2 + value.Ship2,
                this.Ship3 + value.Ship3,
                this.Ship4 + value.Ship4,
                this.Ship5 + value.Ship5,
                this.Ship6 + value.Ship6,
            });
        }

        public static FleetDamages Parse(IEnumerable<int> damages)
        {
            if (damages == null) throw new ArgumentNullException();
            var arr = damages.ToArray();
            // if (arr.Length != 6) throw new ArgumentException("艦隊ダメージ配列の長さは6である必要があります。");
            return new FleetDamages
            {
                Ship1 = arr.SafeAccess(0, 0),
                Ship2 = arr.SafeAccess(1, 0),
                Ship3 = arr.SafeAccess(2, 0),
                Ship4 = arr.SafeAccess(3, 0),
                Ship5 = arr.SafeAccess(4, 0),
                Ship6 = arr.SafeAccess(5, 0),
            };
        }
    }

    public static class FleetDamagesExtensions
    {
        public static FleetDamages ToFleetDamages(this IEnumerable<int> damages)
        {
            return FleetDamages.Parse(damages);
        }
    }
}
