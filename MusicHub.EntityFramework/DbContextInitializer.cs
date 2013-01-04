using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.EntityFramework
{
    public class DbContextInitializer : System.Data.Entity.CreateDatabaseIfNotExists<DbContext>
    {
        static string[] _indexes = new[] {
            "CREATE UNIQUE INDEX IDX_Users_Username ON Users (Username)",
            "CREATE UNIQUE INDEX IDX_Libraries_PathUsernamePassword ON Libraries (Path, Username, Password)",
            "CREATE UNIQUE INDEX IDX_Songs_ExternalId ON Songs (ExternalId)",
        };

        protected override void Seed(DbContext context)
        {
            foreach (var index in _indexes)
                context.Database.ExecuteSqlCommand(index);

            base.Seed(context);
        }
    }
}
