using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp
{
    public class MusicHubNinjectModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<EntityFramework.DbContext>().ToSelf().InThreadScope();
            this.Bind<IMetadataService>().To<TagLibSharp.TagLibSharpMetadataService>().InSingletonScope();
            this.Bind<IUserRepository>().To<EntityFramework.UserRepository>().InThreadScope();
            this.Bind<ISongRepository>().To<EntityFramework.SongRepository>().InThreadScope();
            this.Bind<ILibraryRepository>().To<EntityFramework.LibraryRepository>().InThreadScope();
            this.Bind<IMediaPlayer>().To<FMod.FModMediaPlayer>().InSingletonScope();
        }
    }
}
