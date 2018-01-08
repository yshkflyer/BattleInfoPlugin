﻿using System;
using System.Reactive.Linq;
using BattleInfoPlugin.Models.Raw;
using BattleInfoPlugin.Models.Repositories;
using Grabacr07.KanColleWrapper;

namespace BattleInfoPlugin
{
    class SortieDataListener
    {
        private readonly EnemyDataProvider provider = new EnemyDataProvider();

        public SortieDataListener()
        {
            var proxy = KanColleClient.Current.Proxy;

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_battle_midnight/sp_midnight")
                .TryParse<battle_midnight_sp_midnight>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_combined_battle_airbattle
                .TryParse<combined_battle_airbattle>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_combined_battle_battle
                .TryParse<combined_battle_battle>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/battle_water")
                .TryParse<combined_battle_battle_water>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/sp_midnight")
                .TryParse<combined_battle_sp_midnight>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_sortie/airbattle")
                .TryParse<sortie_airbattle>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_sortie_battle
                .TryParse<sortie_battle>().Subscribe(x => this.Update(x.Data));


            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_map/start")
                .TryParse<map_start>().Subscribe(x =>
                {
                    this.Update(x.Data);
                    Models.Repositories.Master.Current.Update(x.Data);
                });

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_map/next")
                .TryParse<map_next>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_sortie/ld_airbattle")
                .TryParse<sortie_ld_airbattle>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/ld_airbattle")
                .TryParse<combined_battle_ld_airbattle>().Subscribe(x => this.Update(x.Data));


            proxy.api_req_sortie_battleresult
                .TryParse<battle_result>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_combined_battle_battleresult
                .TryParse<battle_result>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_get_member/mapinfo")
                .TryParse<member_mapinfo[]>().Subscribe(x => this.Update(x.Data));
        }

        #region Battle

        public void Update(ICommonFirstBattleMembers data)
        {
            this.provider.UpdateEnemyData(
                data.api_ship_ke,
                data.api_formation,
                data.api_eSlot,
                data.api_eKyouka,
                data.api_eParam,
                data.api_ship_lv,
                data.api_e_maxhps);
            this.provider.UpdateBattleTypes(data);
        }

        #endregion

        #region StartNext

        private void Update(map_next next)
            => this.provider.UpdateMapData(next);

        private void Update(battle_result result)
            => this.provider.UpdateEnemyName(result);

        private void Update(member_mapinfo[] mapinfos)
            => this.provider.UpdateMapInfo(mapinfos);

        #endregion
    }
}
