using System;
using System.Threading.Tasks;

using BlueMeter.WinForm.Plugin;

namespace BlueMeter.WinForm.Forms
{
    public partial class RankingsForm
    {
        public void ToggleTableView()
        {

            table_DpsDetailDataTable.Columns.Clear();

            table_DpsDetailDataTable.Columns = new AntdUI.ColumnCollection
            {   
                new AntdUI.Column("Button", "操作"),
                new("", "序号")
                {
                   
                    Render = (value, record, rowIndex) => rowIndex + 1,
                    Fixed = true
                },

                new AntdUI.Column("NickName","玩家昵称"){ Fixed = true},
                new AntdUI.Column("Professional","职业"){ Fixed = true},
                new AntdUI.Column("SubProfessional","分支"){ Fixed = true},
                new AntdUI.Column("CombatPower","战力"){ Fixed = true,SortOrder=true },
                new AntdUI.Column("TotalDamage","总伤"){ Fixed = true,SortOrder=true},
                new AntdUI.Column("InstantDps","秒伤"){ Fixed = true,SortOrder=true},
                new AntdUI.Column("CritRate","暴击率"){ Fixed = true},
                new AntdUI.Column("LuckyRate","幸运率"){ Fixed = true},
              
                new AntdUI.Column("MaxInstantDps","最大瞬时"){ Fixed = true,SortOrder=true},

                //new AntdUI.Column("battleTime","战斗时长"),
            };

            table_DpsDetailDataTable.Binding(LeaderboardTableDatas.LeaderboardTable);
            get_dps_rank();

        }



        Dictionary<string, string> rank_type_dict = new Dictionary<string, string>()
        {
            {"伤害参考","damage_all"},
        };
        /// <summary>
        /// 
        /// </summary>
        private async void get_dps_rank()
        {
            //LeaderboardTableDatas.LeaderboardTable.Clear();
            //string url = @$"{AppConfig.url}/get_dps_rank";
            //var query = new
            //{
            //    rank_type = rank_type_dict[divider3.Text],
            //    professional = segmented1.Items[segmented1.SelectIndex],
            //    uid = AppConfig.Uid
            //};
            //var data = await Common.RequestGet(url,query);
            //if (data["code"].ToString()=="200")
            //{
            //    foreach (var item in data["data"])
            //    {
            //        string battleid = item["battleId"].ToString();
            //        string nickName = item["nickName"].ToString();
            //        string professional = item["professional"].ToString();
            //        double combatPower = double.Parse(item["combatPower"].ToString());
            //        double instantDps = double.Parse(item["instantDps"].ToString());
            //        double totalDamage = double.Parse(item["totalDamage"].ToString());
            //        double maxInstantDps = double.Parse(item["maxInstantDps"].ToString());
            //        double critRate = double.Parse(item["critRate"].ToString());
            //        double luckyRate = double.Parse(item["luckyRate"].ToString());
            //        string subProfessional = item["subProfession"].ToString();

            //        //int battleTime = int.Parse(item["battleTime"].ToString());
            //        LeaderboardTableDatas.LeaderboardTable.Add(new LeaderboardTable(battleid, nickName, professional, combatPower, totalDamage,instantDps, critRate, luckyRate, maxInstantDps, subProfessional));
            //    }
              
            //}
            //else
            //{
            //    //AntdUI.Modal.open(new AntdUI.Modal.Config(this, "获取失败", "获取失败")
            //    //{
            //    //    CloseIcon = true,
            //    //    Keyboard = false,
            //    //    MaskClosable = false,
            //    //});
            //}

        }
    }
}
