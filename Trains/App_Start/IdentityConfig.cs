using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trains.Models;

namespace Trains
{ 
    //******************* USER-MANAGER
    public class TrainsUserManager : UserManager<AppUser>
    {
        public TrainsUserManager(IUserStore<AppUser> store) : base(store) { }

        public static TrainsUserManager Create(IdentityFactoryOptions<TrainsUserManager> options, IOwinContext context)
        {
        TrainsContext db = context.Get<TrainsContext>(); 

        TrainsUserManager manager = new TrainsUserManager(new UserStore<AppUser>(db));
         
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<AppUser>(dataProtectionProvider.Create("Identity"));
            }
            return manager;
         
        }


    


    }
}