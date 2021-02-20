using System.Collections.Generic;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class IndustryVM {
        
        public int Id { get; set; }
        
        public string Name { get; set; }

        public List<SubIndustryVM> SubIndustries { get; set; }

        public static implicit operator IndustryVM(Industry industry) {
            return new() {
                Id = industry.Id,
                Name = industry.Name
            };
        }

        public sealed class SubIndustryVM {
            
            public int Id { get; set; }
            
            public string Name { get; set; }
            
            public string Description { get; set; }

            public static implicit operator SubIndustryVM(Industry industry) {
                return new() {
                    Id = industry.Id,
                    Name = industry.Name,
                    Description = industry.Description
                };
            }
        }
    }
}