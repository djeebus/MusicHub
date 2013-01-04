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
            // Core
            this.Bind<IJukebox>().To<MusicHub.Implementation.DefaultJukebox>().InSingletonScope();

            // TagLibSharp
            this.Bind<IMetadataService>().To<TagLibSharp.TagLibSharpMetadataService>().InSingletonScope();

            // EntityFramework
            this.Bind<EntityFramework.DbContext>().ToSelf().InTransientScope();
            this.Bind<IAffinityTracker>().To<EntityFramework.AffinityTracker>().InTransientScope();
            this.Bind<IUserRepository>().To<EntityFramework.UserRepository>().InTransientScope();
            this.Bind<ISongRepository>().To<EntityFramework.SongRepository>().InTransientScope();
            this.Bind<ILibraryRepository>().To<EntityFramework.LibraryRepository>().InTransientScope();
            
            // BassNet
            this.Bind<IMediaPlayer>().To<BassNet.BassNetMediaPlayer>().InSingletonScope();

            // ConsoleApp
            this.Bind<IMusicLibraryFactory>().To<MusicLibraryFactory>().InSingletonScope();
        }
    }
}
