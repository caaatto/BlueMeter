using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueMeter.Core.Extends.Data;
using BlueMeter.Core.Models;

namespace BlueMeter.Core.Data.Models
{
    public class PlayerInfo
    {
        private ClassSpec _spec;
        public long UID { get; internal set; }
        public string? Name { get; internal set; }
        public int? ProfessionID { get; internal set; }
        public string? SubProfessionName { get; internal set; }

        /// <summary>
        /// 职业流派
        /// </summary>
        public ClassSpec Spec
        {
            get => _spec;
            internal set
            {
                if (_spec == value) return;
                _spec = value;
                // Update classes
                ProfessionID = Class.GetProfessionID();
            }
        }

        public Classes Class => Spec.GetClasses();

        public int? CombatPower { get; internal set; }
        public int? Level { get; internal set; }
        public int? RankLevel { get; internal set; }
        public int? Critical { get; internal set; }
        public int? Lucky { get; internal set; }
        public long? MaxHP { get; internal set; }
        public long? HP { get; internal set; }
    }
}
