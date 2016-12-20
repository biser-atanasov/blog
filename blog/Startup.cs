using blog.Migrations;
using blog.Models;
using System.Data.Entity;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(blog.Startup))]
namespace blog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer (new MigrateDatabaseToLatestVersion<BlogDbContext, Configuration>());

            ConfigureAuth(app);
        }
    }
}
