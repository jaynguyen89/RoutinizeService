using System;
using HelperLibrary;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class EmployeeVM {
        
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public string EmployeeType { get; set; }
        
        public int OrganizationId { get; set; }

        public int DepartmentId { get; set; }
        
        public int RoleId { get; set; }
        
        public int PositionId { get; set; }
        
        public string EmployeeCode { get; set; }
        
        public DateTime? JointOn { get; set; }
        
        public DateTime? LeftOn { get; set; }

        public string[] VerifyEmployeeData() {
            if (!Helpers.IsProperString(EmployeeCode)) {
                EmployeeCode = null;
                return default;
            }

            EmployeeCode = EmployeeCode.Trim().ToUpper();
            return EmployeeCode.Length > 50 ? new[] { "Employee code is too long. Max 50 characters." } : default;
        }

        public UserOrganization GetManagerUser() {
            return new() {
                UserId = this.UserId,
                OrganizationId = this.OrganizationId,
                DepartmentRoleId = RoleId,
                PositionId = this.PositionId,
                EmployeeCode = this.EmployeeCode,
                IsActive = true,
                JointOn = this.JointOn
            };
        }
        
        public UserDepartment GetEmployeeUser() {
            return new() {
                UserId = this.UserId,
                DepartmentId = this.DepartmentId,
                DepartmentRoleId = RoleId,
                PositionId = this.PositionId,
                EmployeeCode = this.EmployeeCode,
                IsActive = true,
                JointOn = this.JointOn
            };
        }

        public void UpdateManagerUserFor(ref UserOrganization manager) {
            manager.PositionId = PositionId;
            manager.EmployeeCode = EmployeeCode;
            manager.JointOn = JointOn;
        }

        public void UpdateEmployeeUserFor(ref UserDepartment employee) {
            employee.PositionId = PositionId;
            employee.EmployeeCode = EmployeeCode;
            employee.JointOn = JointOn;
        }
    }
}