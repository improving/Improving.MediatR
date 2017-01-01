namespace Improving.MediatR
{
    using System.Configuration;

    public static class MeasurementKey
    {
        public const string ENABLED      = "enabled";
        public const string MEASUREMENTS = "";
        public const string NAME         = "name";
        public const string THRESHOLD    = "threshold";
    }

    public interface IMeasurementConfig
    {
        bool                  Enabled      { get; set; }
        string                Name         { get; set; }
        int?                  Threshold    { get; set; }
        MeasurementCollection Measurements { get; }
    }

    public class MeasurementConfigurationSection : ConfigurationSection, IMeasurementConfig
    {

        [ConfigurationProperty(MeasurementKey.MEASUREMENTS, IsDefaultCollection = true)]
        public MeasurementCollection Measurements => (MeasurementCollection)base[MeasurementKey.MEASUREMENTS];

        [ConfigurationProperty(MeasurementKey.ENABLED, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this[MeasurementKey.ENABLED]; }
            set { this[MeasurementKey.ENABLED] = value; }
        }

        [ConfigurationProperty(MeasurementKey.THRESHOLD)]
        public int? Threshold
        {
            get { return (int?)this[MeasurementKey.THRESHOLD]; }
            set { this[MeasurementKey.THRESHOLD] = value; }
        }

        [ConfigurationProperty(MeasurementKey.NAME, IsKey = true)]
        public string Name
        {
            get { return (string)this[MeasurementKey.NAME]; }
            set { this[MeasurementKey.NAME] = value; }
        }
    }

    public class MeasurementCollection : ConfigurationElementCollection
    {
        public MeasurementElement this[int index]
        {
            get { return (MeasurementElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MeasurementElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MeasurementElement)element).Name;
        }

        public void Add(MeasurementCollection measurements)
        {
            foreach (var measurement in measurements)
            {
                Add((MeasurementElement)measurement);
            }
        }

        public void Add(ConfigurationElement element)
        {
            BaseAdd(element);
        }
    }

    public class MeasurementElement : ConfigurationElement, IMeasurementConfig
    {
        public MeasurementElement()
        {
        }

        public MeasurementElement(IMeasurementConfig config)
        {
            Enabled   = config.Enabled;
            Name      = config.Name;
            Threshold = config.Threshold;
            Measurements.Add(config.Measurements);
        }

        [ConfigurationProperty(MeasurementKey.NAME, IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this[MeasurementKey.NAME]; }
            set { this[MeasurementKey.NAME] = value; }
        }

        [ConfigurationProperty(MeasurementKey.THRESHOLD)]
        public int? Threshold
        {
            get { return (int?)this[MeasurementKey.THRESHOLD]; }
            set { this[MeasurementKey.THRESHOLD] = value; }
        }

        [ConfigurationProperty(MeasurementKey.MEASUREMENTS, IsDefaultCollection = true)]
        public MeasurementCollection Measurements => (MeasurementCollection)base[MeasurementKey.MEASUREMENTS];

        [ConfigurationProperty(MeasurementKey.ENABLED, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this[MeasurementKey.ENABLED]; }
            set { this[MeasurementKey.ENABLED] = value; }
        }
    }
}

