using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class ColorPallete
    {
        public ColorPallete()
        {
            TaskFolders = new HashSet<TaskFolder>();
            TaskLegends = new HashSet<TaskLegend>();
        }

        public int Id { get; set; }
        public string SupplementColorIds { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }

        public virtual ICollection<TaskFolder> TaskFolders { get; set; }
        public virtual ICollection<TaskLegend> TaskLegends { get; set; }
    }
}
