using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Consumers.Xbmc;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Metadata.Consumers.Xbmc
{
    [TestFixture]
    public class FindMetadataFileFixture : CoreTest<XbmcMetadata>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\Movies\The.Movie".AsOsAgnostic())
                                     .Build();

            Subject.Definition = new MetadataDefinition
            {
                Settings = new XbmcMetadataSettings
                {
                    MovieMetadata = true,
                    MovieMetadataURL = false
                }
            };
        }

        [Test]
        public void should_return_null_if_filename_is_not_handled()
        {
            var path = Path.Combine(_movie.Path, "file.jpg");

            Subject.FindMetadataFile(_movie, path).Should().BeNull();
        }

        [Test]
        public void should_return_metadata_for_xbmc_nfo()
        {
            var path = Path.Combine(_movie.Path, "the.movie.2017.nfo");

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Setup(v => v.IsXbmcNfoFile(path))
                  .Returns(true);

            Subject.FindMetadataFile(_movie, path).Type.Should().Be(MetadataType.MovieMetadata);

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Verify(v => v.IsXbmcNfoFile(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_return_null_for_scene_nfo()
        {
            var path = Path.Combine(_movie.Path, "the.movie.2017.nfo");

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Setup(v => v.IsXbmcNfoFile(path))
                  .Returns(false);

            Subject.FindMetadataFile(_movie, path).Should().BeNull();

            Mocker.GetMock<IDetectXbmcNfo>()
                  .Verify(v => v.IsXbmcNfoFile(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void should_use_kodi_codec_ids_in_streamdetails()
        {
            var movie = new Movie
            {
                Path = @"C:\Test\Movies\The.Movie".AsOsAgnostic(),
                MovieMetadataId = 1,
                TmdbId = 12345,
                ImdbId = "tt1234567",
                Title = "The Movie",
                Year = 2026
            };

            movie.MovieMetadata.Value.OriginalTitle = "The Movie";
            movie.MovieMetadata.Value.Overview = "Overview";
            movie.MovieMetadata.Value.Runtime = 90;
            movie.MovieMetadata.Value.Status = MovieStatusType.Released;

            var movieFile = new MovieFile
            {
                RelativePath = "The.Movie.2026.mkv",
                MediaInfo = new MediaInfoModel
                {
                    VideoFormat = "h264",
                    VideoCodecID = "x264",
                    VideoBitrate = 20_000_000,
                    Width = 1920,
                    Height = 1080,
                    VideoFps = 23.976m,
                    ScanType = "Progressive",
                    AudioFormat = "truehd",
                    AudioCodecID = "thd+",
                    AudioBitrate = 4_000_000,
                    AudioChannels = 8,
                    AudioLanguages = new List<string> { "eng" },
                    Subtitles = new List<string>()
                }
            };

            Mocker.GetMock<IMovieTranslationService>()
                  .Setup(v => v.GetAllTranslationsForMovieMetadata(movie.MovieMetadataId))
                  .Returns(new List<MovieTranslation>());

            Mocker.GetMock<ICreditService>()
                  .Setup(v => v.GetAllCreditsForMovieMetadata(movie.MovieMetadataId))
                  .Returns(new List<Credit>());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns(false);

            var result = Subject.MovieMetadata(movie, movieFile);
            var doc = XDocument.Parse(result.Contents);
            var streamDetails = doc.Root.Element("fileinfo").Element("streamdetails");

            streamDetails.Element("video").Element("codec").Value.Should().Be("h264");
            streamDetails.Element("audio").Element("codec").Value.Should().Be("truehd_atmos");
        }
    }
}
