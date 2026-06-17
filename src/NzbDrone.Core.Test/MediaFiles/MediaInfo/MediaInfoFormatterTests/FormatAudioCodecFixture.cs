using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatAudioCodecFixture : TestBase
    {
        private static string sceneName = "My.Series.S01E01-Sonarr";

        [TestCase("mp2, ,  ", "droned.s01e03.swedish.720p.hdtv.x264-prince", "MP2")]
        [TestCase("vorbis, ,  ", "DB Super HDTV", "Vorbis")]
        [TestCase("pcm_s16le, ,  ", "DW DVDRip XviD-idTV, ", "PCM")]
        [TestCase("truehd, ,  ", "", "TrueHD")]
        [TestCase("truehd, ,  ", "TrueHD", "TrueHD")]
        [TestCase("truehd, thd+,  ", "Atmos", "TrueHD Atmos")]
        [TestCase("truehd, thd+,  ", "TrueHD.Atmos.7.1", "TrueHD Atmos")]
        [TestCase("truehd, thd+,  ", "", "TrueHD Atmos")]
        [TestCase("wmav1, , ", "Droned.wmv", "WMA")]
        [TestCase("wmav2, , ", "B.N.S04E18.720p.WEB-DL", "WMA")]
        [TestCase("opus, ,  ", "Roadkill Ep3x11 - YouTube.webm", "Opus")]
        [TestCase("mp3, ,  ", "climbing.mp4", "MP3")]
        [TestCase("dts, , DTS-HD MA", "DTS-HD.MA", "DTS-HD MA")]
        [TestCase("dts, , DTS:X", "DTS-X", "DTS-X")]
        [TestCase("dts, , DTS-HD MA", "DTS-HD.MA", "DTS-HD MA")]
        [TestCase("dts, , DTS-ES", "DTS-ES", "DTS-ES")]
        [TestCase("dts, , DTS-ES", "DTS", "DTS-ES")]
        [TestCase("dts, , DTS-HD HRA", "DTSHD-HRA", "DTS-HD HRA")]
        [TestCase("dts, , DTS", "DTS", "DTS")]
        [TestCase("eac3, ec+3,  ", "EAC3.Atmos", "EAC3 Atmos")]
        [TestCase("eac3, ,  ", "DDP5.1", "EAC3")]
        [TestCase("ac3, ,  ", "DD5.1", "AC3")]
        [TestCase("adpcm_ima_qt, ,  ", "Custom?", "PCM")]
        [TestCase("adpcm_ms, ,  ", "Custom", "PCM")]

        public void should_format_audio_format(string audioFormatPack, string sceneName, string expectedFormat)
        {
            var split = audioFormatPack.Split(new string[] { ", " }, System.StringSplitOptions.None);
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = split[0],
                AudioCodecID = split[1],
                AudioProfile = split[2]
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel, sceneName).Should().Be(expectedFormat);
        }

        [Test]
        public void should_return_AudioFormat_by_default()
        {
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = "Other Audio Format",
                AudioCodecID = "Other Audio Codec"
            };

            MediaInfoFormatter.FormatAudioCodec(mediaInfoModel, sceneName).Should().Be(mediaInfoModel.AudioFormat);
        }

        [TestCase("truehd, ,  ", "truehd")]
        [TestCase("truehd, thd+,  ", "truehd_atmos")]
        [TestCase("truehd, , Dolby TrueHD + Dolby Atmos", "truehd_atmos")]
        [TestCase("eac3, ,  ", "eac3")]
        [TestCase("eac3, ec+3,  ", "eac3_ddp_atmos")]
        [TestCase("eac3, , Dolby Digital Plus + Dolby Atmos", "eac3_ddp_atmos")]
        [TestCase("dts, , DTS", "dts")]
        [TestCase("dts, , DTS-HD MA", "dtshd_ma")]
        [TestCase("dts, , DTS:X", "dtshd_ma_x")]
        [TestCase("dts, , DTS:X IMAX", "dtshd_ma_x_imax")]
        [TestCase("dts, , DTS-HD HRA", "dtshd_hra")]
        [TestCase("aac, , LC", "aac_lc")]
        [TestCase("aac, A_AAC/MPEG4/LC/SBR,  ", "he_aac")]
        [TestCase("aac, , HE-AACv2", "he_aac_v2")]
        [TestCase("aac, , SSR", "aac_ssr")]
        [TestCase("aac, , LTP", "aac_ltp")]
        [TestCase("ac3, ,  ", "ac3")]
        [TestCase("flac, ,  ", "flac")]
        [TestCase("mp3, ,  ", "mp3")]
        [TestCase("pcm_s16le, ,  ", "pcm_s16le")]
        [TestCase("wmav2, ,  ", "wmav2")]
        public void should_format_audio_codec_for_kodi_metadata(string audioFormatPack, string expectedFormat)
        {
            var split = audioFormatPack.Split(new string[] { ", " }, System.StringSplitOptions.None);
            var mediaInfoModel = new MediaInfoModel
            {
                AudioFormat = split[0],
                AudioCodecID = split[1],
                AudioProfile = split[2]
            };

            MediaInfoFormatter.FormatAudioCodecForKodiMetadata(mediaInfoModel).Should().Be(expectedFormat);
        }
    }
}
