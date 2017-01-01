namespace Improving.MediatR.Tests.MeasurementConfiguration
{
    using System.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MeasurementConfigurationTest
    {
        private MeasurementConfigurationSection measurementConfig;

        [TestInitialize]
        public void TestInitialize()
        {
            measurementConfig = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
                {
                    ExeConfigFilename = "MeasurementConfiguration\\valid.config"
                }, ConfigurationUserLevel.None)
                .GetSection("measurements") as MeasurementConfigurationSection;
        }

        [TestMethod]
        public void CanGetConfigSection()
        {
            Assert.IsNotNull(measurementConfig);
        }

        [TestMethod]
        public void SectionShouldHaveEnabledProperty()
        {
            Assert.IsTrue(measurementConfig.Enabled);
        }

        [TestMethod]
        public void SectionShouldHaveThresholdProperty()
        {
            Assert.AreEqual(2000, measurementConfig.Threshold);
        }

        [TestMethod]
        public void HasThreeMeasurements()
        {
            Assert.AreEqual(3, measurementConfig.Measurements.Count);
        }

        [TestMethod]
        public void CanNestMeasurements()
        {
            Assert.AreEqual(2, measurementConfig.Measurements[0].Measurements.Count);
        }

        [TestMethod]
        public void MeasurementElementHasEnabledProperty()
        {
            Assert.IsTrue(measurementConfig.Measurements[0].Enabled);
        }

    }
}
