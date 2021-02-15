using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TaskLegend
    {
        public int Id { get; set; }
        public int CreatedById { get; set; }
        public bool ForOrganizationWide { get; set; }
        public string ForDepartmentIds { get; set; }
        public string ForTeamIds { get; set; }
        public string ForProjectIds { get; set; }
        public string LegendName { get; set; }
        public string Description { get; set; }
        public int? ColorId { get; set; }
        public string FillColor { get; set; }
        public byte FillPattern { get; set; }

        public virtual ColorPallete Color { get; set; }
        public virtual User CreatedBy { get; set; }
    }
}
