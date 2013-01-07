using Moq;
using MusicHub.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.Core.Tests
{
    public abstract class JukeboxBaseTest
    {
        Mock<IMediaPlayer> _mediaPlayer = new Mock<IMediaPlayer>();
        Mock<IMusicLibraryFactory> _musicLibraryFactory = new Mock<IMusicLibraryFactory>();
        Mock<ILibraryRepository> _libraryRepository = new Mock<ILibraryRepository>();
        Mock<ISongRepository> _songRepository = new Mock<ISongRepository>();
        Mock<IAffinityTracker> _affinityTracker = new Mock<IAffinityTracker>();
        Mock<SongSpider> _songSpider = new Mock<SongSpider>();

        protected virtual DefaultJukebox CreateJukebox()
        {
            return new DefaultJukebox(
                _mediaPlayer.Object,
                _musicLibraryFactory.Object,
                _libraryRepository.Object,
                _songRepository.Object,
                _affinityTracker.Object,
                new SongSpider(_libraryRepository.Object, _songRepository.Object, _musicLibraryFactory.Object));
        }

    }
}
