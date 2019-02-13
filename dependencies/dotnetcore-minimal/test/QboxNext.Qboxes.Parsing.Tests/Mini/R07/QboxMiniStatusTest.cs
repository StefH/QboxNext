using NUnit.Framework;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Mini.R07
{
    /// <summary>
    ///This is a test class for QboxMiniStatusTest and is intended
    ///to contain all QboxMiniStatusTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class QboxMiniStatusTest
    {

        /// <summary>
        ///A test for Status
        ///</summary>
        [Test()]
        public void Status_Should_Be_WaitingForMeterType_Test()
        {
            byte p = 0;
            var target = new QboxMiniStatus(p);
            const MiniState expected = MiniState.Waiting;
            var actual = target.Status;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Status
        ///</summary>
        [Test()]
        public void Status_Should_Be_Operational_Test()
        {
            byte p = 7;
            var target = new QboxMiniStatus(p); 
            const MiniState expected = MiniState.Operational;
            var actual = target.Status;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for TimeIsReliable
        ///</summary>
        [Test()]
        public void TimeIsReliable_Should_Be_False_Test()
        {
            byte p = 0; 
            var target = new QboxMiniStatus(p); 
            const bool expected = false;
            var actual = target.TimeIsReliable;
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        ///A test for TimeIsReliable
        ///</summary>
        [Test()]
        public void TimeIsReliable_Should_Be_True_Test()
        {
            byte p = 7;
            var target = new QboxMiniStatus(p);
            const bool expected = true;
            var actual = target.TimeIsReliable;
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        ///A test for ValidResponse
        ///</summary>
        [Test()]
        public void ValidResponse_Should_Be_False_Test()
        {
            byte p = 2; 
            var target = new QboxMiniStatus(p); 
            const bool expected = false;
            var actual = target.ValidResponse;
            Assert.AreEqual(expected, actual);            
        }
    
        /// <summary>
        ///A test for ValidResponse
        ///</summary>
        [Test()]
        public void ValidResponse_Should_Be_true_Test()
        {
            byte p = 7; 
            var target = new QboxMiniStatus(p); 
            const bool expected = true;
            var actual = target.ValidResponse;
            Assert.AreEqual(expected, actual);
        }
    }
}
