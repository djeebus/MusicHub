using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicHub.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.Core.Tests
{
    [TestClass]
    public class WhenOneInThreeHatesTheSong : JukeboxBaseTest
    {
        private string _userId = "userID";
        private string _songId = "songID";

        HateResult _result;
        [TestInitialize]
        public void Setup()
        {
            var jukebox = this.CreateJukebox();

            _result = jukebox.Hate(_userId, _songId, 3);
        }

        [TestMethod]
        public void LessThanHalf()
        {
            Assert.IsNotNull(_result);

            Assert.AreEqual(1, _result.HatersNeeded);
        }
    }
}
