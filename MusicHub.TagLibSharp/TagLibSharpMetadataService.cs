using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MusicHub.TagLibSharp
{
    public class TagLibSharpMetadataService : IMetadataService
    {
        public Song GetSongFromFilename(string filename)
        {
            try
            {
                var info = TagLib.File.Create(filename);

                var tag = info.Tag;
                if (tag != null)
                {
                    return new Song
                    {
                        Album = tag.Album,
                        Artist = tag.JoinedPerformers ?? tag.JoinedAlbumArtists,
                        Title = tag.Title,
                        Track = tag.Track,
                        Year = tag.Year,
                    };
                }
            }
            catch
            {
            }

            return new Song
            {
                Title = Path.GetFileNameWithoutExtension(filename)
            };
        }
    }
}
