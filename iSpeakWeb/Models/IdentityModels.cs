using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System;

namespace iSpeak.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "First Name"), Required]
        public string Firstname { get; set; }

        [Display(Name = "Middle Name")]
        public string Middlename { get; set; }

        [Display(Name = "Last Name")]
        public string Lastname { get; set; }

        public string Address { get; set; }

        [Display(Name = "Phone 1")]
        public string Phone1 { get; set; }

        [Display(Name = "Phone 2")]
        public string Phone2 { get; set; }

        [Display(Name = "Date of Birth"), Required]
        public DateTime Birthday { get; set; }

        public string Notes { get; set; }

        public bool Active { get; set; }

        [Display(Name = "Branch")]
        public Guid Branches_Id { get; set; }

        [Display(Name = "Promotion Events")]
        public Guid? PromotionEvents_Id { get; set; }

        public string Interest { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("Fullname", Firstname + " " + Middlename + " " + Lastname));
            userIdentity.AddClaim(new Claim("Branches_Id", Branches_Id.ToString()));
            return userIdentity;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("Fullname", Firstname + " " + Middlename + " " + Lastname));
            userIdentity.AddClaim(new Claim("Branches_Id", Branches_Id.ToString()));
            return userIdentity;
        }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}